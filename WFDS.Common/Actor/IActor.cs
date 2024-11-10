using Steamworks;
using WFDS.Common.Types;
using WFDS.Godot.Types;

namespace WFDS.Server.Common.Actor;

public interface IActor
{
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
    void OnCosmeticsUpdated(Cosmetics cosmetics);
    void OnHeldItemUpdated(GameItem item);
    void OnZoneUpdated(string zone, long zoneOwner);
    void OnActorUpdated(Vector3 position, Vector3 rotation);
}