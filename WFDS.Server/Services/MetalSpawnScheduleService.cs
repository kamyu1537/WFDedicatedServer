using WFDS.Server.Managers;

namespace WFDS.Server.Services;

public class MetalSpawnScheduleService(ILogger<MetalSpawnScheduleService> logger, ActorManager actor, LobbyManager lobby) : IHostedService
{
    private static readonly TimeSpan Period = TimeSpan.FromSeconds(20);
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

    private void DoWork(object? state)
    {
        var count = lobby.GetSessionCount();
        if (count < 1) return;

        var metal = actor.SpawnMetalActor();
        if (metal != null)
        {
            logger.LogInformation("spawn {ActorType} ({ActorId}) at {Pos}", metal.ActorType, metal.ActorId, metal.Position);
        }
    }
}