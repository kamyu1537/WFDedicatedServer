using System.Numerics;
using Steamworks;

namespace WFDS.Common.Actor.Actors;

public sealed class FishSpawnActor : IActor
{
    public ActorType Type => ActorType.FishSpawn;
    public long ActorId { get; init; }
    public CSteamID CreatorId { get; init; }
    public string Zone { get; set; } = "main_zone";
    public long ZoneOwner { get; set; } = -1;
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay => true;
    public long DecayTimer { get; set; } = 4800;
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

    public bool CanWipe => true;
    public bool IsRemoved { get; set; }
    
    public bool IsDead { get; set; }
    public long NetworkShareDefaultCooldown => 32;
    public long NetworkShareCooldown { get; set; }
}