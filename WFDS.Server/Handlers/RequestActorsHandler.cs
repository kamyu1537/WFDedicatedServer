using WFDS.Server.Common;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("request_actors")]
public class RequestActorsHandler : PacketHandler
{
    public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
    {
        Logger.LogInformation("received request_actors from {Sender} on channel {Channel}", sender.SteamId, channel);
        sender.Send(NetChannel.GameState, new ActorRequestSendPacket());
    }
}