using Steamworks;
using WFDS.Godot.Types;

namespace WFDS.Server.Common.Actor;

public class AmbientBirdActor : IActor
{
    public string ActorType => "ambient_bird";
    public long ActorId { get; init; }
    public SteamId CreatorId { get; init; }
    public string Zone { get; set; } = "main_zone";
    public long ZoneOwner { get; set; } = -1;
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay => true;
    public long DecayTimer { get; set; } = 600;
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

    public bool IsDeadActor { get; set; } = true;
    public long ActorUpdateDefaultCooldown => 0;
    public long ActorUpdateCooldown { get; set; }
    

    public void OnCreated()
    {
    }

    public void OnRemoved()
    {
    }

    public void OnUpdate(double delta)
    {
    }
}