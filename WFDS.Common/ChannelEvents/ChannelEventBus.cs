using System.Threading.Channels;

namespace WFDS.Common.ChannelEvents;

public static class ChannelEventBus
{
    internal static readonly Channel<ChannelEvent> Channel = System.Threading.Channels.Channel.CreateBounded<ChannelEvent>(new BoundedChannelOptions(1000)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false
    });
    
    internal static readonly SemaphoreSlim Semaphore = new(0);

    public static async Task PublishAsync(ChannelEvent e)
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