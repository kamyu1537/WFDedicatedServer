using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Types;

namespace WFDS.Common.Types;

public interface IActor : IDisposable
{
    ILogger? Logger { get; set; }
    IActorManager? ActorManager { get; set; }
    
    string ActorType { get; }
    long ActorId { get; init; }
    SteamId CreatorId { get; init; }
    string Zone { get; set; }
    long ZoneOwner { get; set; }
    Vector3 Position { get; set; }
    Vector3 Rotation { get; set; }

    bool Decay { get; }
    long DecayTimer { get; set; }
    DateTimeOffset CreateTime { get; set; }

    bool IsDeadActor { get; set; }
    long ActorUpdateDefaultCooldown { get; }
    long ActorUpdateCooldown { get; set; }

    void OnCreated();
    void OnRemoved(ActorRemoveTypes type);
    void OnUpdate(double delta);
    void OnZoneUpdated(string zone, long zoneOwner);
    void OnActorUpdated(Vector3 position, Vector3 rotation);
}