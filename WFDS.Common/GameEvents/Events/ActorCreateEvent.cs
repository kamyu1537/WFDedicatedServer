namespace WFDS.Common.GameEvents.Events;

public class ActorCreateEvent(long actorId) : GameEvent
{
    public long ActorId { get; init; } = actorId;
}