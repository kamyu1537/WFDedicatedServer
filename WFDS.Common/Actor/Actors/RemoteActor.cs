using System.Numerics;
using Steamworks;

namespace WFDS.Common.Actor.Actors;

public sealed class RemoteActor : Actor<RemoteActor>
{
    public override ActorType Type => _actorType;
    public override long ActorId { get; set; }
    public override CSteamID CreatorId { get; set; }
    public override string Zone { get; set; } = string.Empty;
    public override long ZoneOwner { get; set; }
    public override Vector3 Position { get; set; } = Vector3.Zero;
    public override Vector3 Rotation { get; set; } = Vector3.Zero;
    public override bool Decay => _decay;
    public override long DefaultDecayTimer => 600;
    public override DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

    public override bool CanWipe => false;
    public override bool IsRemoved { get; set; }
    
    public override bool IsDead { get; set; } = true;
    public override long NetworkShareDefaultCooldown => 0;
    public override long NetworkShareCooldown { get; set; }

    private ActorType _actorType = ActorType.None;
    private bool _decay = true;

    public void SetActorType(ActorType actorType)
    {
        _actorType = actorType;
    }

    public void SetDecay(bool decay)
    {
        _decay = decay;
    }
    
    protected override void OnReset()
    {
        base.OnReset();
        
        _actorType = ActorType.None;
        _decay = true;
    }
}