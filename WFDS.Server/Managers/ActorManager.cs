using System.Collections.Concurrent;
using System.Collections.Immutable;
using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Types;
using WFDS.Server.Actors;
using WFDS.Server.Packets;

namespace WFDS.Server.Managers;

public sealed class ActorManager(
    ILogger<ActorManager> logger,
    ISessionManager sessionManager,
    IActorIdManager idManager)
    : IActorManager
{
    private static readonly string MainZone = "main_zone";
    private static readonly int MaxOwnedActorCount = 32;

    private readonly ConcurrentDictionary<long, IActor> _owned = [];
    private readonly ConcurrentDictionary<long, IActor> _actors = [];
    private readonly ConcurrentDictionary<SteamId, IPlayerActor> _players = [];

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

    public void SelectOwnedActors(Action<IActor> action)
    {
        foreach (var actor in _owned.Values)
        {
            action(actor);
        }
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

    public ImmutableArray<IActor> GetActorsByCreatorId(SteamId creatorId)
    {
        return [.._actors.Values.Where(actor => actor.CreatorId == creatorId)];
    }

    private bool TryAddActorAndPropagate(IActor actor)
    {
        logger.LogInformation("try add actor {ActorId} {ActorType} - {Position}", actor.ActorId, actor.ActorType, actor.Position);
        if (!_actors.TryAdd(actor.ActorId, actor))
        {
            return false;
        }

        actor.OnCreated();
        if (actor is IPlayerActor player)
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
                actor.SendInstanceActor(sessionManager);
                actor.SendUpdatePacket(sessionManager);
                return true;
            }

            logger.LogError("actor already exists");
            return false;
        }

        return true;
    }

    private void SetActorDefaultValues(IActor actor)
    {
        actor.Zone = MainZone;
        actor.ZoneOwner = -1;
        actor.CreateTime = DateTimeOffset.UtcNow;
        
        actor.Logger = logger;
        actor.ActorManager = this;
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

        var success = false;
        IPlayerActor? result = null;
        sessionManager.SelectSession(playerId, session =>
        {
            result = new PlayerActor(session)
            {
                ActorId = actorId,
                CreatorId = playerId,
                Zone = MainZone,
                ZoneOwner = -1,
                Position = Vector3.Zero,
                Rotation = Vector3.Zero
            };

            SetActorDefaultValues(result);
            success = TryAddActorAndPropagate(result);
        });

        if (result != null)
        {
            actor = result;
        }
        
        return result != null && success;
    }

    public bool TryCreateRemoteActor(SteamId steamId, long actorId, string actorType, out IActor actor)
    {
        actor = null!;
        if (!idManager.Add(actorId))
        {
            logger.LogError("actor id already exists {ActorId} {ActorType}", actorId, actorType);
            return false;
        }

        var success = false;
        SelectPlayerActor(steamId, player =>
        {
            var ownedActors = GetActorsByCreatorId(steamId);
            if (ownedActors.Length >= MaxOwnedActorCount)
            {
                logger.LogError("owned actor limit reached ({MaxCount})", MaxOwnedActorCount);
                sessionManager.KickPlayer(steamId);
                return;
            }

            var actor = new RemoteActor
            {
                ActorType = actorType,
                ActorId = actorId,
                CreatorId = steamId,
                Zone = player.Zone,
                ZoneOwner = player.ZoneOwner,
                Position = Vector3.Zero,
                Rotation = Vector3.Zero,
                IsDeadActor = true,
                Decay = false
            };

            SetActorDefaultValues(actor);
            success = TryAddActorAndPropagate(actor);
        });

        return success;
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

        actor.OnRemoved(type);
        idManager.Return(actorId);

        try
        {
            var wipe = ActorActionPacket.CreateWipeActorPacket(actorId);
            sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, wipe);

            if (actor.CreatorId.Value != SteamClient.SteamId.Value)
            {
                return false;
            }

            if (!_owned.TryRemove(actorId, out _))
            {
                return false;
            }

            var queue = ActorActionPacket.CreateQueueFreePacket(actor.ActorId);
            sessionManager.BroadcastP2PPacket(NetChannel.ActorAction, queue);
            return true;
        }
        finally
        {
            actor.Dispose();
        }
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
        if (actorCount >= 10)
        {
            TryRemoveActorFirstByType("ambient_bird", ActorRemoveTypes.ActorCountOver, out _);
        }

        return TryCreateHostActor<AmbientBirdActor>(position, out var fish) ? fish : null;
    }

    public IActor? SpawnFishSpawnActor(Vector3 position)
    {
        var actorCount = GetActorCountByType("fish_spawn");
        if (actorCount >= 7)
        {
            TryRemoveActorFirstByType("fish_spawn", ActorRemoveTypes.ActorCountOver, out _);
        }

        return TryCreateHostActor<FishSpawnActor>(position, out var fish) ? fish : null;
    }

    public IActor? SpawnFishSpawnAlienActor(Vector3 position)
    {
        var actorCount = GetActorCountByType("fish_spawn_alien");
        if (actorCount >= 7)
        {
            TryRemoveActorFirstByType("fish_spawn_alien", ActorRemoveTypes.ActorCountOver, out _);
        }

        return TryCreateHostActor<FishSpawnAlienActor>(position, out var fish) ? fish : null;
    }

    public IActor? SpawnRainCloudActor(Vector3 position)
    {
        var actorCount = GetActorCountByType("raincloud");
        if (actorCount > 0)
        {
            return null;
        }

        return TryCreateHostActor<RainCloudActor>(position, out var cloud) ? cloud : null;
    }

    public IActor? SpawnVoidPortalActor(Vector3 position)
    {
        var actorCount = GetActorCountByType("void_portal");
        if (actorCount > 0)
        {
            logger.LogError("void_portal already exists");
            return null;
        }

        return TryCreateHostActor<VoidPortalActor>(position, out var portal) ? portal : null;
    }

    public IActor? SpawnMetalActor(Vector3 position)
    {
        var actorCount = GetActorCountByType("metal_spawn");
        if (actorCount >= 8)
        {
            logger.LogError("metal_spawn limit reached (8)");
            return null;
        }

        return TryCreateHostActor<MetalSpawnActor>(position, out var metal) ? metal : null;
    }
}