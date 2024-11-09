using System.Collections.Concurrent;
using Steamworks;
using WFDS.Godot.Types;
using WFDS.Server.Common;
using WFDS.Server.Common.Actor;
using WFDS.Server.Common.Types;
using WFDS.Server.Packets;

namespace WFDS.Server.Managers;

public sealed class ActorManager : IDisposable
{
    private const string Zone = "main_zone";
    private readonly Random _random = new();

    private readonly ILogger<ActorManager> _logger;
    private readonly MapManager _map;
    private readonly LobbyManager _lobby;
    private readonly ActorIdManager _id;

    private readonly ConcurrentDictionary<long, IActor> _owned = [];
    private readonly ConcurrentDictionary<long, IActor> _actors = [];
    private readonly ConcurrentDictionary<SteamId, PlayerActor> _players = [];

    public ActorManager(
        ILogger<ActorManager> logger,
        MapManager map,
        LobbyManager lobby,
        ActorIdManager id
    )
    {
        _logger = logger;
        _map = map;
        _lobby = lobby;
        _id = id;
    }

    public void Dispose()
    {
    }


    public void SendAllOwnedActors(Session target)
    {
        foreach (var actor in _owned.Values)
        {
            actor.SendInstanceActor(target);
            actor.SendActorUpdate(target);
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
                _logger.LogError("player already exists");
            }
        }
        else if (actor.CreatorId == SteamClient.SteamId.Value)
        {
            if (_owned.TryAdd(actor.ActorId, actor))
            {
                actor.SendInstanceActor(_lobby);
                actor.SendActorUpdate(_lobby);
            }
            else
            {
                _logger.LogError("actor already exists");
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
            ActorId = _id.Next(),
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

        if (!_id.Add(actorId))
        {
            _logger.LogError("actor id already exists");
            return false;
        }

        if (_players.ContainsKey(playerId))
        {
            _logger.LogError("player already exists");
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
        if (!_id.Add(actorId))
        {
            _logger.LogError("actor id already exists");
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

        _id.Return(actorId);

        var wipe = ActorActionPacket.CreateWipeActorPacket(actorId);
        _lobby.BroadcastPacket(NetChannel.ActorAction, wipe);

        var queue = ActorActionPacket.CreateQueueFreePacket(actor.ActorId);
        _lobby.BroadcastPacket(NetChannel.ActorAction, queue);

        if (actor.CreatorId.Value != SteamClient.SteamId.Value)
        {
            return;
        }

        _owned.TryRemove(actorId, out _);
    }

    private int GetActorCountByType(string actorType)
    {
        return _owned.Values.Count(actor => actor.ActorType == actorType);
    }

    private void RemoveActorFirstByType(string actorType)
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
        var point = _map.TrashPoints[_random.Next() % _map.TrashPoints.Count];

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
        if (actorCount >= 5)
        {
            _logger.LogError("bird count is over");
        }

        var bird = CreateHostActor("ambient_bird", pos);
        bird.Decay = true;
        bird.DecayTimer = 600; // default?

        _logger.LogInformation("spawn bird at {Pos}", bird.Position);
    }

    public void SpawnFish(string type = "fish_spawn")
    {
        var point = _map.FishSpawnPoints[_random.Next() % _map.FishSpawnPoints.Count];
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

        _logger.LogInformation("spawn fish at {Pos}", fish.Position);
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
            _logger.LogError("raincloud already exists");
            return;
        }

        var actor = new RainCloudActor
        {
            ActorType = "raincloud",
            ActorId = _id.Next(),
            CreatorId = SteamClient.SteamId,
            Zone = Zone,
            ZoneOwner = -1,
            Position = pos,
            Rotation = Vector3.Zero,
            Decay = true,
            DecayTimer = 32500
        };

        SetActorDefaultValues(actor);
        AddActorAndPropagate(actor);
        _logger.LogInformation("spawn rain cloud at {Pos}", actor.Position);
    }

    public void SpawnVoidPortal()
    {
        if (_map.HiddenSpots.Count == 0)
        {
            _logger.LogError("no hidden spots found");
            return;
        }

        var point = _map.HiddenSpots[_random.Next() % _map.HiddenSpots.Count];
        var x = _random.NextSingle() - 0.5f;
        var z = _random.NextSingle() - 0.5f;
        var pos = point.Transform.Origin + new Vector3(x, 0, z);

        SpawnVoidPortal(pos);
    }

    public void SpawnVoidPortal(Vector3 pos)
    {
        if (_owned.Values.Any(actor => actor.ActorType == "void_portal"))
        {
            _logger.LogError("void portal already exists");
            return;
        }

        var portal = CreateHostActor("void_portal", pos);
        portal.Decay = true;
        portal.DecayTimer = 36000;

        _logger.LogInformation("spawn void portal at {Pos}", portal.Position);
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
            _logger.LogError("metal count is over");
        }

        var metal = CreateHostActor("metal_spawn", pos);
        metal.Decay = true;
        metal.DecayTimer = 10000;

        _logger.LogInformation("spawn metal at {Pos}", metal.Position);
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
            return _map.ShorelinePoints[_random.Next() % _map.ShorelinePoints.Count];
        }

        return _map.TrashPoints[_random.Next() % _map.TrashPoints.Count];
    }
}