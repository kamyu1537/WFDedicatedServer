using System.Threading.Channels;

namespace WFDS.Server.Core.ChannelEvent;

internal static class ChannelEventBus
{
    internal static readonly Channel<WFDS.Common.ChannelEvents.ChannelEvent> Channel = System.Threading.Channels.Channel.CreateUnbounded<WFDS.Common.ChannelEvents.ChannelEvent>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    private static readonly SemaphoreSlim Semaphore = new(0);
    private static readonly ReaderWriterLockSlim Lock = new();

    public static async Task PublishAsync(WFDS.Common.ChannelEvents.ChannelEvent e)
    {
        try
        {
            Lock.EnterReadLock();
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
        finally
        {
            Lock.ExitReadLock();
        }
    }

    public static async Task WaitAsync()
    {
        try
        {
            Lock.EnterWriteLock();
            Semaphore.Release();
            await Semaphore.WaitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }
}