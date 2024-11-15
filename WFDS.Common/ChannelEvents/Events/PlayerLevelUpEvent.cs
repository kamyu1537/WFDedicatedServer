using Steamworks;

namespace WFDS.Common.ChannelEvents.Events;

public class PlayerLevelUpEvent(SteamId playerId) : ChannelEvent
{
    public SteamId PlayerId { get; init; } = playerId;
}