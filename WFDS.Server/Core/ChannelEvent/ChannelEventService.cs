using WFDS.Common.ChannelEvents;

namespace WFDS.Server.Core.ChannelEvent;

internal class ChannelEventService(IServiceProvider provider, ILogger<ChannelEventService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var e in ChannelEventBus.Channel.Reader.ReadAllAsync(stoppingToken))
        {
            await HandleEventAsync(e);
            await ChannelEventBus.WaitAsync();
        }

        ChannelEventBus.Channel.Writer.Complete();
        logger.LogInformation("actor event service stopped");
    }

    private async Task HandleEventAsync(WFDS.Common.ChannelEvents.ChannelEvent e)
    {
        try
        {
            await using var scope = provider.CreateAsyncScope();
            var handlers = provider.GetServices<ChannelEventHandler>();
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