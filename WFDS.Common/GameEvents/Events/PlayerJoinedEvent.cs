using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerJoinedEvent(CSteamID playerId) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
}