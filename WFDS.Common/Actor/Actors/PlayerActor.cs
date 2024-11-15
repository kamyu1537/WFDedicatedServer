using System.Numerics;
using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.Actor.Actors;

public sealed class PlayerActor : IPlayerActor
{
    public string ActorType => "player";
    public long ActorId { get; init; }
    public SteamId CreatorId { get; init; }
    public string Zone { get; set; } = "main_zone";
    public long ZoneOwner { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay => false;
    public long DecayTimer { get; set; }
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;
    
    public bool IsCanWipe => false;
    public bool IsDeadActor { get; set; } = true;
    public long NetworkShareDefaultCooldown => 0;
    public long NetworkShareCooldown { get; set; }

    public GameItem HeldItem { get; set; } = GameItem.Default;
    public Cosmetics Cosmetics { get; set; } = Cosmetics.Default;
}