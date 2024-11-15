using WFDS.Common.Actor;
using WFDS.Common.ChannelEvents;
using WFDS.Common.ChannelEvents.Events;
using WFDS.Common.Network;
using WFDS.Common.Types;
using WFDS.Common.Network.Packets;
using ISession = WFDS.Common.Types.ISession;

namespace WFDS.Server.PacketHandlers;

[PacketType("actor_update")]
public class ActorUpdateHandler(IActorManager actorManager) : PacketHandler<ActorUpdatePacket>
{
    protected override async Task HandlePacketAsync(ISession sender, NetChannel channel, ActorUpdatePacket packet)
    {
        var actor = actorManager.GetActor(packet.ActorId);
        if (actor == null) return;
        
        if (actor.CreatorId != sender.SteamId)
        {
            return;
        }
        
        await ChannelEventBus.PublishAsync(new ActorTransformUpdateEvent(actor.ActorId, packet.Position, packet.Rotation));
    }
}