﻿using WFDS.Common.Actor;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;

namespace WFDS.Server.Core.Actor;

internal class ActorSetZoneScheduleService(IActorManager actorManager, SteamManager steam, SessionManager session) : IHostedService
{
    private static readonly TimeSpan SetZoneTimeoutPeriod = TimeSpan.FromSeconds(5);
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, SetZoneTimeoutPeriod);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        _timer = null;
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        if (!steam.Initialized)
        {
            return;
        }
        
        foreach (var actor in actorManager.GetOwnedActors())
        {
            if (actor.IsDead) continue;
            if (actor.IsRemoved) continue;
            
            session.BroadcastPacket(NetChannel.GameState, ActorActionPacket.CreateSetZonePacket(actor.ActorId, actor.Zone, actor.ZoneOwner));
        }
    }
}