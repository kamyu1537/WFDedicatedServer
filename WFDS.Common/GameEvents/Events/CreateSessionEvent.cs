using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class CreateSessionEvent(CSteamID playerId) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
}