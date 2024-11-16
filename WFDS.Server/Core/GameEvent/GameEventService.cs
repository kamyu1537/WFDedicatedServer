using WFDS.Common.GameEvents;

namespace WFDS.Server.Core.GameEvent;

internal sealed class GameEventService(IServiceProvider provider, ILogger<GameEventService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GameEventBus.ProcessQueueAsync(HandleEventAsync);
                await Task.Delay(10, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "failed to receive event");
            }
        }
        
        logger.LogInformation("game event service stopped");
    }

    private async Task HandleEventAsync(Common.GameEvents.GameEvent e)
    {
        try
        {
            await using var scope = provider.CreateAsyncScope();
            var handlers = provider.GetServices<GameEventHandler>();
            await Task.WhenAll(handlers
                .Where(x => x.EventType == e.GetType())
                .Select(x => x.HandleAsync(e)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "failed to handle event");
        }
    }
}