using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("instance_actor")]
internal class InstanceActorHandler(ILogger<InstanceActorHandler> logger, IActorManager actorManager) : PacketHandler<InstanceActorPacket>
{
    protected override async Task HandlePacketAsync(Session sender, NetChannel channel, InstanceActorPacket packet)
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

    private void CreateRemoteActor(Session sender, InstanceActorPacket packet)
    {
        if (IsHostActor(packet))
        {
            logger.LogError("player request host actor {Member} - {ActorId} {ActorType}", sender.Friend, packet.Param.ActorId, packet.Param.ActorType);
            
            return;
        }

        var created = actorManager.TryCreateRemoteActor(sender.SteamId, packet.Param.ActorId, packet.Param.ActorType, packet.Param.Position, packet.Param.Rotation, out _);
        if (!created)
        {
            logger.LogError("failed to create remote actor {ActorId} {ActorType}", packet.Param.ActorId, packet.Param.ActorType);
        }
    }

    private void CreatePlayerActor(Session sender, InstanceActorPacket packet)
    {
        if (actorManager.GetPlayerActor(sender.SteamId) != null)
        {
            logger.LogError("player already has actor {Member} - {ActorId} {ActorType}", sender.Friend, packet.Param.ActorId, packet.Param.ActorType);
            return;
        }
        
        var created = actorManager.TryCreatePlayerActor(sender.SteamId, packet.Param.ActorId, out _);
        if (created) return;
        
        logger.LogError("failed to create player actor {ActorId} {ActorType}", packet.Param.ActorId, packet.Param.ActorType);
    }
}