using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("actor_request_send")]
public sealed class ActorRequestSendHandler(ILogger<ActorRequestSendHandler> logger, IActorManager actorManager, SessionManager sessionManager) : PacketHandler<ActorRequestSendPacket>
{
    protected override void Handle(Session sender, NetChannel channel, ActorRequestSendPacket packet)
    {
        foreach (var actor in packet.Actors)
        {
            if (actorManager.GetActor(actor.ActorId) != null)
            {
                logger.LogWarning("actor {ActorId} already exists : {Sender}", actor.ActorId, sender);
                continue;
            }

            if (actor.ActorType == "player") continue; // 여기에서 플레이어는 생성하면 안됨!!

            logger.LogDebug("received actor_request_send from {Sender} : {ActorId} {ActorType}", sender, actor.ActorId, actor.ActorType);

            var actorType = ActorType.GetActorType(actor.ActorType);
            if (actorType == null)
            {
                logger.LogWarning("actor type {ActorType} not found : {Sender}", actor.ActorType, sender);
                continue;
            }

            if (actorType.HostOnly)
            {
                logger.LogWarning("actor type {ActorType} is host only : {Sender}", actor.ActorType, sender);
                sessionManager.KickPlayer(sender.SteamId);
                break;
            }

            actorManager.TryCreateRemoteActor(sender.SteamId, actor.ActorId, actorType, Vector3.Zero, Vector3.Zero, out _);
        }

        var player = actorManager.GetPlayerActor(sender.SteamId);
        if (player != null) player.ReceiveReplication = true;
    }
}