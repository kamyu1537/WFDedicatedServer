using System.Numerics;
using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.Actor.Actors;

public sealed class PlayerActor : IPlayerActor
{
    public ActorType Type => ActorType.Player;
    public long ActorId { get; init; }
    public CSteamID CreatorId { get; init; }
    public string Zone { get; set; } = "main_zone";
    public long ZoneOwner { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay => false;
    public long DecayTimer { get; set; }
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;
    
    public bool CanWipe => false;
    public bool IsRemoved { get; set; }
    
    public bool IsDead { get; set; } = true;
    public long NetworkShareDefaultCooldown => 0;
    public long NetworkShareCooldown { get; set; }

    public GameItem HeldItem { get; set; } = GameItem.Default;
    public Cosmetics Cosmetics { get; set; } = Cosmetics.Default;
    
    public bool ReceiveReplication { get; set; }
}