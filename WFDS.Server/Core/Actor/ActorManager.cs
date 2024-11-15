using System.Collections.Concurrent;
using System.Numerics;
using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.Actor.Actors;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Server.Core.ChannelEvent;

namespace WFDS.Server.Core.Actor;

internal sealed class ActorManager(ILogger<ActorManager> logger, IActorIdManager idManager) : IActorManager
{
    private const string MainZone = "main_zone";
    private const int MaxOwnedActorCount = 32;

    private readonly ConcurrentDictionary<long, IActor> _owned = [];
    private readonly ConcurrentDictionary<long, IActor> _actors = [];
    private readonly ConcurrentDictionary<SteamId, IPlayerActor> _players = [];

    public IActor? GetActor(long actorId)
    {
        return _actors.TryGetValue(actorId, out var actor) ? actor : null;
    }

    public IEnumerable<IActor> GetActors()
    {
        return _actors.Values;
    }

    public IEnumerable<IActor> GetActorsByType(string actorType)
    {
        return _actors.Values.Where(actor => actor.ActorType == actorType);
    }

    public IEnumerable<IActor> GetOwnedActors()
    {
        return _owned.Values;
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

    public int GetPlayerActorCount()
    {
        return _players.Count;
    }

    public IPlayerActor? GetPlayerActor(SteamId playerId)
    {
        return _players.TryGetValue(playerId, out var player) ? player : null;
    }

    public IEnumerable<IPlayerActor> GetPlayerActors()
    {
        return _players.Values;
    }

    public void SelectOwnedActors(Action<IActor> action)
    {
        foreach (var actor in _owned.Values)
        {
            action(actor);
        }
    }

    public IEnumerable<IActor> GetOwnedActorsByType(string actorType)
    {
        return _owned.Values.Where(actor => actor.ActorType == actorType);
    }

    public int GetActorCountByCreatorId(SteamId creatorId)
    {
        return _actors.Values.Count(actor => actor.CreatorId == creatorId);
    }

    public int GetActorCountByCreatorIdAndType(SteamId creatorId, string actorType)
    {
        return _actors.Values.Count(actor => actor.CreatorId == creatorId && actor.ActorType == actorType);
    }

    public void SelectActorsByCreatorId(SteamId creatorId, Action<IActor> action)
    {
        foreach (var actor in _actors.Values.Where(actor => actor.CreatorId == creatorId))
        {
            action(actor);
        }
    }

    public void SelectPlayerActors(Action<IPlayerActor> action)
    {
        foreach (var player in _players.Values)
        {
            action(player);
        }
    }

    public void SelectPlayerActors(Func<IPlayerActor, bool> action)
    {
        foreach (var player in _players.Values)
        {
            if (!action(player))
            {
                break;
            }
        }
    }

    public int GetOwnedActorCount()
    {
        return _owned.Count;
    }

    public int GetOwnedActorCountByType(string actorType)
    {
        return _owned.Values.Count(actor => actor.ActorType == actorType);
    }

    public List<string> GetOwnedActorTypes()
    {
        return _owned.Values.Select(actor => actor.ActorType).ToList();
    }

    public IEnumerable<IActor> GetActorsByCreatorId(SteamId creatorId)
    {
        return _actors.Values.Where(actor => actor.CreatorId == creatorId);
    }

    private bool TryAddActorAndPropagate(IActor actor)
    {
        logger.LogInformation("try add actor {ActorId} {ActorType} - {Position}", actor.ActorId, actor.ActorType, actor.Position);
        
        if (actor is IPlayerActor player)
        {
            if (!_players.TryAdd(player.CreatorId, player))
            {
                logger.LogError("player already exists");
                return false;
            }
        }

        if (actor.CreatorId == SteamClient.SteamId.Value)
        {
            if (!_owned.TryAdd(actor.ActorId, actor))
            {
                logger.LogError("owned actor already exists");
                return false;
            }
        }
        
        if (!_actors.TryAdd(actor.ActorId, actor))
        {
            logger.LogError("actor already exists");
            return false;
        }

        ChannelEventBus.PublishAsync(new ActorCreateEvent(actor.ActorId)).Wait();
        return true;
    }

    private void SetActorDefaultValues(IActor actor)
    {
        actor.Zone = MainZone;
        actor.ZoneOwner = -1;
        actor.CreateTime = DateTimeOffset.UtcNow;
    }

    public bool TryCreateHostActor<T>(Vector3 position, out T actor) where T : IActor, new()
    {
        actor = default!;
        if (_owned.Count >= MaxOwnedActorCount)
        {
            logger.LogError("owned actor limit reached ({MaxCount})", MaxOwnedActorCount);
            return false;
        }

        actor = new T
        {
            ActorId = idManager.Next(),
            CreatorId = SteamClient.SteamId,
            Position = position
        };

        SetActorDefaultValues(actor);
        return TryAddActorAndPropagate(actor);
    }

    public bool TryCreatePlayerActor(SteamId playerId, long actorId, out IPlayerActor actor)
    {
        actor = null!;

        if (!idManager.Add(actorId))
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

        SetActorDefaultValues(actor);
        return TryAddActorAndPropagate(actor);
    }

    public bool TryCreateRemoteActor(SteamId steamId, long actorId, string actorType, Vector3 position, Vector3 rotation, out IActor actor)
    {
        actor = null!;
        if (!idManager.Add(actorId))
        {
            logger.LogError("actor id already exists {ActorId} {ActorType}", actorId, actorType);
            return false;
        }

        var player = GetPlayerActor(steamId);
        if (player == null)
        {
            return false;
        }

        var ownedActors = GetActorsByCreatorId(steamId);
        if (ownedActors.Count() >= MaxOwnedActorCount)
        {
            logger.LogError("owned actor limit reached ({MaxCount})", MaxOwnedActorCount);
            return false;
        }

        actor = new RemoteActor
        {
            ActorType = actorType,
            ActorId = actorId,
            CreatorId = steamId,
            Zone = player.Zone,
            ZoneOwner = player.ZoneOwner,
            Position = position,
            Rotation = rotation,
            IsDeadActor = true,
            Decay = false
        };

        SetActorDefaultValues(actor);
        return TryAddActorAndPropagate(actor);
    }

    public void SelectPlayerActor(SteamId steamId, Action<IPlayerActor> action)
    {
        if (!_players.TryGetValue(steamId, out var player))
        {
            return;
        }

        action(player);
    }

    public bool TryRemoveActor(long actorId, ActorRemoveTypes type, out IActor actor)
    {
        if (!_actors.TryRemove(actorId, out actor!))
        {
            return false;
        }

        if (actor.ActorType == "player" && !_players.TryRemove(actor.CreatorId, out _))
        {
            logger.LogError("player not found {SteamId}", actor.CreatorId);
        }

        ChannelEventBus.PublishAsync(new ActorRemoveEvent(actorId)).Wait();
        idManager.Return(actorId);
        return true;
    }

    public int GetActorCount() => _actors.Count;

    public int GetActorCountByType(string actorType) => _owned.Values.Count(actor => actor.ActorType == actorType);

    public bool TryRemoveActorFirstByType(string actorType, ActorRemoveTypes type, out IActor actor)
    {
        actor = null!;
        var find = _owned.Values.FirstOrDefault(a => a.ActorType == actorType);
        return find != null && TryRemoveActor(find.ActorId, type, out actor);
    }

    public IActor? SpawnAmbientBirdActor(Vector3 position)
    {
        var actorCount = GetActorCountByType("ambient_bird");
        if (actorCount >= 7)
        {
            TryRemoveActorFirstByType("ambient_bird", ActorRemoveTypes.ActorCountOver, out _);
        }

        return TryCreateHostActor<AmbientBirdActor>(position, out var fish) ? fish : null;
    }

    public IActor? SpawnFishSpawnActor(Vector3 position)
    {
        var actorCount = GetActorCountByType("fish_spawn");
        if (actorCount >= 5)
        {
            TryRemoveActorFirstByType("fish_spawn", ActorRemoveTypes.ActorCountOver, out _);
        }

        return TryCreateHostActor<FishSpawnActor>(position, out var fish) ? fish : null;
    }

    public IActor? SpawnFishSpawnAlienActor(Vector3 position)
    {
        var actorCount = GetActorCountByType("fish_spawn_alien");
        if (actorCount >= 1)
        {
            TryRemoveActorFirstByType("fish_spawn_alien", ActorRemoveTypes.ActorCountOver, out _);
        }

        return TryCreateHostActor<FishSpawnAlienActor>(position, out var fish) ? fish : null;
    }

    public IActor? SpawnRainCloudActor(Vector3 position)
    {
        var actorCount = GetActorCountByType("raincloud");
        if (actorCount >= 1)
        {
            TryRemoveActorFirstByType("raincloud", ActorRemoveTypes.ActorCountOver, out _);
        }

        return TryCreateHostActor<RainCloudActor>(position, out var cloud) ? cloud : null;
    }

    public IActor? SpawnVoidPortalActor(Vector3 position)
    {
        var actorCount = GetActorCountByType("void_portal");
        if (actorCount >= 1)
        {
            TryRemoveActorFirstByType("void_portal", ActorRemoveTypes.ActorCountOver, out _);
        }

        return TryCreateHostActor<VoidPortalActor>(position, out var portal) ? portal : null;
    }

    public IActor? SpawnMetalActor(Vector3 position)
    {
        var actorCount = GetActorCountByType("metal_spawn");
        if (actorCount >= 8)
        {
            TryRemoveActorFirstByType("metal_spawn", ActorRemoveTypes.ActorCountOver, out _);
        }

        return TryCreateHostActor<MetalSpawnActor>(position, out var metal) ? metal : null;
    }
}