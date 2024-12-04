namespace WFDS.Common.Actor;

public sealed class ActorType
{
    public string Name { get; }
    public bool HostOnly { get; }
    public int MaxCount { get; }
    public bool DeleteOver { get; }
    public int DecayTimer { get; }

    private ActorType(string name, bool host, int decayTimer, int maxCount, bool deleteOver)
    {
        Name = name;
        HostOnly = host;
        MaxCount = maxCount;
        DeleteOver = deleteOver;
        DecayTimer = decayTimer;
        
        ActorTypes.TryAdd(name, this);
    }
    
    public override string ToString() => Name;

    private static readonly Dictionary<string, ActorType> ActorTypes = [];
    public static ActorType? GetActorType(string name) => ActorTypes.GetValueOrDefault(name);
    
    public static ActorType None { get; } = new("none", true, -1, 0, false);
    public static ActorType FishSpawn { get; } = new("fish_spawn", true, 4800, 13, true);
    public static ActorType FishSpawnAlien { get; } = new("fish_spawn_alien", true, 14400, 1, true);
    public static ActorType RainCloud { get; } = new("raincloud", true, 32500, 1, false);
    public static ActorType MetalSpawn { get; } = new("metal_spawn", true, 10000, 8, false);
    public static ActorType AmbientBird { get; } = new("ambient_bird", true, 600, 8, true);
    public static ActorType VoidPortal { get; } = new("void_portal", true, 36000, 1, true);
    
    public static ActorType Player { get; } = new("player", false, -1, 0, false);
    public static ActorType RainCloudTiny { get; } = new("raincloud_tiny", false, -1, 0, false);
    public static ActorType AquaFish { get; } = new("aqua_fish", false, -1, 0, false);
    public static ActorType Picnic { get; } = new("picnic", false, -1, 0, false);
    public static ActorType Canvas { get; } = new("canvas", false, -1, 0, false);
    public static ActorType Bush { get; } = new("bush", false, -1, 0, false);
    public static ActorType Rock { get; } = new("rock", false, -1, 0, false);
    public static ActorType FishTrap { get; } = new("fish_trap", false, -1, 0, false);
    public static ActorType FishTrapOcean { get; } = new("fish_trap_ocean", false, -1, 0, false);
    public static ActorType IslandTiny { get; } = new("island_tiny", false, -1, 0, false);
    public static ActorType IslandMed { get; } = new("island_med", false, -1, 0, false);
    public static ActorType IslandBig { get; } = new("island_big", false, -1, 0, false);
    public static ActorType Boombox { get; } = new("boombox", false, -1, 0, false);
    public static ActorType Well { get; } = new("well", false, -1, 0, false);
    public static ActorType Campfire { get; } = new("campfire", false, -1, 0, false);
    public static ActorType Chair { get; } = new("chair", false, -1, 0, false);
    public static ActorType Table { get; } = new("table", false, -1, 0, false);
    public static ActorType TherapistChair { get; } = new("therapist_chair", false, -1, 0, false);
    public static ActorType Toilet { get; } = new("toilet", false, -1, 0, false);
    public static ActorType Whoopie { get; } = new("whoopie", false, -1, 0, false);
    public static ActorType Beer { get; } = new("beer", false, -1, 0, false);
    public static ActorType Greenscreen { get; } = new("greenscreen", false, -1, 0, false);
    public static ActorType PortableBait { get; } = new("portable_bait", false, -1, 0, false);
}