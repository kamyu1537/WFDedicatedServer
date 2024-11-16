using System.Collections.Concurrent;
using System.Numerics;
using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.Actor.Actors;
using WFDS.Common.GameEvents.Events;
using WFDS.Server.Core.GameEvent;

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

    public IEnumerable<IActor> GetActorsByType(ActorType actorType)
    {
        return _actors.Values.Where(actor => actor.Type == actorType);
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

    public IEnumerable<IActor> GetOwnedActorsByType(ActorType actorType)
    {
        return _owned.Values.Where(actor => actor.Type == actorType);
    }

    public int GetActorCountByCreatorId(SteamId creatorId)
    {
        return _actors.Values.Count(actor => actor.CreatorId == creatorId);
    }

    public int GetActorCountByCreatorIdAndType(SteamId creatorId, ActorType actorType)
    {
        return _actors.Values.Count(actor => actor.CreatorId == creatorId && actor.Type == actorType);
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

    public int GetOwnedActorCountByType(ActorType actorType)
    {
        return _owned.Values.Count(actor => actor.Type == actorType);
    }

    public List<ActorType> GetOwnedActorTypes()
    {
        return _owned.Values.Select(actor => actor.Type).ToList();
    }

    public IEnumerable<IActor> GetActorsByCreatorId(SteamId creatorId)
    {
        return _actors.Values.Where(actor => actor.CreatorId == creatorId);
    }

    private bool TryAddActorAndPropagate(IActor actor)
    {
        logger.LogInformation("try add actor {ActorId} {ActorType} - {Position}", actor.ActorId, actor.Type, actor.Position);

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

        GameEventBus.Publish(new ActorCreateEvent(actor.ActorId));
        return true;
    }

    private void SetActorDefaultValues(IActor actor)
    {
        actor.Zone = MainZone;
        actor.ZoneOwner = -1;
        actor.CreateTime = DateTimeOffset.UtcNow;
    }

    private bool CheckAndRemoveActor(ActorType actorType)
    {
        if (actorType.MaxCount < 0)
        {
            logger.LogWarning("{Type} actor spawn disallow", actorType);
            return false;
        }

        // unlimited
        if (actorType.MaxCount == 0)
        {
            return true;
        }

        var actorCount = GetActorCountByType(actorType);
        if (actorType.MaxCount <= actorCount)
        {
            if (actorType.DeleteOver)
            {
                return TryRemoveActorFirstByType(actorType, ActorRemoveTypes.ActorCountOver, out _);
            }

            logger.LogWarning("{Type} actor limit reached ({MaxCount})", ActorType.RainCloud, ActorType.RainCloud.MaxCount);
            return false;
        }

        return true;
    }

    public bool TryCreateHostActor<T>(Vector3 position, out T actor) where T : IActor, new()
    {
        actor = default!;
        if (MaxOwnedActorCount <= _owned.Count)
        {
            logger.LogError("owned actor limit reached ({Count}/{MaxCount})", _owned.Count, MaxOwnedActorCount);
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

    public bool TryCreateRemoteActor(SteamId steamId, long actorId, ActorType actorType, Vector3 position, Vector3 rotation, out IActor actor)
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
            Type = actorType,
            ActorId = actorId,
            CreatorId = steamId,
            Zone = player.Zone,
            ZoneOwner = player.ZoneOwner,
            Position = position,
            Rotation = rotation,
            IsDead = true,
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

        actor.IsRemoved = true;
        actor.IsDead = true;
        
        if (actor.Type == ActorType.Player)
        {
            if (!_players.TryRemove(actor.CreatorId, out _))
            {
                logger.LogError("player not found {SteamId}", actor.CreatorId);
            }
        }

        if (actor.CreatorId == SteamClient.SteamId.Value)
        {
            _owned.TryRemove(actorId, out _);
        }

        idManager.Return(actorId);
        GameEventBus.Publish(new ActorRemoveEvent(actorId, actor.Type, actor.CreatorId, type));
        return true;
    }

    public int GetActorCount() => _actors.Count;

    public int GetActorCountByType(ActorType actorType) => _owned.Values.Count(actor => actor.Type == actorType);

    public bool TryRemoveActorFirstByType(ActorType actorType, ActorRemoveTypes type, out IActor actor)
    {
        actor = null!;
        var find = _owned.Values.OrderBy(x => x.CreateTime).FirstOrDefault(a => a.Type == actorType);
        return find != null && TryRemoveActor(find.ActorId, type, out actor);
    }

    public IActor? SpawnAmbientBirdActor(Vector3 position)
    {
        if (!CheckAndRemoveActor(ActorType.AmbientBird))
        {
            return null;
        }
        
        return TryCreateHostActor<AmbientBirdActor>(position, out var fish) ? fish : null;
    }

    public IActor? SpawnFishSpawnActor(Vector3 position)
    {
        if (!CheckAndRemoveActor(ActorType.FishSpawn))
        {
            return null;
        }
        
        return TryCreateHostActor<FishSpawnActor>(position, out var fish) ? fish : null;
    }

    public IActor? SpawnFishSpawnAlienActor(Vector3 position)
    {
        if (!CheckAndRemoveActor(ActorType.FishSpawnAlien))
        {
            return null;
        }
        
        return TryCreateHostActor<FishSpawnAlienActor>(position, out var fish) ? fish : null;
    }

    public IActor? SpawnRainCloudActor(Vector3 position)
    {
        if (!CheckAndRemoveActor(ActorType.RainCloud))
        {
            return null;
        }
        
        return TryCreateHostActor<RainCloudActor>(position, out var cloud) ? cloud : null;
    }

    public IActor? SpawnVoidPortalActor(Vector3 position)
    {
        if (!CheckAndRemoveActor(ActorType.VoidPortal))
        {
            return null;
        }
        
        return TryCreateHostActor<VoidPortalActor>(position, out var portal) ? portal : null;
    }

    public IActor? SpawnMetalActor(Vector3 position)
    {
        if (!CheckAndRemoveActor(ActorType.MetalSpawn))
        {
            return null;
        }
        
        return TryCreateHostActor<MetalSpawnActor>(position, out var metal) ? metal : null;
    }

    public bool IsInSphereActor(ActorType actorType, in Vector3 position, float radius)
    {
        foreach (var actor in _actors.Values.Where(actor => actor.Type == actorType))
        {
            if (Vector3.Distance(actor.Position, position) <= radius)
            {
                return true;
            }
        }

        return false;
    }
}