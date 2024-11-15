using System.Numerics;
using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.Actor;

public interface IActorManager
{
    int GetActorCount();
    int GetActorCountByType(string actorType);
    IActor? GetActor(long actorId);
    IEnumerable<IActor> GetActors();
    IEnumerable<IActor> GetActorsByType(string actorType);
    IEnumerable<IActor> GetOwnedActors();
    
    int GetPlayerActorCount();
    IPlayerActor? GetPlayerActor(SteamId steamId);
    IEnumerable<IPlayerActor> GetPlayerActors();

    int GetOwnedActorCount();
    int GetOwnedActorCountByType(string actorType);
    List<string> GetOwnedActorTypes();
    IEnumerable<IActor> GetOwnedActorsByType(string actorType);
    
    int GetActorCountByCreatorId(SteamId creatorId);
    int GetActorCountByCreatorIdAndType(SteamId creatorId, string actorType);
    IEnumerable<IActor> GetActorsByCreatorId(SteamId creatorId);
    
    bool TryCreateHostActor<T>(Vector3 position, out T actor) where T : IActor, new();
    bool TryCreatePlayerActor(SteamId steamId, long actorId, out IPlayerActor actor);
    bool TryCreateRemoteActor(SteamId steamId, long actorId, string actorType, Vector3 position, Vector3 rotation, out IActor actor);
    bool TryRemoveActor(long actorId, ActorRemoveTypes type, out IActor actor);
    bool TryRemoveActorFirstByType(string actorType, ActorRemoveTypes type, out IActor actor);
    
    IActor? SpawnAmbientBirdActor(Vector3 position);
    IActor? SpawnFishSpawnActor(Vector3 position);
    IActor? SpawnFishSpawnAlienActor(Vector3 position);
    IActor? SpawnRainCloudActor(Vector3 position);
    IActor? SpawnVoidPortalActor(Vector3 position);
    IActor? SpawnMetalActor(Vector3 position);
}