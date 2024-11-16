using System.Globalization;
using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Core.Network;

internal class RequestPingScheduleService(ISessionManager sessionManager, IActorManager actorManager) : IHostedService
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
        sessionManager.BroadcastP2PPacket(NetChannel.GameState, new RequestPingPacket
        {
            Sender = SteamClient.SteamId
        });
        
        foreach (var player in actorManager.GetPlayerActors())
        {
            if (player.ReceiveReplication)
            {
                continue;
            }

            sessionManager.SendP2PPacket(player.CreatorId, NetChannel.GameState, new RequestActorsPacket
            {
                UserId = SteamClient.SteamId.Value.ToString(CultureInfo.InvariantCulture)
            });
        }
    }
}