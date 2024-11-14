using System.Numerics;
using Steamworks;
using WFDS.Common.Extensions;
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
    public bool IsDeadActor { get; set; } = false;
    public long NetworkShareDefaultCooldown => 8;
    public long NetworkShareCooldown { get; set; }
    
    private const float Speed = 0.17f;
    private float _direction;

    public void OnCreated()
    {
        var center = Vector3.Normalize(Position - new Vector3(30, 40, -50));
        _direction = new Vector2(center.X, center.Z).Angle();
    }
    
    public void OnRemoved(ActorRemoveTypes type)
    {
    }

    public void OnUpdate(double delta)
    {
        var vel = new Vector2(1, 0).Rotated(_direction) * Speed;
        Position += new Vector3(vel.X, 0f, vel.X) * (float)delta;
    }

    public void OnZoneUpdated(string zone, long zoneOwner)
    {
    }

    public void OnActorUpdated(Vector3 position, Vector3 rotation)
    {
    }

    public void Dispose()
    {
        Logger = null;
        ActorManager = null;
        SessionManager = null;
    }
}