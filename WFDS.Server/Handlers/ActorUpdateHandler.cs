using WFDS.Server.Common;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("actor_update")]
public class ActorUpdateHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new ActorUpdatePacket();
        packet.Parse(data);

        ActorManager.SelectActor(packet.ActorId, (actor) =>
        {
            actor.Position = packet.Position;
            actor.Rotation = packet.Rotation;
        });
    }
}