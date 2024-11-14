﻿using WFDS.Common.ChannelEvents;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.PacketHandlers;

[PacketType("actor_update")]
public class ActorUpdateHandler(IActorManager actorManager) : PacketHandler<ActorUpdatePacket>
{
    protected override async Task HandlePacketAsync(IGameSession sender, NetChannel channel, ActorUpdatePacket packet)
    {
        var actor = actorManager.GetActor(packet.ActorId);
        if (actor == null) return;
        
        if (actor.CreatorId != sender.SteamId)
        {
            return;
        }
            
        actor.Position = packet.Position;
        actor.Rotation = packet.Rotation;
        
        await ChannelEvent.PublishAsync(new ActorUpdateEvent(actor.ActorId, packet.Position, packet.Rotation));
    }
}