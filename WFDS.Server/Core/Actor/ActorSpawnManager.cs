using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Core.Actor;

internal sealed class ActorSpawnManager(ILogger<ActorSpawnManager> logger, IMapManager map, IActorManager actor) : IActorSpawnManager
{
    private readonly Random _random = new();

    public void SpawnAmbientBirdActor()
    {
        PositionNode point;
        bool found;
        do
        {
            point = map.TrashPoints[_random.Next() % map.TrashPoints.Count];
            found = actor.IsInSphereActor(ActorType.AmbientBird, point.Transform.Origin, 5f);
        } while (found); // prevent spawn on top of each other

        var count = _random.Next() % 3 + 1;
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
        PositionNode point;
        bool found;
        do
        {
            point = map.FishSpawnPoints[_random.Next() % map.FishSpawnPoints.Count];
            found = actor.IsInSphereActor(ActorType.FishSpawn, point.Transform.Origin, 1f);
        } while (found); // prevent spawn on top of each other

        return actor.SpawnFishSpawnActor(point.Transform.Origin);
    }

    public IActor? SpawnFishSpawnAlienActor()
    {
        PositionNode point;
        bool found;
        do
        {
            point = map.FishSpawnPoints[_random.Next() % map.FishSpawnPoints.Count];
            found = actor.IsInSphereActor(ActorType.FishSpawnAlien, point.Transform.Origin, 1f);
        } while (found); // prevent spawn on top of each other

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

        PositionNode point;
        bool found;
        do
        {
            point = map.HiddenSpots[_random.Next() % map.HiddenSpots.Count];
            found = actor.IsInSphereActor(ActorType.VoidPortal, point.Transform.Origin, 1f);
        } while (found); // prevent spawn on top of each other

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
        PositionNode point;
        bool found;

        if (_random.NextSingle() < 0.15)
        {
            do
            {
                point = map.ShorelinePoints[_random.Next() % map.ShorelinePoints.Count];
                found = actor.IsInSphereActor(ActorType.MetalSpawn, point.Transform.Origin, 1f);
            } while (found); // prevent spawn on top of each other
        }
        else
        {
            do
            {
                point = map.TrashPoints[_random.Next() % map.TrashPoints.Count];
                found = actor.IsInSphereActor(ActorType.MetalSpawn, point.Transform.Origin, 1f);
            } while (found); // prevent spawn on top of each other
        }

        return point;
    }
}