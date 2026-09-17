using SubastaYa.Api.Services;

namespace SubastaYa.Api.Workers;

public class AuctionActivationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuctionActivationWorker> _logger;

    public AuctionActivationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<AuctionActivationWorker> logger)
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

                IAuctionActivationService service =
                    scope.ServiceProvider
                        .GetRequiredService<IAuctionActivationService>();

                await service.ProcessScheduledAuctionsAsync();

                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
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
                    "[CODE-ERROR] - error al activar subastas programadas.");
            }
        }
    }
}