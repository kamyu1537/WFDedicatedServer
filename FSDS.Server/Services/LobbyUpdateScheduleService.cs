using FSDS.Server.Managers;

namespace FSDS.Server.Services;

public class LobbyUpdateScheduleService(LobbyManager lobby) : IHostedService
{
    private static readonly TimeSpan RepeatTime = TimeSpan.FromSeconds(30);
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
        lobby.UpdateBrowserValue();
        lobby.KickNoHandshakePlayers();
    }
}