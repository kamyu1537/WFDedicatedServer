using System.Numerics;
using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.Actor.Actors;

public sealed class PlayerActor : Actor<PlayerActor>, IPlayerActor
{
    public override ActorType Type => ActorType.Player;
    public override long ActorId { get; set; }
    public override CSteamID CreatorId { get; set; }
    public override string Zone { get; set; } = "main_zone";
    public override long ZoneOwner { get; set; }
    public override Vector3 Position { get; set; } = Vector3.Zero;
    public override Vector3 Rotation { get; set; } = Vector3.Zero;
    public override bool Decay => false;
    public override DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;
    
    public override bool CanWipe => false;
    public override bool IsRemoved { get; set; }
    
    public override bool IsDead { get; set; } = true;
    public override long NetworkShareDefaultCooldown => 0;
    public override long NetworkShareCooldown { get; set; }

    public GameItem HeldItem { get; set; } = GameItem.Default;
    public Cosmetics Cosmetics { get; set; } = Cosmetics.Default;
    
    public bool ReceiveReplication { get; set; }

    protected override void OnReset()
    {
        base.OnReset();
        
        HeldItem = GameItem.Default;
        Cosmetics = Cosmetics.Default;
    }
}