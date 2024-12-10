using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class LobbyCreatedEvent(CSteamID LobbyId) : GameEvent
{
    public CSteamID LobbyId { get; init; } = LobbyId;
}