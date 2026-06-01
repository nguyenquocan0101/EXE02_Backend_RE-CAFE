using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class BlenderProductCustomizationRenderEngine : IProductCustomizationRenderEngine
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ProductCustomizationRenderSettings _settings;
        private readonly ILogger<BlenderProductCustomizationRenderEngine> _logger;

        public BlenderProductCustomizationRenderEngine(
            IHttpClientFactory httpClientFactory,
            ICloudinaryService cloudinaryService,
            IOptions<ProductCustomizationRenderSettings> settings,
            ILogger<BlenderProductCustomizationRenderEngine> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cloudinaryService = cloudinaryService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<(string ResultModelUrl, string ResultModelPublicId)> RenderAndUploadAsync(
            ProductCustomization customization,
            Product product,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(product.Model3DUrl))
            {
                throw new InvalidOperationException("Product does not have a base 3D model URL.");
            }

            if (string.IsNullOrWhiteSpace(customization.SourceImageUrl))
            {
                throw new InvalidOperationException("Customization source image URL is missing.");
            }

            var workRoot = Path.GetFullPath(_settings.WorkingDirectory);
            var jobDir = Path.Combine(workRoot, customization.Id.ToString("N"));
            Directory.CreateDirectory(jobDir);

            var inputModelPath = Path.Combine(jobDir, "base-model.glb");
            var inputImagePath = Path.Combine(jobDir, "source-image.png");
            var outputModelPath = Path.Combine(jobDir, "result-model.glb");

            try
            {
                await DownloadToFileAsync(product.Model3DUrl, inputModelPath, cancellationToken);
                await DownloadToFileAsync(customization.SourceImageUrl, inputImagePath, cancellationToken);

                await RunBlenderAsync(customization, inputModelPath, inputImagePath, outputModelPath, cancellationToken);

                if (!File.Exists(outputModelPath))
                {
                    throw new InvalidOperationException("Blender finished but output model file was not found.");
                }

                var (url, publicId) = await _cloudinaryService.UploadRawFileAsync(
                    outputModelPath,
                    _settings.OutputCloudinaryFolder,
                    $"customization-{customization.Id:N}.glb");

                return (url, publicId);
            }
            finally
            {
                if (!_settings.KeepTempFiles)
                {
                    TryDeleteDirectory(jobDir);
                }
            }
        }

        private async Task DownloadToFileAsync(string url, string filePath, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            await using var responseStream = await client.GetStreamAsync(url, cancellationToken);
            await using var fileStream = File.Create(filePath);
            await responseStream.CopyToAsync(fileStream, cancellationToken);
        }

        private async Task RunBlenderAsync(
            ProductCustomization customization,
            string inputModelPath,
            string inputImagePath,
            string outputModelPath,
            CancellationToken cancellationToken)
        {
            var blenderScriptPath = Path.GetFullPath(_settings.BlenderScriptPath);
            if (!File.Exists(blenderScriptPath))
            {
                throw new FileNotFoundException("Blender renderer script not found.", blenderScriptPath);
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _settings.BlenderExecutablePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.ArgumentList.Add("--background");
            process.StartInfo.ArgumentList.Add("--python");
            process.StartInfo.ArgumentList.Add(blenderScriptPath);
            process.StartInfo.ArgumentList.Add("--");
            process.StartInfo.ArgumentList.Add("--input-model");
            process.StartInfo.ArgumentList.Add(inputModelPath);
            process.StartInfo.ArgumentList.Add("--input-image");
            process.StartInfo.ArgumentList.Add(inputImagePath);
            process.StartInfo.ArgumentList.Add("--output-model");
            process.StartInfo.ArgumentList.Add(outputModelPath);
            process.StartInfo.ArgumentList.Add("--position-x");
            process.StartInfo.ArgumentList.Add(ToInvariant(customization.PositionX));
            process.StartInfo.ArgumentList.Add("--position-y");
            process.StartInfo.ArgumentList.Add(ToInvariant(customization.PositionY));
            process.StartInfo.ArgumentList.Add("--position-z");
            process.StartInfo.ArgumentList.Add(ToInvariant(customization.PositionZ));
            process.StartInfo.ArgumentList.Add("--rotation-x");
            process.StartInfo.ArgumentList.Add(ToInvariant(customization.RotationX));
            process.StartInfo.ArgumentList.Add("--rotation-y");
            process.StartInfo.ArgumentList.Add(ToInvariant(customization.RotationY));
            process.StartInfo.ArgumentList.Add("--rotation-z");
            process.StartInfo.ArgumentList.Add(ToInvariant(customization.RotationZ));
            process.StartInfo.ArgumentList.Add("--scale");
            process.StartInfo.ArgumentList.Add(ToInvariant(customization.Scale));
            process.StartInfo.ArgumentList.Add("--engrave-depth");
            process.StartInfo.ArgumentList.Add(ToInvariant(customization.EngraveDepth));

            if (!process.Start())
            {
                throw new InvalidOperationException("Could not start Blender process.");
            }

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(1, _settings.MaxProcessingSeconds)));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var stdoutTask = process.StandardOutput.ReadToEndAsync(linkedCts.Token);
            var stderrTask = process.StandardError.ReadToEndAsync(linkedCts.Token);

            try
            {
                await process.WaitForExitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch
                {
                    // ignored
                }

                throw new TimeoutException($"Blender rendering exceeded timeout ({_settings.MaxProcessingSeconds}s).");
            }

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Blender render failed with exit code {process.ExitCode}. stderr: {stderr}");
            }

            _logger.LogInformation("Blender render completed for customization {CustomizationId}. Stdout: {Stdout}",
                customization.Id, stdout);
        }

        private static string ToInvariant(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }
            }
            catch
            {
                // no-op
            }
        }
    }
}
