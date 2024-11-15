using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.ChannelEvents.Events;

public class PlayerCosmeticsUpdateEvent(SteamId playerId, Cosmetics cosmetics) : ChannelEvent
{
    public SteamId PlayerId { get; init; } = playerId;
    public Cosmetics Cosmetics { get; init; } = cosmetics;
}