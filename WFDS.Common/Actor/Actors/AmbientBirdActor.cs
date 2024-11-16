using System.Numerics;
using Steamworks;

namespace WFDS.Common.Actor.Actors;

public sealed class AmbientBirdActor : IActor
{
    public ActorType Type => ActorType.AmbientBird;
    public long ActorId { get; init; }
    public SteamId CreatorId { get; init; }
    public string Zone { get; set; } = "main_zone";
    public long ZoneOwner { get; set; } = -1;
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay => true;
    public long DecayTimer { get; set; } = 600;
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

    public bool CanWipe => true;
    public bool IsRemoved { get; set; }
    
    public bool IsDead { get; set; } = true;
    public long NetworkShareDefaultCooldown => 32;
    public long NetworkShareCooldown { get; set; }
}