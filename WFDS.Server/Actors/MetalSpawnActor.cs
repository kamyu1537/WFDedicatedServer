using System.Numerics;
using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WFDS.Server.Actors;

public sealed class MetalSpawnActor : IActor
{
    public string ActorType => "metal_spawn";
    public long ActorId { get; init; }
    public SteamId CreatorId { get; init; }
    public string Zone { get; set; } = "main_zone";
    public long ZoneOwner { get; set; } = -1;
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay => true;
    public long DecayTimer { get; set; } = 10000;
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

    public bool IsCanWipe => true;
    public bool IsDeadActor { get; set; }
    public long NetworkShareDefaultCooldown => 32;
    public long NetworkShareCooldown { get; set; }
}