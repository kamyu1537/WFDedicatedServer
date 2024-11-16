using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerLeaveEvent(SteamId playerId) : GameEvent
{
    public SteamId PlayerId { get; init; } = playerId;
}
