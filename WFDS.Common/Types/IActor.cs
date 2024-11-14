﻿using System.Numerics;
using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.Types.Manager;

namespace WFDS.Common.Types;

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
    long NetworkShareDefaultCooldown { get; }
    long NetworkShareCooldown { get; set; }
}