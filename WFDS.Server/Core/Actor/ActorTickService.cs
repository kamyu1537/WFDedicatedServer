using Cysharp.Threading;
using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Types.Manager;
using WFDS.Server.Core.GameEvent;

namespace WFDS.Server.Core.Actor;

internal sealed class ActorTickService(ILogger<ActorTickService> logger, IActorManager manager, ISessionManager session) : BackgroundService
{
    private readonly LogicLooper _looper = new(60);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _looper.RegisterActionAsync(Update).ConfigureAwait(false);
        Console.WriteLine("ActorTickService stopped");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _looper.Dispose();
        return base.StopAsync(cancellationToken);
    }

    private bool Update(in LogicLooperActionContext ctx)
    {
        if (ctx.CancellationToken.IsCancellationRequested)
        {
            return false;
        }

        var delta = ctx.ElapsedTimeFromPreviousFrame.TotalMilliseconds / 1000d;
        var actors = manager.GetActors();
        foreach (var actor in actors)
        {
            if (Decay(actor)) continue;
            if (actor.IsRemoved) continue;
            GameEventBus.Publish(new ActorTickEvent(actor.ActorId, delta));
        }

        return true;
    }

    private bool Decay(IActor actor)
    {
        if (actor.CreatorId != SteamClient.SteamId)
        {
            if (!session.IsSessionValid(actor.CreatorId))
            {
                actor.IsDead = true;
                actor.IsRemoved = true;

                logger.LogInformation("remove actor {ActorId} {ActorType} (owner not found)", actor.ActorId, actor.Type);
                manager.TryRemoveActor(actor.ActorId, ActorRemoveTypes.OwnerNotFound, out _);
                return false;
            }

            return true;
        }

        if (!actor.Decay) return false;
        actor.DecayTimer -= 1;

        if (actor.DecayTimer < 1)
        {
            actor.IsDead = true;
            actor.IsRemoved = true;

            logger.LogInformation("decay actor {ActorId} {ActorType}", actor.ActorId, actor.Type);
            manager.TryRemoveActor(actor.ActorId, ActorRemoveTypes.Decay, out _);
            return true;
        }

        return false;
    }
}