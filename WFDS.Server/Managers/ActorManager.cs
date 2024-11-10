using System.Collections.Concurrent;
using System.Collections.Immutable;
using Steamworks;
using WFDS.Common.Types;
using WFDS.Godot.Types;
using WFDS.Server.Common;
using WFDS.Server.Common.Actor;
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
    private static readonly string MainZone = "main_zone";
    private static readonly int MaxOwnedActorCount = 32;

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

    public ImmutableArray<IActor> GetActorsByCreatorId(SteamId creatorId)
    {
        return [.._actors.Values.Where(actor => actor.CreatorId == creatorId)];
    }

    private bool AddActorAndPropagate(IActor actor)
    {
        logger.LogInformation("try add actor {ActorId} {ActorType} - {Position}", actor.ActorId, actor.ActorType, actor.Position);
        if (!_actors.TryAdd(actor.ActorId, actor))
        {
            return false;
        }
        
        actor.OnCreated();
        if (actor is PlayerActor player)
        {
            if (!_players.TryAdd(player.CreatorId, player))
            {
                logger.LogError("player already exists");
                return false;
            }
        }
        else if (actor.CreatorId == SteamClient.SteamId.Value)
        {
            if (_owned.TryAdd(actor.ActorId, actor))
            {
                actor.SendInstanceActor(lobby);
                actor.SendUpdatePacket(lobby);
                return true;
            }

            logger.LogError("actor already exists");
            return false;
        }

        return true;
    }

    private static void SetActorDefaultValues(IActor actor)
    {
        actor.Zone = MainZone;
        actor.ZoneOwner = -1;
        actor.CreateTime = DateTimeOffset.UtcNow;
    }

    private bool TryCreateHostActor<T>(Vector3 position, out T actor) where T : IActor, new()
    {
        actor = default!;
        if (_owned.Count >= MaxOwnedActorCount)
        {
            logger.LogError("owned actor limit reached ({MaxCount})", MaxOwnedActorCount);
            return false;
        }

        actor = new T
        {
            ActorId = id.Next(),
            CreatorId = SteamClient.SteamId,
            Position = position
        };

        SetActorDefaultValues(actor);
        return AddActorAndPropagate(actor);
    }

    public bool TryCreatePlayerActor(SteamId playerId, long actorId, out PlayerActor actor)
    {
        actor = null!;

        if (!id.Add(actorId))
        {
            logger.LogError("actor id already exists {ActorId} {ActorType}", actorId, "player");
            return false;
        }

        actor = new PlayerActor
        {
            ActorId = actorId,
            CreatorId = playerId,
            Zone = MainZone,
            ZoneOwner = -1,
            Position = Vector3.Zero,
            Rotation = Vector3.Zero
        };

        if (!_players.TryAdd(playerId, actor))
        {
            logger.LogError("player already exists {SteamId}", playerId);
            return false;
        }

        SetActorDefaultValues(actor);
        AddActorAndPropagate(actor);
        return true;
    }


    public bool TryCreateRemoteActor(SteamId owner, long actorId, string actorType, out RemoteActor actor)
    {
        actor = null!;
        if (!id.Add(actorId))
        {
            logger.LogError("actor id already exists {ActorId} {ActorType}", actorId, actorType);
            return false;
        }

        var success = false;
        SelectPlayerActor(owner, player =>
        {
            var ownedActors = GetActorsByCreatorId(owner);
            if (ownedActors.Length >= MaxOwnedActorCount)
            {
                logger.LogError("owned actor limit reached ({MaxCount})", MaxOwnedActorCount);
                lobby.KickPlayer(owner);
                return;
            }

            var actor = new RemoteActor
            {
                ActorType = actorType,
                ActorId = actorId,
                CreatorId = owner,
                Zone = player.Zone,
                ZoneOwner = player.ZoneOwner,
                Position = Vector3.Zero,
                Rotation = Vector3.Zero,
                IsDeadActor = true,
                Decay = false
            };

            SetActorDefaultValues(actor);
            success = AddActorAndPropagate(actor);
        });

        return success;
    }

    public void SelectPlayerActor(SteamId steamId, Action<PlayerActor> action)
    {
        if (!_players.TryGetValue(steamId, out var player))
        {
            return;
        }

        action(player);
    }

    public void RemoveActor(long actorId, ActorRemoveTypes removeType = ActorRemoveTypes.None)
    {
        if (!_actors.TryRemove(actorId, out var actor))
        {
            return;
        }

        if ( actor.ActorType == "player" && !_players.TryRemove(actor.CreatorId, out _))
        {
            logger.LogError("player not found {SteamId}", actor.CreatorId);
        }

        actor.OnRemoved(removeType);
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

    public void RemoveActorFirstByType(string actorType, ActorRemoveTypes removeType = ActorRemoveTypes.None)
    {
        var actor = _owned.Values.FirstOrDefault(a => a.ActorType == actorType);
        if (actor == null)
        {
            return;
        }

        RemoveActor(actor.ActorId, removeType);
    }

    public void SpawnAmbientBirdActor()
    {
        var count = _random.Next() % 3 + 1;
        var point = map.TrashPoints[_random.Next() % map.TrashPoints.Count];

        for (var i = 0; i < count; i++)
        {
            var x = _random.NextSingle() * 5f - 2.5f;
            var z = _random.NextSingle() * 5f - 2.5f;
            var pos = point.Transform.Origin + new Vector3(x, 0, z);

            SpawnAmbientBirdActor(pos);
        }
    }

    public void SpawnAmbientBirdActor(Vector3 pos)
    {
        var actorCount = GetActorCountByType("ambient_bird");
        if (actorCount >= 10)
        {
            RemoveActorFirstByType("ambient_bird", ActorRemoveTypes.ActorCountOver);
        }

        if (TryCreateHostActor<AmbientBirdActor>(pos, out var bird))
        {
            logger.LogInformation("spawn {ActorType} ({ActorId}) at {Pos}", bird.ActorType, bird.ActorId, bird.Position);
        }
    }

    public IActor? SpawnFishSpawnActor()
    {
        var point = map.FishSpawnPoints[_random.Next() % map.FishSpawnPoints.Count];
        return SpawnFishSpawnActor(point.Transform.Origin);
    }

    public IActor? SpawnFishSpawnActor(Vector3 pos)
    {
        var actorCount = GetActorCountByType("fish_spawn");
        if (actorCount >= 7)
        {
            RemoveActorFirstByType("fish_spawn", ActorRemoveTypes.ActorCountOver);
        }

        return TryCreateHostActor<FishSpawnActor>(pos, out var fish) ? fish : null;
    }

    public IActor? SpawnFishSpawnAlienActor()
    {
        var point = map.FishSpawnPoints[_random.Next() % map.FishSpawnPoints.Count];
        return SpawnFishSpawnAlienActor(point.Transform.Origin);
    }

    public IActor? SpawnFishSpawnAlienActor(Vector3 pos)
    {
        var actorCount = GetActorCountByType("fish_spawn_alien");
        if (actorCount >= 7)
        {
            RemoveActorFirstByType("fish_spawn_alien", ActorRemoveTypes.ActorCountOver);
        }

        return TryCreateHostActor<FishSpawnAlienActor>(pos, out var fish) ? fish : null;
    }

    public IActor? SpawnRainCloudActor()
    {
        var x = _random.NextSingle() * 250f - 100f;
        var z = _random.NextSingle() * 250f - 150f;
        var pos = new Vector3(x, 42f, z);

        return SpawnRainCloudActor(pos);
    }

    public IActor? SpawnRainCloudActor(Vector3 pos)
    {
        var actorCount = GetActorCountByType("raincloud");
        if (actorCount > 0)
        {
            return null;
        }

        return TryCreateHostActor<RainCloudActor>(pos, out var cloud) ? cloud : null;
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

        return SpawnVoidPortalActor(pos);
    }

    public IActor? SpawnVoidPortalActor(Vector3 pos)
    {
        var actorCount = GetActorCountByType("void_portal");
        if (actorCount > 0)
        {
            logger.LogError("void_portal already exists");
            return null;
        }

        return TryCreateHostActor<VoidPortalActor>(pos, out var portal) ? portal : null;
    }

    // _spawn_metal_spot
    public IActor? SpawnMetalActor()
    {
        var point = RandomPickMetalPoint();
        var x = _random.NextSingle() - 0.5f;
        var z = _random.NextSingle() - 0.5f;
        var pos = point.Transform.Origin + new Vector3(x, 0, z);

        return SpawnMetalActor(pos);
    }

    public IActor? SpawnMetalActor(Vector3 pos)
    {
        var actorCount = GetActorCountByType("metal_spawn");
        if (actorCount >= 8)
        {
            logger.LogError("metal_spawn limit reached (8)");
            return null;
        }

        return TryCreateHostActor<MetalSpawnActor>(pos, out var metal) ? metal : null;
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