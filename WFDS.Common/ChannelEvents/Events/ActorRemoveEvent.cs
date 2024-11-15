using Steamworks;
using WFDS.Common.Actor;

namespace WFDS.Common.ChannelEvents.Events;

public class ActorRemoveEvent(long actorId, ActorType actorType, SteamId ownerId, ActorRemoveTypes removeType) : ChannelEvent
{
    public long ActorId { get; init; } = actorId;
    public ActorType Type { get; init; } = actorType;
    public SteamId OwnerId { get; init; } = ownerId;
    public ActorRemoveTypes RemoveType { get; init; } = removeType;
}