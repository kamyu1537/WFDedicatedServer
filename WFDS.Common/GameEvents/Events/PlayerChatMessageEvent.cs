using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerChatMessageEvent(SteamId playerId, string message) : GameEvent
{
    public SteamId PlayerId { get; init; } = playerId;
    public string Message { get; init; } = message;
}