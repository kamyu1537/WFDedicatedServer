using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Types;

namespace WFDS.Server.Services;

public class MetalSpawnScheduleService(ILogger<MetalSpawnScheduleService> logger, IActorManager actor, IGameSessionManager session, IMapManager map) : IHostedService
{
    private static readonly TimeSpan MetalSpawnTimeoutPeriod = TimeSpan.FromSeconds(20);
    private Timer? _timer;
    private readonly Random _random = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, MetalSpawnTimeoutPeriod);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_timer != null)
            await _timer.DisposeAsync();
    }

    private void DoWork(object? state)
    {
        var count = session.GetSessionCount();
        if (count < 1) return;

        var metal = SpawnMetalActor();
        if (metal != null)
        {
            logger.LogInformation("spawn {ActorType} ({ActorId}) at {Pos}", metal.ActorType, metal.ActorId, metal.Position);
        }
    }

    private IActor? SpawnMetalActor()
    {
        var point = RandomPickMetalPoint();
        var x = _random.NextSingle() - 0.5f;
        var z = _random.NextSingle() - 0.5f;
        var pos = point.Transform.Origin + new Vector3(x, 0, z);

        return actor.SpawnMetalActor(pos);
    }

    private PositionNode RandomPickMetalPoint()
    {
        if (_random.NextSingle() < 0.15)
        {
            return map.ShorelinePoints[_random.Next() % map.ShorelinePoints.Count];
        }

        return map.TrashPoints[_random.Next() % map.TrashPoints.Count];
    }
}