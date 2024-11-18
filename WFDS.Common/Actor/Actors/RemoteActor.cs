using System.Numerics;
using Steamworks;

namespace WFDS.Common.Actor.Actors;

public sealed class RemoteActor : IActor
{
    public ActorType Type { get; init; } = ActorType.None;
    public long ActorId { get; init; }
    public CSteamID CreatorId { get; init; }
    public string Zone { get; set; } = string.Empty;
    public long ZoneOwner { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay { get; init; }
    public long DecayTimer { get; set; } = 600;
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

    public bool CanWipe => false;
    public bool IsRemoved { get; set; }
    
    public bool IsDead { get; set; } = true;
    public long NetworkShareDefaultCooldown => 0;
    public long NetworkShareCooldown { get; set; }
}