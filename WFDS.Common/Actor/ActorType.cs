namespace WFDS.Common.Actor;

/*
const ACTOR_BANK = {
    "player": [preload("res://Scenes/Entities/Player/player.tscn"), false, 1],
    "fish_spawn": [preload("res://Scenes/Entities/FishSpawn/fish_spawn.tscn"), true, 16],
    "fish_spawn_alien": [preload("res://Scenes/Entities/MeteorSpawn/meteor_spawn.tscn"), true, 4],
    "raincloud": [preload("res://Scenes/Entities/RainCloud/raincloud.tscn"), true, 2],
    "raincloud_tiny": [preload("res://Scenes/Entities/RainCloud/raincloud_tiny.tscn"), false, 1],
    "aqua_fish": [preload("res://Scenes/Entities/AquaFish/aqua_fish.tscn"), false, 1],
    "metal_spawn": [preload("res://Scenes/Entities/MetalSpawn/metal_spawn.tscn"), true, 10],
    "ambient_bird": [preload("res://Scenes/Entities/AmbientEnts/bird.tscn"), true, 24],
    "void_portal": [preload("res://Scenes/Entities/VoidPortal/void_portal.tscn"), true, 2],


    "picnic": [preload("res://Scenes/Entities/Props/prop_picnic.tscn"), false, 1],
    "canvas": [preload("res://Scenes/Entities/Props/prop_canvas.tscn"), false, 1],
    "bush": [preload("res://Scenes/Entities/Props/prop_bush.tscn"), false, 1],
    "rock": [preload("res://Scenes/Entities/Props/prop_rock.tscn"), false, 1],
    "fish_trap": [preload("res://Scenes/Entities/Props/prop_fish_trap.tscn"), false, 1],
    "fish_trap_ocean": [preload("res://Scenes/Entities/Props/prop_fish_trap_ocean.tscn"), false, 1],
    "island_tiny": [preload("res://Scenes/Entities/Props/prop_island_tiny_spawn.tscn"), false, 1],
    "island_med": [preload("res://Scenes/Entities/Props/prop_island_med_spawn.tscn"), false, 1],
    "island_big": [preload("res://Scenes/Entities/Props/prop_island_big_spawn.tscn"), false, 1],
    "boombox": [preload("res://Scenes/Entities/Props/prop_boombox.tscn"), false, 1],
    "well": [preload("res://Scenes/Entities/Props/prop_well.tscn"), false, 1],
    "campfire": [preload("res://Scenes/Entities/Props/prop_campfire.tscn"), false, 1],
    "chair": [preload("res://Scenes/Entities/Props/prop_chair.tscn"), false, 4],
    "table": [preload("res://Scenes/Entities/Props/prop_table.tscn"), false, 1],
    "therapist_chair": [preload("res://Scenes/Entities/Props/prop_therapist_chair.tscn"), false, 1],
    "toilet": [preload("res://Scenes/Entities/Props/prop_toilet.tscn"), false, 1],
    "whoopie": [preload("res://Scenes/Entities/Props/prop_whoopie.tscn"), false, 1],
    "beer": [preload("res://Scenes/Entities/Props/prop_beer.tscn"), false, 1],
    "greenscreen": [preload("res://Scenes/Entities/Props/prop_greenscreen.tscn"), false, 1],
    "portable_bait": [preload("res://Scenes/Entities/Props/prop_portable_bait.tscn"), false, 1],
}
 */
public sealed class ActorType
{
    public string Name { get; }
    public bool HostOnly { get; }

    private ActorType(string name, bool host)
    {
        Name = name;
        HostOnly = host;
        ActorTypes.TryAdd(name, this);
    }
    
    public override string ToString() => Name;

    private static readonly Dictionary<string, ActorType> ActorTypes = [];
    public static ActorType? GetActorType(string name) => ActorTypes.GetValueOrDefault(name);
    
    public static ActorType None { get; } = new("none", true);
    public static ActorType Player { get; } = new("player", false);
    public static ActorType FishSpawn { get; } = new("fish_spawn", true);
    public static ActorType FishSpawnAlien { get; } = new("fish_spawn_alien", true);
    public static ActorType RainCloud { get; } = new("raincloud", true);
    public static ActorType RainCloudTiny { get; } = new("raincloud_tiny", false);
    public static ActorType AquaFish { get; } = new("aqua_fish", false);
    public static ActorType MetalSpawn { get; } = new("metal_spawn", true);
    public static ActorType AmbientBird { get; } = new("ambient_bird", true);
    public static ActorType VoidPortal { get; } = new("void_portal", true);
    
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