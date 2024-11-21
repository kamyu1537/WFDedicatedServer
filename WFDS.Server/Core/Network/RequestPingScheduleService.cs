using System.Globalization;
using WFDS.Common.Actor;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;

namespace WFDS.Server.Core.Network;

internal class RequestPingScheduleService(LobbyManager lobby, SessionManager sessionManager, IActorManager actorManager, SteamManager steam) : IHostedService
{
    private static readonly TimeSpan RequestPingTimeoutPeriod = TimeSpan.FromSeconds(8);
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, RequestPingTimeoutPeriod);
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
        
        sessionManager.BroadcastP2PPacket(lobby.GetLobbyId(), NetChannel.GameState, new RequestPingPacket
        {
            Sender = steam.SteamId
        });
        
        foreach (var player in actorManager.GetPlayerActors())
        {
            if (player.ReceiveReplication)
            {
                continue;
            }

            sessionManager.SendP2PPacket(player.CreatorId, NetChannel.GameState, new RequestActorsPacket
            {
                UserId = steam.SteamId.m_SteamID.ToString(CultureInfo.InvariantCulture)
            });
        }
    }
}