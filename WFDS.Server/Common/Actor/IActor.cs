using WFDS.Godot.Types;
using Steamworks;

namespace WFDS.Server.Common.Actor;

public interface IActor
{
    public string ActorType { get; set; }
    public long ActorId { get; set; }
    public SteamId CreatorId { get; set; }
    public string Zone { get; set; }
    public long ZoneOwner { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    public bool Decay { get; set; }
    public long DecayTimer { get; set; }
    public DateTimeOffset CreateTime { get; set; }

    public bool IsActorUpdated { get; set; }

    void OnCreated();
    void OnUpdate(double delta);
}