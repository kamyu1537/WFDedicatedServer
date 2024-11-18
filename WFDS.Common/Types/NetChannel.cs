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
    public static readonly NetChannel ActorUpdate = new(NetChannels.ActorUpdate, EP2PSend.k_EP2PSendUnreliable, "actor_update");
    public static readonly NetChannel ActorAction = new(NetChannels.ActorAction, EP2PSend.k_EP2PSendReliable, "actor_action");
    public static readonly NetChannel GameState = new(NetChannels.GameState, EP2PSend.k_EP2PSendReliable, "game_state");
    public static readonly NetChannel Chalk = new(NetChannels.Chalk, EP2PSend.k_EP2PSendReliable, "chalk");
    public static readonly NetChannel Guitar = new(NetChannels.Guitar, EP2PSend.k_EP2PSendReliable, "guitar");
    public static readonly NetChannel ActorAnimation = new(NetChannels.ActorAnimation, EP2PSend.k_EP2PSendUnreliable, "actor_animation");
    public static readonly NetChannel Speech = new(NetChannels.Speech, EP2PSend.k_EP2PSendReliable, "speech");

    public int Value { get; }
    public EP2PSend SendType { get; }
    public string Name { get; }

    private NetChannel(NetChannels type, EP2PSend sendType, string name)
    {
        Value = (int)type;
        Name = name;
        SendType = sendType;
    }

    public override string ToString()
    {
        return Name;
    }
}