using Steamworks;

namespace WFDS.Common.ChannelEvents.Events;

public class PlayerLeaveEvent(SteamId playerId) : ChannelEvent
{
    public SteamId PlayerId { get; init; } = playerId;
}
