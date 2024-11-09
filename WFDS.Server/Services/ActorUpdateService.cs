using Steamworks;
using WFDS.Server.Common.Actor;
using WFDS.Server.Managers;
using WFDS.Server.Packets;

namespace WFDS.Server.Services;

public class ActorUpdateService(ILogger<ActorUpdateService> logger, ActorManager manager, LobbyManager lobby) : BackgroundService
{
    private const int Delay = 1000 / 10;

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
        manager.SelectActors(actor =>
        {
            if (Decay(actor)) return;

            actor.OnUpdate(delta);

            if (!actor.IsActorUpdated) return;
            if (actor.CreatorId != SteamClient.SteamId) return;

            actor.IsActorUpdated = false;
            actor.SendActorUpdate(lobby);
        });
    }

    private bool Decay(IActor actor)
    {
        if (actor.CreatorId != SteamClient.SteamId)
        {
            if (!lobby.IsSessionExists(actor.CreatorId))
            {
                logger.LogInformation("remove actor {ActorId} {ActorType} (owner not found)", actor.ActorId, actor.ActorType);
                manager.RemoveActor(actor.ActorId);
            }

            return false;
        }

        if (!actor.Decay) return false;
        actor.DecayTimer -= 1;

        if (actor.DecayTimer < 1)
        {
            logger.LogInformation("decay actor {ActorId} {ActorType}", actor.ActorId, actor.ActorType);
            manager.RemoveActor(actor.ActorId);
            return true;
        }

        return false;
    }
}