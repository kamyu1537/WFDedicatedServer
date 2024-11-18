using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerLeaveEvent(CSteamID playerId) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
}
