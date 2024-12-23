﻿using System.Numerics;
using Steamworks;

namespace WFDS.Common.Actor.Actors;

public sealed class VoidPortalActor : Actor<VoidPortalActor>
{
    public override ActorType Type => ActorType.VoidPortal;
    public override long ActorId { get; set; }
    public override CSteamID CreatorId { get; set; }
    public override string Zone { get; set; } = "main_zone";
    public override long ZoneOwner { get; set; } = -1;
    public override Vector3 Position { get; set; } = Vector3.Zero;
    public override Vector3 Rotation { get; set; } = Vector3.Zero;
    public override bool Decay => true;
    public override bool IsRemoved { get; set; }
    
    public override DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

    public override bool CanWipe => false;
    public override bool IsDead { get; set; }
    public override long NetworkShareDefaultCooldown => 32;
    public override long NetworkShareCooldown { get; set; }
}