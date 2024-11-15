using System.Threading.Channels;

namespace WFDS.Server.Core.ChannelEvent;

internal static class ChannelEventBus
{
    internal static readonly Channel<WFDS.Common.ChannelEvents.ChannelEvent> Channel = System.Threading.Channels.Channel.CreateBounded<WFDS.Common.ChannelEvents.ChannelEvent>(new BoundedChannelOptions(1000)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false
    });

    private static readonly SemaphoreSlim Semaphore = new(0);

    public static async Task PublishAsync(WFDS.Common.ChannelEvents.ChannelEvent e)
    {
        try
        {
            Semaphore.Release();
            await Channel.Writer.WriteAsync(e);
        }
        catch (ChannelClosedException ex)
        {
            Console.WriteLine($"channel closed: {ex}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static async Task WaitAsync()
    {
        Semaphore.Release();
        await Semaphore.WaitAsync();
    }
}