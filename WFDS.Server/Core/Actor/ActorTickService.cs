﻿using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Types.Manager;
using WFDS.Server.Core.ChannelEvent;

namespace WFDS.Server.Core.Actor;

internal sealed class ActorTickService(ILogger<ActorTickService> logger, IActorManager manager, ISessionManager session) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var prev = DateTime.UtcNow.Ticks;
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow.Ticks;
            var delta = (now - prev) / (double)TimeSpan.TicksPerSecond;
            prev = now;

            await UpdateAsync(delta);
            await ChannelEventBus.WaitAsync();
            await Task.Delay(1000 / 60, stoppingToken); // godot physics fps is 60
        }
    }

    private async Task UpdateAsync(double delta)
    {
        var actors = manager.GetActors();
        foreach (var actor in actors)
        {
            if (Decay(actor)) return;
            if (actor.IsDeadActor) return;

            await ChannelEventBus.PublishAsync(new ActorTickEvent(actor.ActorId, delta));
        }
    }

    private bool Decay(IActor actor)
    {
        if (actor.CreatorId != SteamClient.SteamId)
        {
            if (!session.IsSessionValid(actor.CreatorId))
            {
                actor.IsDeadActor = true;
                logger.LogInformation("remove actor {ActorId} {ActorType} (owner not found)", actor.ActorId, actor.Type);
                manager.TryRemoveActor(actor.ActorId, ActorRemoveTypes.OwnerNotFound, out _);
            }

            return false;
        }

        if (!actor.Decay) return false;
        actor.DecayTimer -= 1;

        if (actor.DecayTimer < 1)
        {
            actor.IsDeadActor = true;
            logger.LogInformation("decay actor {ActorId} {ActorType}", actor.ActorId, actor.Type);
            manager.TryRemoveActor(actor.ActorId, ActorRemoveTypes.Decay, out _);
            return true;
        }

        return false;
    }
}