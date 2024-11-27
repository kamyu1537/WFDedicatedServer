using Cysharp.Threading;
using WFDS.Common.Actor;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Steam;
using WFDS.Server.Core.Network;
using ZLogger;

namespace WFDS.Server.Core.Actor;

internal sealed class ActorTickService(ILogger<ActorTickService> logger, IActorManager manager, SteamManager steam, SessionManager session) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = LogicLooperPool.Shared.RegisterActionAsync(Update, LooperActionOptions.Default with { TargetFrameRateOverride = 60 }).ConfigureAwait(false);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.ZLogInformation($"ActorTickService stopped");
        return Task.CompletedTask;
    }

    private bool Update(in LogicLooperActionContext ctx)
    {
        if (ctx.CancellationToken.IsCancellationRequested)
        {
            return false;
        }
        
        if (!steam.Initialized)
        {
            return true;
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
        if (actor.CreatorId != steam.SteamId)
        {
            if (!session.IsSessionValid(actor.CreatorId))
            {
                actor.IsDead = true;
                actor.IsRemoved = true;

                logger.ZLogInformation($"remove actor {actor.ActorId} {actor.Type} (owner not found)");
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

            logger.ZLogInformation($"decay actor {actor.ActorId} {actor.Type}");
            manager.TryRemoveActor(actor.ActorId, ActorRemoveTypes.Decay, out _);
            return true;
        }

        return false;
    }
}