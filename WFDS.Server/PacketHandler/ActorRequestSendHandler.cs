using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using ZLogger;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("actor_request_send")]
public class ActorRequestSendHandler(ILogger<ActorRequestSendHandler> logger, IActorManager actorManager, SessionManager sessionManager) : PacketHandler<ActorRequestSendPacket>
{
    protected override void Handle(Session sender, NetChannel channel, ActorRequestSendPacket packet)
    {
        foreach (var actor in packet.Actors)
        {
            if (actorManager.GetActor(actor.ActorId) != null)
            {
                logger.ZLogWarning($"actor {actor.ActorId} already exists : {sender}");
                continue;
            }

            if (actor.ActorType == "player") continue; // 여기에서 플레이어는 생성하면 안됨!!

            logger.LogDebug($"received actor_request_send from {sender} : {actor.ActorId} {actor.ActorType}");

            var actorType = ActorType.GetActorType(actor.ActorType);
            if (actorType == null)
            {
                logger.ZLogWarning($"actor type {actor.ActorType} not found : {sender}");
                continue;
            }

            if (actorType.HostOnly)
            {
                logger.ZLogWarning($"actor type {actor.ActorType} is host only : {sender}");
                sessionManager.KickPlayer(sender.SteamId);
                break;
            }

            actorManager.TryCreateRemoteActor(sender.SteamId, actor.ActorId, actorType, Vector3.Zero, Vector3.Zero, out _);
        }

        var player = actorManager.GetPlayerActor(sender.SteamId);
        if (player != null) player.ReceiveReplication = true;
    }
}