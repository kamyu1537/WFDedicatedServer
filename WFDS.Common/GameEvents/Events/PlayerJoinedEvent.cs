using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerJoinedEvent(SteamId playerId) : GameEvent
{
    public SteamId PlayerId { get; init; } = playerId;
}