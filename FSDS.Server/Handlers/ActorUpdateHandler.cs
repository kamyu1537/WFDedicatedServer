using FSDS.Server.Common;
using FSDS.Server.Packets;

namespace FSDS.Server.Handlers;

[PacketType("actor_update")]
public class ActorUpdateHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new ActorUpdatePacket();
        packet.Parse(data);

        ActorManager.UpdateActor(packet.ActorId, (actor) =>
        {
            actor.Position = packet.Position;
            actor.Rotation = packet.Rotation;
        });
    }
}