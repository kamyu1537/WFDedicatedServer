namespace WFDS.Common.GameEvents.Events;

public class ActorTickEvent(long actorId, double deltaTime) : GameEvent
{
    public long ActorId { get; init; } = actorId;
    public double DeltaTime { get; init; } = deltaTime;
}