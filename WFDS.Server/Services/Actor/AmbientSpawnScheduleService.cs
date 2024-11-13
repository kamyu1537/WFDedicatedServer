using WFDS.Common.Types.Manager;
using WFDS.Godot.Types;

namespace WFDS.Server.Services;

public class AmbientSpawnScheduleService(IActorManager actor, IGameSessionManager session, IMapManager map) : IHostedService
{
    private static readonly TimeSpan AmbientSpawnTimeoutPeriod = TimeSpan.FromSeconds(10);
    private readonly Random _random = new();
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, AmbientSpawnTimeoutPeriod);
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
        var count = session.GetSessionCount();
        if (count < 1) return;

        var index = _random.Next() % 3;

        if (index == 2)
            SpawnAmbientBirdActor();
    }
    
    private void SpawnAmbientBirdActor()
    {
        var count = _random.Next() % 3 + 1;
        var point = map.TrashPoints[_random.Next() % map.TrashPoints.Count];

        for (var i = 0; i < count; i++)
        {
            var x = _random.NextSingle() * 5f - 2.5f;
            var z = _random.NextSingle() * 5f - 2.5f;
            var pos = point.Transform.Origin + new Vector3(x, 0, z);

            actor.SpawnAmbientBirdActor(pos);
        }
    }
}