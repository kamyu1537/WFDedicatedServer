using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("actor_request_send")]
public class ActorRequestSendHandler : PacketHandler<ActorRequestSendPacket>
{
    protected override void HandlePacket(ISession sender, NetChannel channel, ActorRequestSendPacket packet)
    { 
        foreach (var actor in packet.Actors)
        {
            if (actor.ActorType == "player") continue; // 여기에서 플레이어는 생성하면 안됨!!
            Logger.LogInformation("received actor_request_send from {Member} : {Actor} {ActorType}", sender.Friend, actor.ActorId, actor.ActorType);
            ActorManager?.TryCreateRemoteActor(sender.SteamId, actor.ActorId, actor.ActorType, out _);
        }
    }
}