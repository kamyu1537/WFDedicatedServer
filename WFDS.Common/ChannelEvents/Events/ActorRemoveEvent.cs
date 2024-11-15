namespace WFDS.Common.ChannelEvents.Events;

public class ActorRemoveEvent(long actorId) : ChannelEvent
{
    public long ActorId { get; init; } = actorId;
}