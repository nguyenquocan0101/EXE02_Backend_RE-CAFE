using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class ProductCustomizationRenderWorker : BackgroundService
    {
        private const int MaxFailureReasonLength = 1000;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ProductCustomizationRenderSettings _settings;
        private readonly ILogger<ProductCustomizationRenderWorker> _logger;

        public ProductCustomizationRenderWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<ProductCustomizationRenderSettings> settings,
            ILogger<ProductCustomizationRenderWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("ProductCustomizationRenderWorker is disabled by configuration.");
                return;
            }

            _logger.LogInformation("ProductCustomizationRenderWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOneQueuedCustomizationAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while processing customization queue.");
                }

                await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, _settings.PollIntervalSeconds)), stoppingToken);
            }
        }

        private async Task ProcessOneQueuedCustomizationAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var renderEngine = scope.ServiceProvider.GetRequiredService<IProductCustomizationRenderEngine>();

            var customization = await dbContext.ProductCustomizations
                .Include(c => c.Product)
                .Where(c => c.Status == ProductCustomizationStatus.Queued)
                .OrderBy(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (customization == null)
            {
                return;
            }

            customization.Status = ProductCustomizationStatus.Processing;
            customization.UpdatedAt = DateTime.UtcNow;
            customization.FailureReason = null;
            await dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                if (customization.Product == null)
                {
                    throw new InvalidOperationException("Customization product relation is missing.");
                }

                var (resultModelUrl, resultModelPublicId) = await renderEngine.RenderAndUploadAsync(
                    customization,
                    customization.Product,
                    cancellationToken);

                customization.Status = ProductCustomizationStatus.Completed;
                customization.IsMockResult = false;
                customization.ResultModelUrl = resultModelUrl;
                customization.ResultModelPublicId = resultModelPublicId;
                customization.CompletedAt = DateTime.UtcNow;
                customization.UpdatedAt = DateTime.UtcNow;
                customization.FailureReason = null;
                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Customization {CustomizationId} rendered successfully.", customization.Id);
            }
            catch (Exception ex)
            {
                customization.Status = ProductCustomizationStatus.Failed;
                customization.IsMockResult = false;
                customization.UpdatedAt = DateTime.UtcNow;
                customization.CompletedAt = null;
                customization.FailureReason = TrimFailureReason(ex.Message);
                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogError(ex, "Failed to render customization {CustomizationId}.", customization.Id);
            }
        }

        private static string TrimFailureReason(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "Rendering failed with unknown error.";
            }

            return message.Length <= MaxFailureReasonLength
                ? message
                : message.Substring(0, MaxFailureReasonLength);
        }
    }
}
