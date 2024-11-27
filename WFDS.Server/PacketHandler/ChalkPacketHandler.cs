using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;

namespace WFDS.Server.PacketHandler;

[PacketType("chalk_packet")]
public class ChalkPacketHandler(ICanvasManager canvas, LobbyManager lobby, SessionManager session) : PacketHandler<ChalkPacket>
{
    protected override void Handle(Session sender, NetChannel channel, ChalkPacket packet)
    {
        canvas.Draw(packet);
        session.BroadcastP2PPacket(lobby.GetLobbyId(), NetChannel.Chalk, packet);
    }
}