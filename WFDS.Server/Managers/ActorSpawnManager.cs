using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Types;

namespace WFDS.Server.Managers;

public interface IActorSpawnManager
{
    void SpawnAmbientBirdActor();
    IActor? SpawnFishSpawnActor();
    IActor? SpawnFishSpawnAlienActor();
    IActor? SpawnRainCloudActor();
    IActor? SpawnVoidPortalActor();
    IActor? SpawnMetalActor();
}

public class ActorSpawnManager(ILogger<ActorSpawnManager> logger, IMapManager map, IActorManager actor) : IActorSpawnManager
{
    private readonly Random _random = new();
    
    public void SpawnAmbientBirdActor()
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
    
    public IActor? SpawnFishSpawnActor()
    {
        var point = map.FishSpawnPoints[_random.Next() % map.FishSpawnPoints.Count];
        return actor.SpawnFishSpawnActor(point.Transform.Origin);
    }

    public IActor? SpawnFishSpawnAlienActor()
    {
        var point = map.FishSpawnPoints[_random.Next() % map.FishSpawnPoints.Count];
        return actor.SpawnFishSpawnAlienActor(point.Transform.Origin);
    }

    public IActor? SpawnRainCloudActor()
    {
        var x = _random.NextSingle() * 250f - 100f;
        var z = _random.NextSingle() * 250f - 150f;
        var pos = new Vector3(x, 42f, z);

        return actor.SpawnRainCloudActor(pos);
    }

    public IActor? SpawnVoidPortalActor()
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
    
    public IActor? SpawnMetalActor()
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