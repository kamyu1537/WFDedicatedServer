namespace WFDS.Common.ChannelEvents.Events;

public class ActorTickEvent(long actorId, double deltaTime) : ChannelEvent
{
    public long ActorId { get; init; } = actorId;
    public double DeltaTime { get; init; } = deltaTime;
}