using FSDS.Server.Common;
using FSDS.Server.Managers;
using FSDS.Server.Packets;
using Steamworks;

namespace FSDS.Server.Services;

public class RequestPingScheduleService(LobbyManager lobbyManager) : IHostedService
{
    private static readonly TimeSpan RepeatTime = TimeSpan.FromSeconds(8);
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, RepeatTime);
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
        lobbyManager.BroadcastPacket(NetChannel.GameState, new RequestPingPacket
        {
            Sender = SteamClient.SteamId,
        });
    }
}