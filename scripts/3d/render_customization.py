import argparse
import math
import os
import sys

import bpy


def parse_args():
    argv = sys.argv
    if "--" not in argv:
        return None

    argv = argv[argv.index("--") + 1 :]
    parser = argparse.ArgumentParser(description="Render product customization onto base GLB model.")
    parser.add_argument("--input-model", required=True)
    parser.add_argument("--input-image", required=True)
    parser.add_argument("--output-model", required=True)
    parser.add_argument("--position-x", type=float, default=0.0)
    parser.add_argument("--position-y", type=float, default=0.0)
    parser.add_argument("--position-z", type=float, default=0.0)
    parser.add_argument("--rotation-x", type=float, default=0.0)
    parser.add_argument("--rotation-y", type=float, default=0.0)
    parser.add_argument("--rotation-z", type=float, default=0.0)
    parser.add_argument("--scale", type=float, default=1.0)
    parser.add_argument("--engrave-depth", type=float, default=1.0)
    return parser.parse_args(argv)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def find_target_mesh():
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not meshes:
        raise RuntimeError("No mesh objects were found in imported model.")

    meshes.sort(key=lambda x: len(x.data.vertices), reverse=True)
    return meshes[0]


def create_decal_plane(args, target_mesh):
    plane_size = max(0.01, args.scale * 0.1)
    bpy.ops.mesh.primitive_plane_add(size=plane_size)
    plane = bpy.context.active_object
    plane.name = "CustomizationDecal"

    plane.location = (args.position_x, args.position_y, args.position_z)
    plane.rotation_euler = (
        math.radians(args.rotation_x),
        math.radians(args.rotation_y),
        math.radians(args.rotation_z),
    )

    # Project decal onto product surface
    shrinkwrap = plane.modifiers.new(name="ShrinkwrapToProduct", type="SHRINKWRAP")
    shrinkwrap.target = target_mesh
    shrinkwrap.wrap_method = "NEAREST_SURFACEPOINT"
    shrinkwrap.offset = -max(0.0, args.engrave_depth) * 0.0005

    # Give the decal a tiny thickness so it survives export/viewers
    solidify = plane.modifiers.new(name="SolidifyForExport", type="SOLIDIFY")
    solidify.thickness = 0.0005
    solidify.offset = 0

    return plane


def apply_image_material(plane, image_path):
    if not os.path.exists(image_path):
        raise RuntimeError(f"Source image not found: {image_path}")

    mat = bpy.data.materials.new(name="CustomizationDecalMat")
    mat.use_nodes = True
    mat.blend_method = "BLEND"
    mat.shadow_method = "NONE"

    nodes = mat.node_tree.nodes
    links = mat.node_tree.links

    # Reset default nodes
    for node in list(nodes):
        nodes.remove(node)

    output = nodes.new(type="ShaderNodeOutputMaterial")
    output.location = (400, 0)
    principled = nodes.new(type="ShaderNodeBsdfPrincipled")
    principled.location = (100, 0)
    tex = nodes.new(type="ShaderNodeTexImage")
    tex.location = (-200, 0)

    image = bpy.data.images.load(image_path, check_existing=True)
    tex.image = image

    links.new(tex.outputs["Color"], principled.inputs["Base Color"])
    if "Alpha" in tex.outputs:
        links.new(tex.outputs["Alpha"], principled.inputs["Alpha"])
    links.new(principled.outputs["BSDF"], output.inputs["Surface"])

    if plane.data.materials:
        plane.data.materials[0] = mat
    else:
        plane.data.materials.append(mat)


def ensure_output_dir(path):
    out_dir = os.path.dirname(path)
    if out_dir and not os.path.exists(out_dir):
        os.makedirs(out_dir, exist_ok=True)


def main():
    args = parse_args()
    if args is None:
        raise RuntimeError("No arguments provided for customization renderer.")

    if not os.path.exists(args.input_model):
        raise RuntimeError(f"Input model not found: {args.input_model}")

    clear_scene()
    bpy.ops.import_scene.gltf(filepath=args.input_model)

    target_mesh = find_target_mesh()
    plane = create_decal_plane(args, target_mesh)
    apply_image_material(plane, args.input_image)

    ensure_output_dir(args.output_model)
    bpy.ops.export_scene.gltf(
        filepath=args.output_model,
        export_format="GLB",
        export_apply=True,
        export_yup=True,
    )

    print(f"Customization render completed: {args.output_model}")


if __name__ == "__main__":
    main()
