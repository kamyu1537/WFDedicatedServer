using Steamworks;
using WFDS.Server.Common.Actor;
using WFDS.Server.Managers;
using WFDS.Server.Packets;

namespace WFDS.Server.Services;

public class ActorActionService(ActorManager manager, LobbyManager lobby) : BackgroundService
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
        var count = lobby.GetSessionCount();
        if (count < 1)
            return;
        
        manager.SelectOwnedActors(NetworkShare);
    }

    // _network_share
    private void NetworkShare(IActor actor)
    {
        if (actor.IsDeadActor)
            return;
        
        if (actor.CreatorId != SteamClient.SteamId.Value)
            return;
        
        actor.ActorUpdateCooldown -= 1;
        if (actor.ActorUpdateCooldown > 0) return;
        
        actor.ActorUpdateCooldown = actor.ActorUpdateDefaultCooldown;
        actor.SendUpdatePacket(lobby);
    }
}