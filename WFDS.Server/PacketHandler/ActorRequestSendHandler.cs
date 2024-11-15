using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("actor_request_send")]
internal class ActorRequestSendHandler(ILogger<ActorRequestSendHandler> logger, IActorManager actorManager, ISessionManager sessionManager) : PacketHandler<ActorRequestSendPacket>
{
    protected override async Task HandlePacketAsync(Session sender, NetChannel channel, ActorRequestSendPacket packet)
    { 
        foreach (var actor in packet.Actors)
        {
            if (actor.ActorType == "player") continue; // 여기에서 플레이어는 생성하면 안됨!!
            logger.LogDebug("received actor_request_send from {Member} : {Actor} {ActorType}", sender.Friend, actor.ActorId, actor.ActorType);

            var actorType = ActorType.GetActorType(actor.ActorType);
            if (actorType == null)
            {
                logger.LogWarning("actor type {ActorType} not found : {Member}", actor.ActorType, sender.Friend);
                continue;
            }

            if (actorType.HostOnly)
            {
                logger.LogWarning("actor type {ActorType} is host only : {Member}", actor.ActorType, sender.Friend);
                sessionManager.KickPlayer(sender.SteamId);
                break;
            }
            
            actorManager.TryCreateRemoteActor(sender.SteamId, actor.ActorId, actorType, Vector3.Zero, Vector3.Zero, out _);
        }

        await Task.Yield();
    }
}