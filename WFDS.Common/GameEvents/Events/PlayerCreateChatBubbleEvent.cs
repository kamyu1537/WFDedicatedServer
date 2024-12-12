using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerCreateChatBubbleEvent(CSteamID playerId, string message) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
    public string Message { get; init; } = message;
}