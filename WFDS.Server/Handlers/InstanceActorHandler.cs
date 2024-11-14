using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("instance_actor")]
public class InstanceActorHandler(ILogger<InstanceActorHandler> logger, IActorManager actorManager) : PacketHandler<InstanceActorPacket>
{
    protected override async Task HandlePacketAsync(IGameSession sender, NetChannel channel, InstanceActorPacket packet)
    {
        logger.LogDebug("received instance_actor from {Sender} on channel {Channel} / {ActorId} {ActorType} ", sender.SteamId, channel, packet.Param.ActorId, packet.Param.ActorType);
        
        if (packet.Param.ActorType == "player") CreatePlayerActor(sender, packet);
        else CreateRemoteActor(sender, packet);
        await Task.Yield();
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
            logger.LogError("player request host actor {Member} - {ActorId} {ActorType}", sender.Friend, packet.Param.ActorId, packet.Param.ActorType);
            sender.Kick();
            return;
        }

        var created = actorManager.TryCreateRemoteActor(sender.SteamId, packet.Param.ActorId, packet.Param.ActorType, out _);
        if (!created)
        {
            logger.LogError("failed to create remote actor {ActorId} {ActorType}", packet.Param.ActorId, packet.Param.ActorType);
        }
    }

    private void CreatePlayerActor(IGameSession sender, InstanceActorPacket packet)
    {
        if (sender.ActorCreated)
        {
            logger.LogError("player actor already exists for {Member}", sender.Friend);
            return;
        }

        var created = actorManager.TryCreatePlayerActor(sender.SteamId, packet.Param.ActorId, out var actor);
        if (!created)
        {
            logger.LogError("failed to create player actor {ActorId} {ActorType}", packet.Param.ActorId, packet.Param.ActorType);
            return;
        }

        sender.ActorCreated = true;
        sender.Actor = actor;
    }
}