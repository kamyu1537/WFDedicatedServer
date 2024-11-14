using System.Collections.Immutable;
using Steamworks;
using WFDS.Godot.Types;

namespace WFDS.Common.Types.Manager;

public interface IActorManager
{
    int GetActorCount();
    int GetActorCountByType(string actorType);
    IActor? GetActor(long actorId);
    IEnumerable<IActor> GetActors();
    IEnumerable<IActor> GetActorsByType(string actorType);
    IEnumerable<IActor> GetOwnedActors();
    
    void SelectActor(long actorId, Action<IActor> action);
    void SelectActors(Action<IActor> action);
    
    int GetPlayerActorCount();
    void SelectPlayerActor(SteamId steamId, Action<IPlayerActor> action);
    void SelectPlayerActors(Action<IPlayerActor> action);
    void SelectPlayerActors(Func<IPlayerActor, bool> action);

    int GetOwnedActorCount();
    int GetOwnedActorCountByType(string actorType);
    List<string> GetOwnedActorTypes();
    void SelectOwnedActors(Action<IActor> action);
    
    int GetActorCountByCreatorId(SteamId creatorId);
    int GetActorCountByCreatorIdAndType(SteamId creatorId, string actorType);
    IEnumerable<IActor> GetActorsByCreatorId(SteamId creatorId);
    void SelectActorsByCreatorId(SteamId creatorId, Action<IActor> action);
    
    bool TryCreateHostActor<T>(Vector3 position, out T actor) where T : IActor, new();
    bool TryCreatePlayerActor(SteamId steamId, long actorId, out IPlayerActor actor);
    bool TryCreateRemoteActor(SteamId steamId, long actorId, string actorType, out IActor actor);
    bool TryRemoveActor(long actorId, ActorRemoveTypes type, out IActor actor);
    bool TryRemoveActorFirstByType(string actorType, ActorRemoveTypes type, out IActor actor);
    
    IActor? SpawnAmbientBirdActor(Vector3 position);
    IActor? SpawnFishSpawnActor(Vector3 position);
    IActor? SpawnFishSpawnAlienActor(Vector3 position);
    IActor? SpawnRainCloudActor(Vector3 position);
    IActor? SpawnVoidPortalActor(Vector3 position);
    IActor? SpawnMetalActor(Vector3 position);
}