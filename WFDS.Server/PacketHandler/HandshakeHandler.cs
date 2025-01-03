using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.PacketHandler;

[PacketType("handshake")]
public sealed class HandshakeHandler(ILogger<HandshakeHandler> logger, SessionManager sessionManager) : PacketHandler<HandshakePacket>
{
    protected override void Handle(Session sender, NetChannel channel, HandshakePacket packet)
    {
        logger.LogDebug("received handshake from {Sender} : {UserId}", sender, packet.UserId);

        sender.HandshakeReceived = true;
        sender.HandshakeReceiveTime = DateTimeOffset.UtcNow;

        var webLobbyPacket = ReceiveWebLobbyPacket.Create(sessionManager.GetSessions().Select(x => x.SteamId).ToList());
        sessionManager.SendPacket(sender.SteamId, NetChannel.GameState, webLobbyPacket, false);
    }
}