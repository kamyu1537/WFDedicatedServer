using System.Numerics;
using WFDS.Common.Actor;
using WFDS.Common.Network;
using WFDS.Common.Types;
using WFDS.Common.Network.Packets;
using ISession = WFDS.Common.Types.ISession;

namespace WFDS.Server.PacketHandlers;

[PacketType("actor_request_send")]
public class ActorRequestSendHandler(ILogger<ActorRequestSendHandler> logger, IActorManager actorManager) : PacketHandler<ActorRequestSendPacket>
{
    protected override async Task HandlePacketAsync(ISession sender, NetChannel channel, ActorRequestSendPacket packet)
    { 
        foreach (var actor in packet.Actors)
        {
            if (actor.ActorType == "player") continue; // 여기에서 플레이어는 생성하면 안됨!!
            logger.LogDebug("received actor_request_send from {Member} : {Actor} {ActorType}", sender.Friend, actor.ActorId, actor.ActorType);
            actorManager.TryCreateRemoteActor(sender.SteamId, actor.ActorId, actor.ActorType, Vector3.Zero, Vector3.Zero, out _);
        }

        await Task.Yield();
    }
}