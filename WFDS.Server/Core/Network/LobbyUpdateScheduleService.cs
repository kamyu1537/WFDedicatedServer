using WFDS.Common.Steam;

namespace WFDS.Server.Core.Network;

internal class LobbyUpdateScheduleService(LobbyManager lobby) : IHostedService
{
    private static readonly TimeSpan LobbyServerBrowserUpdateTimeoutPeriod = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan LobbyPingTimeoutPeriod = TimeSpan.FromSeconds(5);

    private Timer? _serverBrowserTimer;
    private Timer? _pingTimer;


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _serverBrowserTimer = new Timer(UpdateBrowserValue, null, TimeSpan.Zero, LobbyServerBrowserUpdateTimeoutPeriod);
        _pingTimer = new Timer(LobbyPing, null, TimeSpan.Zero, LobbyPingTimeoutPeriod);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _serverBrowserTimer?.Dispose();
        _pingTimer?.Dispose();

        _serverBrowserTimer = null;
        _pingTimer = null;

        return Task.CompletedTask;
    }

    private void UpdateBrowserValue(object? state)
    {
        lobby.UpdateBrowserValue();
    }

    private void LobbyPing(object? state)
    {
        lobby.UpdateTimestamp();
    }
}