# FE + Ops Guide: 3D Auto Render Customization

## Tong quan
- Customer custom tren base model (`product.Model3DUrl`) va upload portrait image.
- Backend tao customization job voi `status = Queued`.
- Worker nen xu ly render tu dong qua Blender CLI.
- Xong thi cap nhat `resultModelUrl`, `status = Completed`.

## API customer su dung

### 1) Lay bootstrap de mo customizer
`GET /api/products/{productId}/customizations/bootstrap` (Bearer)

Tra ve:
- `baseModel3DUrl`: URL model 3D goc cua product.
- `customizations`: lich su custom cua user.

### 2) Tao customization job
`POST /api/products/{productId}/customizations` (Bearer, multipart/form-data)

Fields:
- `portraitImage` (required)
- `positionX`, `positionY`, `positionZ`
- `rotationX`, `rotationY`, `rotationZ`
- `scale`
- `engraveDepth`
- `note`

Sau khi tao:
- `status` se la `Queued`.
- FE can polling danh sach/chi tiet de doi trang thai.

### 3) Poll status
- `GET /api/products/{productId}/customizations`
- `GET /api/products/{productId}/customizations/{customizationId}`

`status` co the la:
- `Queued`
- `Processing`
- `Completed`
- `Failed`

Neu `Completed`:
- dung `resultModelUrl` de load file GLB ket qua.

Neu `Failed`:
- hien `failureReason` cho user va cho phep tao lai job.

## Cai dat backend de bat auto render

Them config:
```json
"ProductCustomizationRender": {
  "Enabled": true,
  "PollIntervalSeconds": 5,
  "MaxProcessingSeconds": 240,
  "BlenderExecutablePath": "blender",
  "BlenderScriptPath": "scripts/3d/render_customization.py",
  "WorkingDirectory": "tmp/customization-renders",
  "OutputCloudinaryFolder": "recafe/customizations/result-models",
  "KeepTempFiles": false
}
```

Yeu cau he thong:
- Server co cai Blender (CLI).
- Cloudinary config hop le.
- Server outbound duoc URL Cloudinary de download model/anh.

## Luu y ky thuat
- Worker render hien tai dung Blender script tao decal image bam theo be mat model va export GLB.
- Ban nay da tu dong hoa pipeline, khong can designer gan tay tung file.
- Neu muon "khac that geometry" sau nay, co the nang cap script Blender (displace/boolean) ma khong doi contract API FE.
