using System.Numerics;
using Steamworks;

namespace WFDS.Common.Actor.Actors;

public sealed class RainCloudActor : Actor<RainCloudActor>
{
    public override ActorType Type => ActorType.RainCloud;
    public override long ActorId { get; set; }
    public override CSteamID CreatorId { get; set; }
    public override string Zone { get; set; } = "main_zone";
    public override long ZoneOwner { get; set; }
    public override Vector3 Position { get; set; } = Vector3.Zero;
    public override Vector3 Rotation { get; set; } = Vector3.Zero;
    public override bool Decay => true;
    public override DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;
    
    public override bool CanWipe => false;
    public override bool IsRemoved { get; set; }
    
    public override bool IsDead { get; set; } = false;
    public override long NetworkShareDefaultCooldown => 8;
    public override long NetworkShareCooldown { get; set; }

    public static float Speed => 0.17f;
    public float Direction { get; set; }

    protected override void OnReset()
    {
        base.OnReset();
        
        Direction = 0;
    }
}