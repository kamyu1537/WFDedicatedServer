using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.GameEvents.Events;

public class PlayerCosmeticsUpdateEvent(CSteamID playerId, Cosmetics cosmetics) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
    public Cosmetics Cosmetics { get; init; } = cosmetics;
}