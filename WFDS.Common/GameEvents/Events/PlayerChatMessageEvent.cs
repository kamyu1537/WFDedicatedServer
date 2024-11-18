using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerChatMessageEvent(CSteamID playerId, string message) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
    public string Message { get; init; } = message;
}