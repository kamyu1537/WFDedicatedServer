using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using ZLogger;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("instance_actor")]
public class InstanceActorHandler(ILogger<InstanceActorHandler> logger, IActorManager actorManager, SessionManager sessionManager) : PacketHandler<InstanceActorPacket>
{
    protected override void Handle(Session sender, NetChannel channel, InstanceActorPacket packet)
    {
        logger.ZLogInformation($"received instance_actor from {sender} on channel {channel} / {packet.Param.ActorId} {packet.Param.ActorType}");

        if (packet.Param.ActorType == "player") CreatePlayerActor(sender, packet);
        else CreateRemoteActor(sender, packet);
    }

    private void CreateRemoteActor(Session sender, InstanceActorPacket packet)
    {
        var actorType = ActorType.GetActorType(packet.Param.ActorType);
        if (actorType == null)
        {
            logger.ZLogError($"actor type not found {packet.Param.ActorType} : {sender}");
            return;
        }

        if (actorType.HostOnly)
        {
            logger.ZLogWarning($"actor type {packet.Param.ActorType} is host only : {sender}", actorType.Name, sender.ToString());
            sessionManager.KickPlayer(sender.SteamId);
            return;
        }

        var created = actorManager.TryCreateRemoteActor(sender.SteamId, packet.Param.ActorId, actorType, packet.Param.Position, packet.Param.Rotation, out _);
        if (!created) logger.ZLogError($"failed to create remote actor {packet.Param.ActorId} {packet.Param.ActorType}");
    }

    private void CreatePlayerActor(Session sender, InstanceActorPacket packet)
    {
        if (actorManager.GetPlayerActor(sender.SteamId) != null)
        {
            logger.ZLogError($"player already has actor {sender} - {packet.Param.ActorId} {packet.Param.ActorType}");
            return;
        }

        var created = actorManager.TryCreatePlayerActor(sender.SteamId, packet.Param.ActorId, out _);
        if (created) return;

        logger.ZLogError($"failed to create player actor {packet.Param.ActorId} {packet.Param.ActorType}");
    }
}