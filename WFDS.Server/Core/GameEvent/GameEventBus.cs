using System.Collections.Concurrent;

namespace WFDS.Server.Core.GameEvent;

internal static class GameEventBus
{
    private static readonly ConcurrentQueue<Common.GameEvents.GameEvent> Queue = new();

    public static void Publish(Common.GameEvents.GameEvent e)
    {
        Queue.Enqueue(e);
    }

    public static Task WaitEmptyAsync()
    {
        var tcs = new TaskCompletionSource();
        Task.Run(async () =>
        {
            while (!Queue.IsEmpty)
            {
                await Task.Delay(10);
            }
            tcs.SetResult();
        });
        
        return tcs.Task;
    }

    public static async Task ProcessQueueAsync(Func<Common.GameEvents.GameEvent, Task> action)
    {
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
}