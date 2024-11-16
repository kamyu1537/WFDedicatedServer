using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerLevelUpEvent(SteamId playerId) : GameEvent
{
    public SteamId PlayerId { get; init; } = playerId;
}