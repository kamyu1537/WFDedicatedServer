using Steamworks;

namespace WFDS.Common.ChannelEvents.Events;

public class PlayerChatMessageEvent(SteamId playerId, string message) : ChannelEvent
{
    public SteamId PlayerId { get; init; } = playerId;
    public string Message { get; init; } = message;
}