namespace WFDS.Common.ChannelEvents;

public abstract class ChannelEventHandler
{
    internal ChannelEventHandler()
    {
    }
    
    public abstract Type EventType { get; }
    public abstract Task HandleAsync(ChannelEvent e);
}

public abstract class ChannelEventHandler<T> : ChannelEventHandler where T : ChannelEvent
{
    public override Type EventType { get; } = typeof(T);

    public override async Task HandleAsync(ChannelEvent e)
    {
        if (e is T t)
        {
            await HandleAsync(t);
        }
    }

    protected abstract Task HandleAsync(T e);
}