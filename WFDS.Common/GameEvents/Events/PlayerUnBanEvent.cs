using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerUnBanEvent(CSteamID playerId) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
}