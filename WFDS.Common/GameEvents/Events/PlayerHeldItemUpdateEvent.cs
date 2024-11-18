using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.GameEvents.Events;

public class PlayerHeldItemUpdateEvent(CSteamID playerId, GameItem item) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
    public GameItem Item { get; init; } = item;
}