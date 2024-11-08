using FSDS.Server.Common;
using FSDS.Server.Packets;

namespace FSDS.Server.Handlers;

[PacketType("actor_action")]
public class ActorActionHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new ActorActionPacket();
        packet.Parse(data);
        
        if (packet.Action == "queue_free")
        {
            Logger.LogInformation("received queue_free for actor {ActorId}", packet.ActorId);
            ActorManager.RemoveActor(packet.ActorId);
        }
    }
}