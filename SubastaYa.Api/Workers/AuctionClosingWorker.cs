using SubastaYa.Api.Services;

namespace SubastaYa.Api.Workers;

public class AuctionClosingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuctionClosingWorker> _logger;

    public AuctionClosingWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<AuctionClosingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
    CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope =
                    _scopeFactory.CreateScope();

                IAuctionClosingService service =
                    scope.ServiceProvider
                        .GetRequiredService<IAuctionClosingService>();

                await service.ProcessExpiredAuctionsAsync();
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "[CODE-ERROR] - error general del worker de cierre.");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}