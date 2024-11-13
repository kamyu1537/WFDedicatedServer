using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Types;

namespace WFDS.Server.Services;

public class HostSpawnScheduleService(ILogger<HostSpawnScheduleService> logger, IActorManager actor, IGameSessionManager session, IMapManager map) : IHostedService
{
    private const int DefaultAlienCooldown = 6;
    private const int ResetAlienCooldown = 16;

    private static readonly TimeSpan HostSpawnTimeoutPeriod = TimeSpan.FromSeconds(10);
    private readonly Random _random = new();
    private Timer? _timer;

    private int _alienCooldown = DefaultAlienCooldown;
    private float _rainChance;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _rainChance = _random.NextSingle() * 0.02f;

        _timer = new Timer(DoWork, null, TimeSpan.Zero, HostSpawnTimeoutPeriod);
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

        var ownedActorCount = actor.GetOwnedActorCount();
        var ownedActorTypes = actor.GetOwnedActorTypes();

        logger.LogInformation("owned_actors: {Count}, owned_actors_types: {Types}", ownedActorCount, string.Join(',', ownedActorTypes));

        var type = RandomPickSpawnType();
        IActor? spawn = null;
        switch (type)
        {
            case HostSpawnTypes.Fish:
                spawn = SpawnFishSpawnActor();
                break;
            case HostSpawnTypes.FishAlien:
                spawn = SpawnFishSpawnAlienActor();
                break;
            case HostSpawnTypes.Rain:
                spawn = SpawnRainCloudActor();
                break;
            case HostSpawnTypes.VoidPortal:
                spawn = SpawnVoidPortalActor();
                break;
            case HostSpawnTypes.None:
            default:
                break;
        }

        if (spawn == null)
        {
            return;
        }

        logger.LogInformation("spawn {ActorType} ({ActorId}) at {Pos}", spawn.ActorType, spawn.ActorId, spawn.Position);
    }

    private HostSpawnTypes RandomPickSpawnType()
    {
        var type = (HostSpawnTypes)(_random.Next() % 2);

        _alienCooldown -= 1;
        if (_random.NextSingle() <= 0.01 && _random.NextSingle() <= 0.4 && _alienCooldown <= 0)
        {
            type = HostSpawnTypes.FishAlien;
            _alienCooldown = ResetAlienCooldown;
        }

        if (_random.NextSingle() <= _rainChance && _random.NextSingle() <= 0.12f)
        {
            type = HostSpawnTypes.Rain;
            _rainChance = 0f;
        }
        else if (_random.NextSingle() < 0.75f)
        {
            _rainChance += 0.01f;
        }

        if (_random.NextSingle() <= 0.01 && _random.NextSingle() <= 0.25)
        {
            type = HostSpawnTypes.VoidPortal;
        }

        logger.LogDebug("select spawn type: {Type}", type);
        return type;
    }
    
    private IActor? SpawnFishSpawnActor()
    {
        var point = map.FishSpawnPoints[_random.Next() % map.FishSpawnPoints.Count];
        return actor.SpawnFishSpawnActor(point.Transform.Origin);
    }

    private IActor? SpawnFishSpawnAlienActor()
    {
        var point = map.FishSpawnPoints[_random.Next() % map.FishSpawnPoints.Count];
        return actor.SpawnFishSpawnAlienActor(point.Transform.Origin);
    }

    private IActor? SpawnRainCloudActor()
    {
        var x = _random.NextSingle() * 250f - 100f;
        var z = _random.NextSingle() * 250f - 150f;
        var pos = new Vector3(x, 42f, z);

        return actor.SpawnRainCloudActor(pos);
    }

    private IActor? SpawnVoidPortalActor()
    {
        if (map.HiddenSpots.Count == 0)
        {
            logger.LogError("no hidden_spots found");
            return null;
        }

        var point = map.HiddenSpots[_random.Next() % map.HiddenSpots.Count];
        var x = _random.NextSingle() - 0.5f;
        var z = _random.NextSingle() - 0.5f;
        var pos = point.Transform.Origin + new Vector3(x, 0, z);

        return actor.SpawnVoidPortalActor(pos);
    }
}