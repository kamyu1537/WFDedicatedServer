using System.Numerics;
using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Actors;

public sealed class RainCloudActor : IActor
{
    public ILogger? Logger { get; set; }
    public IActorManager? ActorManager { get; set; }
    public IGameSessionManager? SessionManager { get; set; }

    public string ActorType => "raincloud";
    public long ActorId { get; init; }
    public SteamId CreatorId { get; init; }
    public string Zone { get; set; } = "main_zone";
    public long ZoneOwner { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay => true;
    public long DecayTimer { get; set; } = 32500;
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;
    
    public bool IsCanWipe => false;
    public bool IsDeadActor { get; set; } = false;
    public long NetworkShareDefaultCooldown => 8;
    public long NetworkShareCooldown { get; set; }

    public float Speed => 0.17f;
    public float Direction { get; set; }
}