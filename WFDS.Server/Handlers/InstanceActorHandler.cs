using WFDS.Server.Common;
using WFDS.Server.Common.Network;
using WFDS.Server.Common.Packet;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

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
            if (sender.ActorCreated)
            {
                Logger.LogError("player actor already exists for {SteamId}", sender.SteamId);
                return;
            }

            var created = ActorManager.CreatePlayerActor(sender.SteamId, packet.ActorId, sender.Friend.Name, out var actor);
            if (!created)
            {
                Logger.LogError("failed to create player actor {ActorId} {ActorType}", packet.ActorId, packet.ActorType);
                return;
            }

            sender.ActorCreated = true;
            sender.Actor = actor;
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