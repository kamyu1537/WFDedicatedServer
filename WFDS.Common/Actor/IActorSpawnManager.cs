namespace WFDS.Common.Actor;

public interface IActorSpawnManager
{
    void SpawnAmbientBirdActor();
    IActor? SpawnFishSpawnActor();
    IActor? SpawnFishSpawnAlienActor();
    IActor? SpawnRainCloudActor();
    IActor? SpawnVoidPortalActor();
    IActor? SpawnMetalActor();
}