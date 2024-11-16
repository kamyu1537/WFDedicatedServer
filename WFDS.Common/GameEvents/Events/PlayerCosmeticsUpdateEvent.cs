using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.GameEvents.Events;

public class PlayerCosmeticsUpdateEvent(SteamId playerId, Cosmetics cosmetics) : GameEvent
{
    public SteamId PlayerId { get; init; } = playerId;
    public Cosmetics Cosmetics { get; init; } = cosmetics;
}