using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Common.Network.Packets;

namespace WFDS.Server.Services;

public class RequestPingScheduleService(ISessionManager sessionManager) : IHostedService
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
    }
}