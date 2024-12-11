using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("instance_actor")]
public sealed class InstanceActorHandler(ILogger<InstanceActorHandler> logger, IActorManager actorManager, SessionManager sessionManager) : PacketHandler<InstanceActorPacket>
{
    protected override void Handle(Session sender, NetChannel channel, InstanceActorPacket packet)
    {
        logger.LogInformation("received instance_actor from {Sender} on channel {Channel} / {ActorId} {ActorType}", sender, channel, packet.Param.ActorId, packet.Param.ActorType);

        if (packet.Param.ActorType == "player") CreatePlayerActor(sender, packet);
        else CreateRemoteActor(sender, packet);
    }

    private void CreateRemoteActor(Session sender, InstanceActorPacket packet)
    {
        var actorType = ActorType.GetActorType(packet.Param.ActorType);
        if (actorType == null)
        {
            logger.LogError("actor type not found {ActorType} : {Sender}", packet.Param.ActorType, sender);
            return;
        }

        if (actorType.HostOnly)
        {
            logger.LogWarning("actor type {ActorType} is host only : {Sender}", packet.Param.ActorType, sender);
            sessionManager.KickPlayer(sender.SteamId);
            return;
        }

        var created = actorManager.TryCreateRemoteActor(sender.SteamId, packet.Param.ActorId, actorType, packet.Param.Position, packet.Param.Rotation, out _);
        if (!created) logger.LogError("failed to create remote actor {ActorId} {ActorType}", packet.Param.ActorId, packet.Param.ActorType);
    }

    private void CreatePlayerActor(Session sender, InstanceActorPacket packet)
    {
        if (actorManager.GetPlayerActor(sender.SteamId) != null)
        {
            logger.LogError("player already has actor {Sender} - {ActorId} {ActorType}", sender, packet.Param.ActorId, packet.Param.ActorType);
            return;
        }

        var created = actorManager.TryCreatePlayerActor(sender.SteamId, packet.Param.ActorId, out _);
        if (created) return;

        logger.LogError("failed to create player actor {ActorId} {ActorType}", packet.Param.ActorId, packet.Param.ActorType);
    }
}