using WFDS.Common.GameEvents;
using ZLogger;

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
            catch (TaskCanceledException)
            {
                logger.ZLogInformation($"task canceled");
            }
            catch (Exception ex)
            {
                logger.ZLogError(ex, $"failed to receive event");
            }
        }
        
        logger.ZLogInformation($"game event service stopped");
    }

    private async Task HandleEventAsync(Common.GameEvents.GameEvent e)
    {
        try
        {
            await using var scope = provider.CreateAsyncScope();
            var handlers = scope.ServiceProvider.GetServices<GameEventHandler>();
            foreach (var handler in handlers.Where(x => x.EventType == e.GetType()))
            {
                handler.Handle(e);
            }
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"failed to handle event");
        }
    }
}