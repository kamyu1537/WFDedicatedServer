using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.ChannelEvents.Events;

public class PlayerHeldItemUpdateEvent(SteamId playerId, GameItem item) : ChannelEvent
{
    public SteamId PlayerId { get; init; } = playerId;
    public GameItem Item { get; init; } = item;
}