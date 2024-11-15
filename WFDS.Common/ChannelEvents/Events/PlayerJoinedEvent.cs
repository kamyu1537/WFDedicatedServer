using Steamworks;

namespace WFDS.Common.ChannelEvents.Events;

public class PlayerJoinedEvent(SteamId playerId) : ChannelEvent
{
    public SteamId PlayerId { get; init; } = playerId;
}