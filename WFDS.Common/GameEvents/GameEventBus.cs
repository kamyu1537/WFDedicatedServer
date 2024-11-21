using System.Collections.Concurrent;

namespace WFDS.Common.GameEvents;

public static class GameEventBus
{
    private static readonly ConcurrentQueue<GameEvent> Queue = new();

    public static void Publish(GameEvent e)
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

    public static async Task ProcessQueueAsync(Func<GameEvent, Task> action)
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