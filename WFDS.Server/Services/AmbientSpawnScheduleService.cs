using WFDS.Server.Managers;

namespace WFDS.Server.Services;

public class AmbientSpawnScheduleService(ActorManager actor, LobbyManager lobby) : IHostedService
{
    private static readonly TimeSpan Period = TimeSpan.FromSeconds(10);
    private readonly Random _random = new();
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, Period);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_timer != null)
            await _timer.DisposeAsync();
    }

    // _on_ambient_spawn_timer_timeout
    private void DoWork(object? state)
    {
        var count = lobby.GetSessionCount();
        if (count < 1) return;
        
        var index = _random.Next() % 3;

        if (index == 2)
            actor.SpawnBird();
    }
}