namespace WFDS.Common.GameEvents;

public abstract class GameEventHandler
{
    internal GameEventHandler()
    {
    }
    
    public abstract Type EventType { get; }
    public abstract void Handle(GameEvent e);
}

public abstract class GameEventHandler<T> : GameEventHandler
{
    public override Type EventType { get; } = typeof(T);

    public override void Handle(GameEvent e)
    {
        if (e is not T t) return;
        Handle(t);
    }

    protected abstract void Handle(T e);
}