using System.Collections.Concurrent;

namespace WFDS.Server.Core.GameEvent;

internal static class GameEventBus
{
    private static readonly ConcurrentQueue<Common.GameEvents.GameEvent> Queue = new();
    private static readonly ReaderWriterLockSlim Lock = new();

    public static void Publish(Common.GameEvents.GameEvent e)
    {
        try
        {
            Lock.EnterReadLock();
            Queue.Enqueue(e);
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
    
    public static Task WaitAsync()
    {
        TaskCompletionSource tcs = new();
        Task.Run(() =>
        {
            try
            {
                Console.WriteLine("wait"); // debug
                Lock.EnterReadLock();
                tcs.SetResult();
            }
            finally
            {
                Lock.ExitReadLock();
                Console.WriteLine("wait end");
            }
        });
        return tcs.Task;
    }

    public static async Task DequeueAsync(Func<Common.GameEvents.GameEvent, Task> action)
    {
        try
        {
            Lock.EnterWriteLock();
            Console.WriteLine("dequeue start");
            while (Queue.TryDequeue(out var e))
            {
                try
                {
                    await action(e);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        finally
        {
            Lock.ExitWriteLock();
            Console.WriteLine("dequeue end");
        }
    }
}