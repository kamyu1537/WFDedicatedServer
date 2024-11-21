using System.Numerics;
using Steamworks;

namespace WFDS.Common.Actor;

public interface IActorManager
{
    int GetActorCount();
    int GetActorCountByType(ActorType actorType);
    IActor? GetActor(long actorId);
    IEnumerable<IActor> GetActors();
    IEnumerable<IActor> GetActorsByType(ActorType actorType);
    IEnumerable<IActor> GetOwnedActors();
    
    int GetPlayerActorCount();
    IPlayerActor? GetPlayerActor(CSteamID steamId);
    IEnumerable<IPlayerActor> GetPlayerActors();

    int GetOwnedActorCount();
    int GetOwnedActorCountByType(ActorType actorType);
    List<ActorType> GetOwnedActorTypes();
    IEnumerable<IActor> GetOwnedActorsByType(ActorType actorType);
    
    int GetActorCountByCreatorId(CSteamID creatorId);
    int GetActorCountByCreatorIdAndType(CSteamID creatorId, ActorType actorType);
    IEnumerable<IActor> GetActorsByCreatorId(CSteamID creatorId);
    
    bool TryCreateHostActor<T>(Vector3 position, out T actor) where T : class, IActor, new();
    bool TryCreatePlayerActor(CSteamID steamId, long actorId, out IPlayerActor actor);
    bool TryCreateRemoteActor(CSteamID steamId, long actorId, ActorType actorType, Vector3 position, Vector3 rotation, out IActor actor);
    bool TryRemoveActor(long actorId, ActorRemoveTypes type, out IActor actor);
    bool TryRemoveActorFirstByType(ActorType actorType, ActorRemoveTypes type, out IActor actor);
    
    IActor? SpawnAmbientBirdActor(Vector3 position);
    IActor? SpawnFishSpawnActor(Vector3 position);
    IActor? SpawnFishSpawnAlienActor(Vector3 position);
    IActor? SpawnRainCloudActor(Vector3 position);
    IActor? SpawnVoidPortalActor(Vector3 position);
    IActor? SpawnMetalActor(Vector3 position);
    
    bool IsInSphereActor(ActorType actorType, in Vector3 position, float radius);
}