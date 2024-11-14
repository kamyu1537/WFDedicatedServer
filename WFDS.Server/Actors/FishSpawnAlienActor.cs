using System.Numerics;
using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Types;

namespace WFDS.Server.Actors;

public sealed class FishSpawnAlienActor : IActor
{
    public ILogger? Logger { get; set; }
    public IActorManager? ActorManager { get; set; }
    public IGameSessionManager? SessionManager { get; set; }
    
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

    public bool IsDeadActor { get; set; }
    public long NetworkShareDefaultCooldown => 8;
    public long NetworkShareCooldown { get; set; }
    
    public void OnCreated()
    {
    }

    public void OnRemoved(ActorRemoveTypes type)
    {
    }
    
    public void OnUpdate(double delta)
    {
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