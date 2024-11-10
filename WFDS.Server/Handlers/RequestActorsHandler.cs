using WFDS.Server.Common;
using WFDS.Server.Common.Actor;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("request_actors")]
public class RequestActorsHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        Logger.LogInformation("received request_actors from {Sender} on channel {Channel}", sender.SteamId, channel);

        var packet = new ActorRequestSendPacket();
        sender.SendPacket(NetChannel.GameState, packet);
    }
}