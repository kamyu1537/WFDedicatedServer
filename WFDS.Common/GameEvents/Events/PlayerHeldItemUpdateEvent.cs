using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.GameEvents.Events;

public class PlayerHeldItemUpdateEvent(SteamId playerId, GameItem item) : GameEvent
{
    public SteamId PlayerId { get; init; } = playerId;
    public GameItem Item { get; init; } = item;
}