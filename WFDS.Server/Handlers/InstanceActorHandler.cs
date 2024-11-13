using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("instance_actor")]
public class InstanceActorHandler : PacketHandler<InstanceActorPacket>
{
    protected override void HandlePacket(IGameSession sender, NetChannel channel, InstanceActorPacket packet)
    {
        Logger.LogInformation("received instance_actor from {Sender} on channel {Channel} / {ActorId} {ActorType} ", sender.SteamId, channel, packet.Param.ActorId, packet.Param.ActorType);
        
        if (packet.Param.ActorType == "player") CreatePlayerActor(sender, packet);
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
        return OnlyHostActors.Contains(packet.Param.ActorType);
    }

    private void CreateRemoteActor(IGameSession sender, InstanceActorPacket packet)
    {
        if (IsHostActor(packet))
        {
            Logger.LogError("player request host actor {Member} - {ActorId} {ActorType}", sender.Friend, packet.Param.ActorId, packet.Param.ActorType);
            sender.Kick();
            return;
        }
            
        var created = ActorManager?.TryCreateRemoteActor(sender.SteamId, packet.Param.ActorId, packet.Param.ActorType, out _) ?? false;
        if (!created)
        {
            Logger.LogError("failed to create remote actor {ActorId} {ActorType}", packet.Param.ActorId, packet.Param.ActorType);
        }
    }

    private void CreatePlayerActor(IGameSession sender, InstanceActorPacket packet)
    {
        if (sender.ActorCreated)
        {
            Logger.LogError("player actor already exists for {Member}", sender.Friend);
            return;
        }

        IPlayerActor actor = null!;
        var created = ActorManager?.TryCreatePlayerActor(sender.SteamId, packet.Param.ActorId, out actor) ?? false;
        if (!created)
        {
            Logger.LogError("failed to create player actor {ActorId} {ActorType}", packet.Param.ActorId, packet.Param.ActorType);
            return;
        }

        sender.ActorCreated = true;
        sender.Actor = actor;
    }
}