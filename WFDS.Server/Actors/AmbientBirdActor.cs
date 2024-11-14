﻿using System.Numerics;
using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Actors;

public sealed class AmbientBirdActor : IActor
{
    public ILogger? Logger { get; set; }
    public IActorManager? ActorManager { get; set; }
    public IGameSessionManager? SessionManager { get; set; }
    
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

    public bool IsDeadActor { get; set; }
    public long NetworkShareDefaultCooldown => 32;
    public long NetworkShareCooldown { get; set; }
    

    public void OnCreated()
    {
    }

    public void OnRemoved(ActorRemoveTypes type)
    {
    }

    public void OnUpdate(double delta)
    {
        var near = false;
        ActorManager?.SelectPlayerActors(player =>
        {
            var distance = Vector3.Distance(Position, player.Position);
            if (distance < 10)
            {
                Logger?.LogInformation("bird {ActorId} is near player {PlayerId}", ActorId, player.ActorId);
                near = true;
                return false;
            }

            return true;
        });

        if (near)
        {
            DecayTimer = 0;
        }
    }

    public void OnZoneUpdated(string zone, long zoneOwner)
    {
    }

    public void OnActorUpdated(Vector3 position, Vector3 rotation)
    {
    }

    public void Dispose()
    {
        Logger = null;
        ActorManager = null;
        SessionManager = null;
    }
}