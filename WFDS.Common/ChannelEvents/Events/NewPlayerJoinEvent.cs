using Steamworks;

namespace WFDS.Common.ChannelEvents.Events;

public class NewPlayerJoinEvent(SteamId steamId) : ChannelEvent
{
    public SteamId SteamId { get; init; } = steamId;
}