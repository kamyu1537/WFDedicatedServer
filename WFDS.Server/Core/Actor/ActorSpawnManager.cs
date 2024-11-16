using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Core.Actor;

internal sealed class ActorSpawnManager(ILogger<ActorSpawnManager> logger, IZoneManager zoneManager, IActorManager actor) : IActorSpawnManager
{
    private readonly Random _random = new();

    public void SpawnAmbientBirdActor()
    {
        var zone = zoneManager.GetZone();

        var count = _random.Next() % 3 + 1;
        for (var i = 0; i < count; i++)
        {
            if (TryPickRandomNode(zone.TrashPoints, out var point))
            {
                var x = _random.NextSingle() * 5f - 2.5f;
                var z = _random.NextSingle() * 5f - 2.5f;
                var pos = point.Transform.Origin + new Vector3(x, 0, z);

                actor.SpawnAmbientBirdActor(pos);
            }
            else
            {
                logger.LogError("no fish spawn point found");
            }
        }
    }

    public IActor? SpawnFishSpawnActor()
    {
        var zone = zoneManager.GetZone();
        if (TryPickRandomNode(zone.FishSpawnPoints, out var point))
        {
            return actor.SpawnFishSpawnActor(point.Transform.Origin);
        }
        
        logger.LogError("no fish spawn point found");
        return null;
    }

    public IActor? SpawnFishSpawnAlienActor()
    {
        var zone = zoneManager.GetZone();
        if (TryPickRandomNode(zone.FishSpawnPoints, out var point))
        {
            return actor.SpawnFishSpawnAlienActor(point.Transform.Origin);
        }

        logger.LogError("no fish spawn point found");
        return null;
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
        var zone = zoneManager.GetZone();
        if (TryPickRandomNode(zone.HiddenSpots, out var point))
        {
            var x = _random.NextSingle() - 0.5f;
            var z = _random.NextSingle() - 0.5f;
            var pos = point.Transform.Origin + new Vector3(x, 0, z);

            return actor.SpawnVoidPortalActor(pos);
        }

        logger.LogError("no hidden spot found");
        return null;
    }

    public IActor? SpawnMetalActor()
    {
        var point = RandomPickMetalPoint();
        if (point == null)
        {
            logger.LogError("no metal point found");
            return null;
        }

        var x = _random.NextSingle() - 0.5f;
        var z = _random.NextSingle() - 0.5f;
        var pos = point.Transform.Origin + new Vector3(x, 0, z);

        return actor.SpawnMetalActor(pos);
    }

    private PositionNode? RandomPickMetalPoint()
    {
        var zone = zoneManager.GetZone();
        if (_random.NextSingle() < 0.15)
        {
            if (TryPickRandomNode(zone.ShorelinePoints, out var point))
            {
                return point;
            }
        }
        else
        {
            if (TryPickRandomNode(zone.TrashPoints, out var point))
            {
                return point;
            }
        }

        return null;
    }

    private bool TryPickRandomNode(List<PositionNode> nodes, out PositionNode point)
    {
        point = default!;
        if (nodes.Count == 0)
        {
            logger.LogError("no node found");
            return false;
        }

        bool found;
        var repeat = 0;
        do
        {
            if (repeat++ > 10)
            {
                break;
            }

            point = nodes[_random.Next() % nodes.Count];
            found = actor.IsInSphereActor(ActorType.MetalSpawn, point.Transform.Origin, 1f);
        } while (found); // prevent spawn on top of each other

        return true;
    }
}