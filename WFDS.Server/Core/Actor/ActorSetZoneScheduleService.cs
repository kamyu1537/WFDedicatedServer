using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Server.Core.Network;

namespace WFDS.Server.Core.Actor;

internal class ActorSetZoneScheduleService(IActorManager actorManager, ISessionManager sessionManager, ILobbyManager lobby, SteamManager steam) : IHostedService
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
            
            sessionManager.BroadcastP2PPacket(lobby.GetLobbyId(), NetChannel.GameState, ActorActionPacket.CreateSetZonePacket(actor.ActorId, actor.Zone, actor.ZoneOwner));
        }
    }
}