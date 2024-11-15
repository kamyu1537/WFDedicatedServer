namespace WFDS.Common.ChannelEvents.Events;

public class ActorZoneUpdateEvent(long actorId, string zone, long zoneOwner) : ChannelEvent
{
    public long ActorId { get; init; } = actorId;
    public string Zone { get; init; } = zone;
    public long ZoneOwner { get; init; } = zoneOwner;
}