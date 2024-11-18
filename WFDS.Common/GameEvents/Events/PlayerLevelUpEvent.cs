using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerLevelUpEvent(CSteamID playerId) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
}