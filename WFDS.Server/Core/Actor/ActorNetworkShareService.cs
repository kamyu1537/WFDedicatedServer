using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Server.Core.Network;
using WFDS.Server.Core.Steam;

namespace WFDS.Server.Core.Actor;

internal sealed class ActorNetworkShareService(IActorManager actorManager, ISessionManager sessionManager, SteamManager steam) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Tick();
            await Task.Delay(1000 / 16, stoppingToken); // godot physics fps is 60
        }
    }

    private void Tick()
    {
        if (!steam.Initialized)
        {
            return;
        }
        
        var owned = actorManager.GetOwnedActors();
        foreach (var actor in owned)
        {
            NetworkShare(actor);
        }
    }

    // _network_share
    private void NetworkShare(IActor actor)
    {
        if (actor.IsDead || actor.IsRemoved)
            return;
        
        if (actor.CreatorId != SteamUser.GetSteamID())
            return;
        
        actor.NetworkShareCooldown -= 1;
        if (actor.NetworkShareCooldown > 0) return;
        
        actor.NetworkShareCooldown = actor.NetworkShareDefaultCooldown;
        actor.BroadcastInZone(NetChannel.ActorUpdate, ActorUpdatePacket.Create(actor), actorManager, sessionManager);
    }
}