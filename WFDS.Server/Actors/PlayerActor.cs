using Steamworks;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Types;

namespace WFDS.Server.Actors;

public sealed class PlayerActor(ISession session) : IPlayerActor
{
    public ILogger? Logger { get; set; }
    public IActorManager? ActorManager { get; set; }

    public string ActorType => "player";
    public long ActorId { get; init; }
    public SteamId CreatorId { get; init; }
    public string Zone { get; set; } = "main_zone";
    public long ZoneOwner { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public bool Decay => false;
    public long DecayTimer { get; set; }
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeadActor { get; set; } = true;
    public long ActorUpdateDefaultCooldown => 0;
    public long ActorUpdateCooldown { get; set; }
    public ISession Session => session;

    public GameItem HeldItem { get; set; } = GameItem.Default;
    public Cosmetics Cosmetics { get; set; } = Cosmetics.Default;

    public void OnCreated()
    {
    }

    public void OnRemoved(ActorRemoveTypes type)
    {
    }

    public void OnUpdate(double delta)
    {
    }

    public void OnZoneUpdated(string zone, long zoneOwner)
    {
        Zone = zone;
        ZoneOwner = zoneOwner;
    }

    public void OnActorUpdated(Vector3 position, Vector3 rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public void Dispose()
    {
        Logger = null;
        ActorManager = null;
    }

    public void OnCosmeticsUpdated(Cosmetics cosmetics)
    {
        Cosmetics = cosmetics;
    }

    public void OnHeldItemUpdated(GameItem item)
    {
        HeldItem = item;
    }

    public void OnLevelUp()
    {
    }

    public void OnMessage(string message, string color, bool local, Vector3 position, string zone, long zoneOwner)
    {
    }

    public void OnChatMessage(string message)
    {
        Logger?.LogInformation("{Member}'s message: {Message}", session.Friend, message);

#if DEBUG
        if (message == "rain")
        {
            ActorManager?.SpawnRainCloudActor(new Vector3(0, 42, 0));
        }

        if (message == "void")
        {
            ActorManager?.SpawnVoidPortalActor(Position);
        }

        if (message == "fish")
        {
            ActorManager?.SpawnFishSpawnActor(Position);
        }

        if (message == "alien")
        {
            ActorManager?.SpawnFishSpawnAlienActor(Position);
        }

        if (message == "bird")
        {
            ActorManager?.SpawnAmbientBirdActor(Position);
        }

        if (message == "metal")
        {
            ActorManager?.SpawnMetalActor(Position);
        }
#endif
    }
}