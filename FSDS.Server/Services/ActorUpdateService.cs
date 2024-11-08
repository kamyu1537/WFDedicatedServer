using FSDS.Server.Common.Actor;
using FSDS.Server.Managers;
using FSDS.Server.Packets;
using Steamworks;

namespace FSDS.Server.Services;

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
            await Task.Delay(Delay, stoppingToken);
        }
    }

    private void Update(double delta)
    {
        manager.UpdateOwnedActors(actor =>
        {
            actor.OnUpdate(delta);

            if (Decay(actor)) return;
            if (!actor.IsActorUpdated) return;
                
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

        var now = DateTimeOffset.UtcNow;
        var diff = now - actor.CreateTime;

        if (diff >= actor.DecayTime)
        {
            logger.LogInformation("decay actor {ActorId} {ActorType}", actor.ActorId, actor.ActorType);
            manager.RemoveActor(actor.ActorId);
            return true;
        }

        return false;
    }
}