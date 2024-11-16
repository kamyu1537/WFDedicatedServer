namespace WFDS.Common.GameEvents;

public abstract class GameEventHandler
{
    internal GameEventHandler()
    {
    }
    
    public abstract Type EventType { get; }
    public abstract Task HandleAsync(GameEvent e);
}

public abstract class GameEventHandler<T> : GameEventHandler where T : GameEvent
{
    public override Type EventType { get; } = typeof(T);

    public override async Task HandleAsync(GameEvent e)
    {
        if (e is T t)
        {
            await HandleAsync(t);
        }
    }

    protected abstract Task HandleAsync(T e);
}