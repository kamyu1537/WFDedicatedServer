using WFDS.Server.Common;
using WFDS.Server.Common.Network;
using WFDS.Server.Common.Packet;

namespace WFDS.Server.Handlers;

[PacketType("new_player_join")]
public class NewPlayerJoinHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        Logger.LogInformation("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);

        ActorManager.SendAllOwnedActors(sender);
    }
}