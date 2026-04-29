using BitrixJiraConnector.Api.Services.Interfaces;

namespace BitrixJiraConnector.Api.BackgroundServices;

public class DealScanBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DealScanBackgroundService> _logger;

    public DealScanBackgroundService(IServiceScopeFactory scopeFactory, ILogger<DealScanBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DealScanBackgroundService started");
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbService = scope.ServiceProvider.GetRequiredService<IDbService>();
            var processingService = scope.ServiceProvider.GetRequiredService<IDealProcessingService>();

            int intervalMinutes = await dbService.GetScanIntervalMinutesAsync();

            await processingService.ScanAndProcessAllDealsAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }
}
