using Steamworks;

namespace WFDS.Common.Types;

public enum NetChannels
{
    ActorUpdate,
    ActorAction,
    GameState,
    Chalk,
    Guitar,
    ActorAnimation,
    Speech
}

public class NetChannel
{
    public static readonly NetChannel ActorUpdate = new(NetChannels.ActorUpdate, "actor_update");
    public static readonly NetChannel ActorAction = new(NetChannels.ActorAction, "actor_action");
    public static readonly NetChannel GameState = new(NetChannels.GameState, "game_state");
    public static readonly NetChannel Chalk = new(NetChannels.Chalk, "chalk");
    public static readonly NetChannel Guitar = new(NetChannels.Guitar, "guitar");
    public static readonly NetChannel ActorAnimation = new(NetChannels.ActorAnimation, "actor_animation");
    public static readonly NetChannel Speech = new(NetChannels.Speech, "speech");

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