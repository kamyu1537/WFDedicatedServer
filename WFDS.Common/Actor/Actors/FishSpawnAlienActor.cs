using System.Numerics;
using Steamworks;

namespace WFDS.Common.Actor.Actors;

public sealed class FishSpawnAlienActor : IActor
{
    public ActorType Type => ActorType.FishSpawnAlien;
    public long ActorId { get; init; }
    public CSteamID CreatorId { get; init; }
    public string Zone { get; set; } = "main_zone";
    public long ZoneOwner { get; set; } = -1;
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay => true;
    public bool IsRemoved { get; set; }
    
    public long DecayTimer { get; set; } = 14400;
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

    public bool CanWipe => true;
    public bool IsDead { get; set; }
    public long NetworkShareDefaultCooldown => 8;
    public long NetworkShareCooldown { get; set; }
}