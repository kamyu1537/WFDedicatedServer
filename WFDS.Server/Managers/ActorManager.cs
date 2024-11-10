using System.Collections.Concurrent;
using Steamworks;
using WFDS.Godot.Types;
using WFDS.Server.Common;
using WFDS.Server.Common.Actor;
using WFDS.Server.Common.Network;
using WFDS.Server.Common.Types;
using WFDS.Server.Packets;

namespace WFDS.Server.Managers;

public sealed class ActorManager(
    ILogger<ActorManager> logger,
    MapManager map,
    LobbyManager lobby,
    ActorIdManager id)
    : IDisposable
{
    private const string Zone = "main_zone";
    private readonly Random _random = new();

    private readonly ConcurrentDictionary<long, IActor> _owned = [];
    private readonly ConcurrentDictionary<long, IActor> _actors = [];
    private readonly ConcurrentDictionary<SteamId, PlayerActor> _players = [];

    public void Dispose()
    {
        _owned.Clear();
        _actors.Clear();
        _players.Clear();
    }


    public void SendAllOwnedActors(Session target)
    {
        foreach (var actor in _owned.Values)
        {
            actor.SendInstanceActor(target);
            actor.SendUpdatePacket(target);
        }
    }

    public void SelectActor(long actorId, Action<IActor> update)
    {
        if (!_actors.TryGetValue(actorId, out var actor))
        {
            return;
        }

        update(actor);
    }

    public void SelectActors(Action<IActor> action)
    {
        foreach (var actor in _actors.Values)
        {
            action(actor);
        }
    }

    public void SelectOwnedActors(Action<IActor> action)
    {
        foreach (var actor in _owned.Values)
        {
            action(actor);
        }
    }

    public int GetOwnedActorCount()
    {
        return _owned.Count;
    }

    public List<string> GetOwnedActorTypes()
    {
        return _owned.Values.Select(actor => actor.ActorType).ToList();
    }

    private void AddActorAndPropagate(IActor actor)
    {
        actor.OnCreated();
        _actors.TryAdd(actor.ActorId, actor);

        if (actor.ActorType == "player")
        {
            if (!_players.TryAdd(actor.CreatorId, (PlayerActor)actor))
            {
                logger.LogError("player already exists");
            }
        }
        else if (actor.CreatorId == SteamClient.SteamId.Value)
        {
            if (_owned.TryAdd(actor.ActorId, actor))
            {
                actor.SendInstanceActor(lobby);
                actor.SendUpdatePacket(lobby);
            }
            else
            {
                logger.LogError("actor already exists");
            }
        }
    }

    private static void SetActorDefaultValues(IActor actor)
    {
        actor.Zone = Zone;
        actor.ZoneOwner = -1;
        actor.CreateTime = DateTimeOffset.UtcNow;
    }

    private BaseActor CreateHostActor(string actorType, Vector3 position)
    {
        var actor = new BaseActor
        {
            ActorType = actorType,
            ActorId = id.Next(),
            CreatorId = SteamClient.SteamId,
            Position = position,
            Rotation = Vector3.Zero
        };

        SetActorDefaultValues(actor);
        AddActorAndPropagate(actor);
        return actor;
    }

    public bool CreatePlayerActor(SteamId playerId, long actorId, string name, out PlayerActor actor)
    {
        actor = null!;

        if (!id.Add(actorId))
        {
            logger.LogError("actor id already exists {ActorId} {ActorType}", actorId, "player");
            return false;
        }

        if (_players.ContainsKey(playerId))
        {
            logger.LogError("player already exists {SteamId}", playerId);
            return false;
        }

        actor = new PlayerActor
        {
            ActorType = "player",
            ActorId = actorId,
            CreatorId = playerId,
            Zone = Zone,
            ZoneOwner = -1,
            Position = Vector3.Zero,
            Rotation = Vector3.Zero,
            Name = name
        };

        SetActorDefaultValues(actor);
        AddActorAndPropagate(actor);
        return true;
    }

    public bool CreateRemoteActor(SteamId owner, long actorId, string actorType)
    {
        if (!id.Add(actorId))
        {
            logger.LogError("actor id already exists {ActorId} {ActorType}", actorId, actorType);
            return false;
        }

        var actor = new BaseActor
        {
            ActorType = actorType,
            ActorId = actorId,
            CreatorId = owner,
            Zone = Zone,
            ZoneOwner = -1,
            Position = Vector3.Zero,
            Rotation = Vector3.Zero
        };

        SetActorDefaultValues(actor);
        AddActorAndPropagate(actor);
        return true;
    }

    public void RemoveActor(long actorId)
    {
        if (!_actors.TryRemove(actorId, out var actor))
        {
            return;
        }

        if (actor.ActorType == "player")
        {
            _players.TryRemove(actor.CreatorId, out _);
        }

        id.Return(actorId);

        var wipe = ActorActionPacket.CreateWipeActorPacket(actorId);
        lobby.BroadcastPacket(NetChannel.ActorAction, wipe);

        if (actor.CreatorId.Value != SteamClient.SteamId.Value)
        {
            return;
        }

        if (!_owned.TryRemove(actorId, out _))
        {
            return;
        }
        
        var queue = ActorActionPacket.CreateQueueFreePacket(actor.ActorId);
        lobby.BroadcastPacket(NetChannel.ActorAction, queue);
    }

    private int GetActorCountByType(string actorType)
    {
        return _owned.Values.Count(actor => actor.ActorType == actorType);
    }

    public void RemoveActorFirstByType(string actorType)
    {
        var actor = _owned.Values.FirstOrDefault(a => a.ActorType == actorType);
        if (actor == null)
        {
            return;
        }

        RemoveActor(actor.ActorId);
    }

    public void SpawnBird()
    {
        var count = _random.Next() % 3 + 1;
        var point = map.TrashPoints[_random.Next() % map.TrashPoints.Count];

        for (var i = 0; i < count; i++)
        {
            var x = _random.NextSingle() * 5f - 2.5f;
            var z = _random.NextSingle() * 5f - 2.5f;
            var pos = point.Transform.Origin + new Vector3(x, 0, z);

            SpawnBird(pos);
        }
    }

    public void SpawnBird(Vector3 pos)
    {
        var actorCount = GetActorCountByType("ambient_bird");
        if (actorCount >= 10)
        {
            logger.LogError("ambient_bird limit reached (10)");
            return;
        }
        
        var bird = CreateHostActor("ambient_bird", pos);
        bird.Decay = true;
        bird.DecayTimer = 600; // default?
        bird.IsDeadActor = true;

        logger.LogInformation("spawn bird at {ActorId} - {Pos}", bird.ActorId, bird.Position);
    }

    public void SpawnFish(string type = "fish_spawn")
    {
        var point = map.FishSpawnPoints[_random.Next() % map.FishSpawnPoints.Count];
        SpawnFish(point.Transform.Origin, type);
    }

    public void SpawnFish(Vector3 pos, string type = "fish_spawn")
    {
        var actorCount = GetActorCountByType(type);
        if (actorCount >= 7)
        {
            RemoveActorFirstByType(type);
        }

        var fish = CreateHostActor(type, pos);
        fish.Decay = true;
        fish.DecayTimer = type == "fish_spawn_alien" ? 4800 : 14400;
        fish.ActorUpdateDefaultCooldown = type == "fish_spawn_alien" ? 8 : 32;

        logger.LogInformation("spawn {ActorType} ({ActorId}) at {Pos}", fish.ActorType, fish.ActorId, fish.Position);
    }

    public void SpawnRainCloud()
    {
        var x = _random.NextSingle() * 250f - 100f;
        var z = _random.NextSingle() * 250f - 150f;
        var pos = new Vector3(x, 42f, z);

        SpawnRainCloud(pos);
    }

    public void SpawnRainCloud(Vector3 pos)
    {
        if (_owned.Values.Any(actor => actor.ActorType == "raincloud"))
        {
            logger.LogError("raincloud already exists");
            return;
        }

        var actor = new RainCloudActor
        {
            ActorType = "raincloud",
            ActorId = id.Next(),
            CreatorId = SteamClient.SteamId,
            Zone = Zone,
            ZoneOwner = -1,
            Position = pos,
            Rotation = Vector3.Zero,
            Decay = true,
            DecayTimer = 32500,
            ActorUpdateDefaultCooldown = 8
        };

        SetActorDefaultValues(actor);
        AddActorAndPropagate(actor);
        logger.LogInformation("spawn raincloud ({ActorId}) at {Pos}", actor.ActorId, actor.Position);
    }

    public void SpawnVoidPortal()
    {
        if (map.HiddenSpots.Count == 0)
        {
            logger.LogError("no hidden_spots found");
            return;
        }

        var point = map.HiddenSpots[_random.Next() % map.HiddenSpots.Count];
        var x = _random.NextSingle() - 0.5f;
        var z = _random.NextSingle() - 0.5f;
        var pos = point.Transform.Origin + new Vector3(x, 0, z);

        SpawnVoidPortal(pos);
    }

    public void SpawnVoidPortal(Vector3 pos)
    {
        if (_owned.Values.Any(actor => actor.ActorType == "void_portal"))
        {
            logger.LogError("void portal already exists");
            return;
        }

        var portal = CreateHostActor("void_portal", pos);
        portal.Decay = true;
        portal.DecayTimer = 36000;

        logger.LogInformation("spawn void_portal ({ActorId}) at {Pos}", portal.ActorId, portal.Position);
    }

    // _spawn_metal_spot
    public void SpawnMetal()
    {
        var point = RandomPickMetalPoint();
        var x = _random.NextSingle() - 0.5f;
        var z = _random.NextSingle() - 0.5f;
        var pos = point.Transform.Origin + new Vector3(x, 0, z);

        SpawnMetal(pos);
    }

    public void SpawnMetal(Vector3 pos)
    {
        var actorCount = GetActorCountByType("metal_spawn");
        if (actorCount >= 8)
        {
            logger.LogError("metal_spawn limit reached (8)");
            return;
        }

        var metal = CreateHostActor("metal_spawn", pos);
        metal.Decay = true;
        metal.DecayTimer = 10000;

        logger.LogInformation("spawn metal_spawn ({ActorId}) at {Pos}", metal.ActorId, metal.Position);
    }

    public void SelectPlayerActor(SteamId steamId, Action<PlayerActor> action)
    {
        if (!_players.TryGetValue(steamId, out var player))
        {
            return;
        }

        action(player);
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