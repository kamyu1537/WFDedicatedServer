using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class NewPlayerJoinEvent(SteamId steamId) : GameEvent
{
    public SteamId SteamId { get; init; } = steamId;
}