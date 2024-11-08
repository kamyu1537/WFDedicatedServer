namespace FSDS.Server.Common;

public enum NetChannels
{
    ActorUpdate,
    ActorAction,
    GameState,
    Chalk,
    Guitar,
    ActorAnimation,
    Speech,
}

public class NetChannel
{
    public static NetChannel ActorUpdate { get; } = new(NetChannels.ActorUpdate, "actor_update");
    public static NetChannel ActorAction { get; } = new(NetChannels.ActorAction, "actor_action");
    public static NetChannel GameState { get; } = new(NetChannels.GameState, "game_state");
    public static NetChannel Chalk { get; } = new(NetChannels.Chalk, "chalk");
    public static NetChannel Guitar { get; } = new(NetChannels.Guitar, "guitar");
    public static NetChannel ActorAnimation { get; } = new(NetChannels.ActorAnimation, "actor_animation");
    public static NetChannel Speech { get; } = new(NetChannels.Speech, "speech");
    
    public int Value { get; }
    public string Name { get; }
    
    private NetChannel(NetChannels type, string name)
    {
        Value = (int)type;
        Name = name;
    }
    
    public override string ToString()
    {
        return Name;
    }
}