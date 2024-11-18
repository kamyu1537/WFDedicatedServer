using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class NewPlayerJoinEvent(CSteamID steamId) : GameEvent
{
    public CSteamID SteamId { get; init; } = steamId;
}