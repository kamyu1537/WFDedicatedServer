namespace WFDS.Common.ChannelEvents.Events;

public class ActorCreateEvent(long actorId) : ChannelEvent
{
    public long ActorId { get; init; } = actorId;
}