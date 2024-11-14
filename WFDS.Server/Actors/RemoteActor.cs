﻿using System.Numerics;
using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Actors;

public sealed class RemoteActor : IActor
{
    public string ActorType { get; init; } = string.Empty;
    public long ActorId { get; init; }
    public SteamId CreatorId { get; init; }
    public string Zone { get; set; } = string.Empty;
    public long ZoneOwner { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay { get; init; }
    public long DecayTimer { get; set; } = 600;
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;

    public bool IsCanWipe => false;
    public bool IsDeadActor { get; set; } = true;
    public long NetworkShareDefaultCooldown => 0;
    public long NetworkShareCooldown { get; set; }
}