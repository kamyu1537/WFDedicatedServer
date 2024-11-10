using Steamworks;
using WFDS.Common.Types;
using WFDS.Server.Common.Actor;
using WFDS.Server.Managers;

namespace WFDS.Server.Services;

public class ActorUpdateService(ILogger<ActorUpdateService> logger, ActorManager manager, LobbyManager lobby) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var prev = DateTime.UtcNow.Ticks;
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow.Ticks;
            var delta = (now - prev) / (double)TimeSpan.TicksPerSecond;
            prev = now;

            Update(delta);
            await Task.Delay(1000 / 60, stoppingToken); // godot physics fps is 60
        }
    }

    private void Update(double delta)
    {
        var count = lobby.GetSessionCount();
        if (count < 1) return;
        
        manager.SelectActors(actor =>
        {
            if (Decay(actor)) return;
            if (actor.IsDeadActor) return;
            
            actor.OnUpdate(delta);
        });
    }

    private bool Decay(IActor actor)
    {
        if (actor.CreatorId != SteamClient.SteamId)
        {
            if (!lobby.IsSessionExists(actor.CreatorId))
            {
                actor.IsDeadActor = true;
                logger.LogInformation("remove actor {ActorId} {ActorType} (owner not found)", actor.ActorId, actor.ActorType);
                manager.RemoveActor(actor.ActorId, ActorRemoveTypes.OwnerNotFound);
            }

            return false;
        }

        if (!actor.Decay) return false;
        actor.DecayTimer -= 1;

        if (actor.DecayTimer < 1)
        {
            actor.IsDeadActor = true;
            logger.LogInformation("decay actor {ActorId} {ActorType}", actor.ActorId, actor.ActorType);
            manager.RemoveActor(actor.ActorId, ActorRemoveTypes.Decay);
            return true;
        }

        return false;
    }
}