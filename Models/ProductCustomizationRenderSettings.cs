namespace EXE02_Backend_RE_CAFE.Models
{
    public class ProductCustomizationRenderSettings
    {
        public bool Enabled { get; set; } = false;
        public int PollIntervalSeconds { get; set; } = 5;
        public int MaxProcessingSeconds { get; set; } = 240;
        public string BlenderExecutablePath { get; set; } = "blender";
        public string BlenderScriptPath { get; set; } = "scripts/3d/render_customization.py";
        public string WorkingDirectory { get; set; } = "tmp/customization-renders";
        public string OutputCloudinaryFolder { get; set; } = "recafe/customizations/result-models";
        public bool KeepTempFiles { get; set; } = false;
    }
}
