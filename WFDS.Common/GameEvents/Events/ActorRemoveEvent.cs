using Steamworks;
using WFDS.Common.Actor;

namespace WFDS.Common.GameEvents.Events;

public class ActorRemoveEvent(long actorId, ActorType actorType, CSteamID ownerId, ActorRemoveTypes removeType) : GameEvent
{
    public long ActorId { get; init; } = actorId;
    public ActorType Type { get; init; } = actorType;
    public CSteamID OwnerId { get; init; } = ownerId;
    public ActorRemoveTypes RemoveType { get; init; } = removeType;
}