using FSDS.Server.Common;
using FSDS.Server.Packets;

namespace FSDS.Server.Handlers;

[PacketType("instance_actor")]
public class InstanceActorHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        var packet = new InstanceActorPacket();
        packet.Parse(data);
        Logger.LogInformation("received instance_actor from {Sender} on channel {Channel} / {ActorId} {ActorType} ", sender.SteamId, channel, packet.ActorId, packet.ActorType);

        if (packet.ActorType == "player")
        {
            var created = ActorManager.CreatePlayerActor(sender.SteamId, packet.ActorId, sender.Friend.Name);
            if (!created)
            {
                Logger.LogError("failed to create player actor {ActorId} {ActorType}", packet.ActorId, packet.ActorType);
            }
        }
        else
        {
            var created = ActorManager.CreateRemoteActor(sender.SteamId, packet.ActorId, packet.ActorType);
            if (!created)
            {
                Logger.LogError("failed to create remote actor {ActorId} {ActorType}", packet.ActorId, packet.ActorType);
            }
        }
    }
}