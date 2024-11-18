using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerBanEvent(CSteamID playerId) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
}