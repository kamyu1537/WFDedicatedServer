namespace WFDS.Common.Actor;

public sealed class ActorType
{
    public string Name { get; }
    public bool HostOnly { get; }
    public int MaxCount { get; }
    public bool DeleteOver { get; }
    public bool ConnectionSync { get; }

    private ActorType(string name, bool host, int maxCount = -1, bool deleteOver = false, bool connectionSync = false)
    {
        Name = name;
        HostOnly = host;
        MaxCount = maxCount;
        DeleteOver = deleteOver;
        
        ActorTypes.TryAdd(name, this);
    }
    
    public override string ToString() => Name;

    private static readonly Dictionary<string, ActorType> ActorTypes = [];
    public static ActorType? GetActorType(string name) => ActorTypes.GetValueOrDefault(name);
    
    public static ActorType None { get; } = new("none", true, 0);
    public static ActorType FishSpawn { get; } = new("fish_spawn", true, 12, true);
    public static ActorType FishSpawnAlien { get; } = new("fish_spawn_alien", true, 1, true);
    public static ActorType RainCloud { get; } = new("raincloud", true, 2);
    public static ActorType MetalSpawn { get; } = new("metal_spawn", true, 8, false, true);
    public static ActorType AmbientBird { get; } = new("ambient_bird", true, 8, true);
    public static ActorType VoidPortal { get; } = new("void_portal", true, 1, true);
    
    public static ActorType Player { get; } = new("player", false);
    public static ActorType RainCloudTiny { get; } = new("raincloud_tiny", false);
    public static ActorType AquaFish { get; } = new("aqua_fish", false);
    public static ActorType Picnic { get; } = new("picnic", false);
    public static ActorType Canvas { get; } = new("canvas", false);
    public static ActorType Bush { get; } = new("bush", false);
    public static ActorType Rock { get; } = new("rock", false);
    public static ActorType FishTrap { get; } = new("fish_trap", false);
    public static ActorType FishTrapOcean { get; } = new("fish_trap_ocean", false);
    public static ActorType IslandTiny { get; } = new("island_tiny", false);
    public static ActorType IslandMed { get; } = new("island_med", false);
    public static ActorType IslandBig { get; } = new("island_big", false);
    public static ActorType Boombox { get; } = new("boombox", false);
    public static ActorType Well { get; } = new("well", false);
    public static ActorType Campfire { get; } = new("campfire", false);
    public static ActorType Chair { get; } = new("chair", false);
    public static ActorType Table { get; } = new("table", false);
    public static ActorType TherapistChair { get; } = new("therapist_chair", false);
    public static ActorType Toilet { get; } = new("toilet", false);
    public static ActorType Whoopie { get; } = new("whoopie", false);
    public static ActorType Beer { get; } = new("beer", false);
    public static ActorType Greenscreen { get; } = new("greenscreen", false);
    public static ActorType PortableBait { get; } = new("portable_bait", false);
}