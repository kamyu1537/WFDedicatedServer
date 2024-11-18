using System.Numerics;
using Steamworks;

namespace WFDS.Common.GameEvents.Events;

public class PlayerMessageEvent(CSteamID playerId, string message, string color, bool local, Vector3 position, string zone, long zoneOwner) : GameEvent
{
    public CSteamID PlayerId { get; init; } = playerId;
    public string Message { get; init; } = message;
    public string Color { get; init; } = color;
    public bool Local { get; init; } = local;
    public Vector3 Position { get; init; } = position;
    public string Zone { get; init; } = zone;
    public long ZoneOwner { get; init; } = zoneOwner;
}