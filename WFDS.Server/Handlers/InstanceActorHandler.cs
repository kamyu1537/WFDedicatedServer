using WFDS.Common.Types;
using WFDS.Server.Common;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("instance_actor")]
public class InstanceActorHandler : PacketHandler
{
    public override void HandlePacket(ISession sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new InstanceActorPacket();
        packet.Parse(data);
        Logger.LogInformation("received instance_actor from {Sender} on channel {Channel} / {ActorId} {ActorType} ", sender.SteamId, channel, packet.ActorId, packet.ActorType);
        
        if (packet.ActorType == "player") CreatePlayerActor(sender, packet);
        else CreateRemoteActor(sender, packet);
    }
    
    private static readonly string[] OnlyHostActors =
    [
        "fish_spawn",
        "fish_spawn_alien",
        "void_portal",
        "raincloud",
        "ambient_bird",
        "metal_spawn"
    ];

    private static bool IsHostActor(InstanceActorPacket packet)
    {
        return OnlyHostActors.Contains(packet.ActorType);
    }

    private void CreateRemoteActor(ISession sender, InstanceActorPacket packet)
    {
        if (IsHostActor(packet))
        {
            Logger.LogError("player request host actor {PlayerName}[{SteamId}] - {ActorId} {ActorType}", sender.Friend.Name, sender.SteamId, packet.ActorId, packet.ActorType);
            sender.Kick();
            return;
        }
            
        var created = ActorManager?.TryCreateRemoteActor(sender.SteamId, packet.ActorId, packet.ActorType, out _) ?? false;
        if (!created)
        {
            Logger.LogError("failed to create remote actor {ActorId} {ActorType}", packet.ActorId, packet.ActorType);
        }
    }

    private void CreatePlayerActor(ISession sender, InstanceActorPacket packet)
    {
        if (sender.ActorCreated)
        {
            Logger.LogError("player actor already exists for {SteamId}", sender.SteamId);
            return;
        }

        IPlayerActor actor = null!;
        var created = ActorManager?.TryCreatePlayerActor(sender.SteamId, packet.ActorId, out actor) ?? false;
        if (!created)
        {
            Logger.LogError("failed to create player actor {ActorId} {ActorType}", packet.ActorId, packet.ActorType);
            return;
        }

        sender.ActorCreated = true;
        sender.Actor = actor;
    }
}