using System.Collections.Concurrent;
using System.Numerics;
using Steamworks;
using WFDS.Common.Actor;
using WFDS.Common.Actor.Actors;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Steam;


namespace WFDS.Server.Core.Actor;

internal sealed class ActorManager(ILogger<ActorManager> logger, IActorIdManager idManager, SteamManager steam, IActorSettingManager actorSettingManager) : IActorManager
{
    private const string MainZone = "main_zone";
    private const int MaxOwnedActorCount = 32;

    private readonly ConcurrentDictionary<long, IActor> _owned = [];
    private readonly ConcurrentDictionary<long, IActor> _actors = [];
    private readonly ConcurrentDictionary<CSteamID, IPlayerActor> _players = [];

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

    public IPlayerActor? GetPlayerActor(CSteamID steamId)
    {
        return _players.TryGetValue(steamId, out var player) ? player : null;
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

    public int GetActorCountByCreatorId(CSteamID creatorId)
    {
        return _actors.Values.Count(actor => actor.CreatorId == creatorId);
    }

    public int GetActorCountByCreatorIdAndType(CSteamID creatorId, ActorType actorType)
    {
        return _actors.Values.Count(actor => actor.CreatorId == creatorId && actor.Type == actorType);
    }

    public void SelectActorsByCreatorId(CSteamID creatorId, Action<IActor> action)
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

    public IEnumerable<IActor> GetActorsByCreatorId(CSteamID creatorId)
    {
        return _actors.Values.Where(actor => actor.CreatorId == creatorId);
    }

    private bool TryAddActorAndPropagate(IActor actor)
    {
        logger.LogInformation("try add actor {ActorId} {ActorType} - {ActorPosition}", actor.ActorId, actor.Type, actor.Position);

        if (actor is IPlayerActor player && !_players.TryAdd(player.CreatorId, player))
        {
            logger.LogError("player already exists");
            return false;
        }

        if (actor.CreatorId == steam.SteamId && !_owned.TryAdd(actor.ActorId, actor))
        {
            logger.LogError("owned actor already exists");
            return false;
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
        actor.DecayTimer = actorSettingManager.GetDecayTimer(actor.Type.Name);
    }

    private bool CheckAndRemoveActor(ActorType actorType)
    {
        var maxCount = actorSettingManager.GetMaxCount(actorType.Name);
        
        if (maxCount < 0)
        {
            logger.LogWarning("{ActorType} actor spawn disallow", actorType);
            return false;
        }

        if (maxCount == 0) // unlimited
        {
            return true;
        }

        var actorCount = GetActorCountByType(actorType);
        if (maxCount <= actorCount)
        {
            if (actorType.DeleteOver)
            {
                return TryRemoveActorFirstByType(actorType, ActorRemoveTypes.ActorCountOver, out _);
            }

            logger.LogWarning("{ActorType} actor limit reached ({MaxCount})", ActorType.RainCloud.Name, ActorType.RainCloud.MaxCount);
            return false;
        }

        return true;
    }

    public bool TryCreateHostActor<T>(Vector3 position, out T actor) where T : class, IActor, new()
    {
        actor = default!;
        if (MaxOwnedActorCount <= _owned.Count)
        {
            logger.LogError("owned actor limit reached ({OwnedActorCount}/{MaxOwnedActorCount})", _owned.Count, MaxOwnedActorCount);
            return false;
        }

        actor = Actor<T>.Get();
        actor.ActorId = idManager.Next();
        actor.CreatorId = steam.SteamId;
        actor.Position = position;

        SetActorDefaultValues(actor);
        return TryAddActorAndPropagate(actor);
    }

    public bool TryCreatePlayerActor(CSteamID steamId, long actorId, out IPlayerActor actor)
    {
        actor = null!;

        if (!idManager.Add(actorId))
        {
            logger.LogError("actor id already exists {ActorId} {ActorType}", actorId, ActorType.Player.Name);
            return false;
        }

        actor = PlayerActor.Get();
        actor.ActorId = actorId;
        actor.CreatorId = steamId;
        actor.Zone = MainZone;
        actor.ZoneOwner = -1;
        actor.Position = Vector3.Zero;
        actor.Rotation = Vector3.Zero;

        SetActorDefaultValues(actor);
        return TryAddActorAndPropagate(actor);
    }

    public bool TryCreateRemoteActor(CSteamID steamId, long actorId, ActorType actorType, Vector3 position, Vector3 rotation, out IActor actor)
    {
        actor = null!;
        if (!idManager.Add(actorId))
        {
            logger.LogError("actor id already exists {ActorId} {ActorType}", actorId, actorType.Name);
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
            logger.LogError("owned actor limit reached ({MaxOwnedActorCount})", MaxOwnedActorCount);
            return false;
        }

        var remoteActor = RemoteActor.Get();
        remoteActor.ActorId = actorId;
        remoteActor.CreatorId = steamId;
        remoteActor.Zone = player.Zone;
        remoteActor.ZoneOwner = player.ZoneOwner;
        remoteActor.Position = position;
        remoteActor.Rotation = rotation;
        remoteActor.IsDead = true;
        remoteActor.SetActorType(actorType);
        remoteActor.SetDecay(false);

        actor = remoteActor;
        SetActorDefaultValues(actor);
        return TryAddActorAndPropagate(actor);
    }

    public void SelectPlayerActor(CSteamID steamId, Action<IPlayerActor> action)
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
        
        if (actor.Type == ActorType.Player && !_players.TryRemove(actor.CreatorId, out _))
        {
            logger.LogError("player not found {CreatorId}", actor.CreatorId);
        }

        if (actor.CreatorId == steam.SteamId)
        {
            _owned.TryRemove(actorId, out _);
        }

        idManager.Return(actorId);
        actor.Remove();
        
        logger.LogInformation("actor removed {ActorId} {ActorType} {ActorCreatorId} {RemoveType}", actorId, actor.Type, actor.CreatorId, type);
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