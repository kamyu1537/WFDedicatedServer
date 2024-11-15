using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WFDS.Common.ChannelEvents;

public class ChannelEventService(IServiceProvider provider, ILogger<ChannelEventService> logger) : BackgroundService
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

    private async Task HandleEventAsync(ChannelEvent e)
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