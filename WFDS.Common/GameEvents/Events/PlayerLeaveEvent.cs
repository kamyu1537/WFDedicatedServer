﻿using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerLeaveEvent(CSteamID playerId, string displayName) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
    public string DisplayName { get; init; } = displayName;
}
