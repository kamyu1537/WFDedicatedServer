using System.Numerics;
using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Actors;

public sealed class FishSpawnAlienActor : IActor
{
    public string ActorType => "fish_spawn_alien";
    public long ActorId { get; init; }
    public SteamId CreatorId { get; init; }
    public string Zone { get; set; } = "main_zone";
    public long ZoneOwner { get; set; } = -1;
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay => true;
    public long DecayTimer { get; set; } = 14400;
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

    public bool IsCanWipe => true;
    public bool IsDeadActor { get; set; }
    public long NetworkShareDefaultCooldown => 8;
    public long NetworkShareCooldown { get; set; }
}