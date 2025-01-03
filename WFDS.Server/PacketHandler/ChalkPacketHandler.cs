using WFDS.Common.Extensions;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Database.DbSet;
using WFDS.Server.Core;

namespace WFDS.Server.PacketHandler;

[PacketType("chalk_packet")]
public sealed class ChalkPacketHandler(ICanvasManager canvas, SessionManager session, PlayerLogManager playerLogManager) : PacketHandler<ChalkPacket>
{
    protected override void Handle(Session sender, NetChannel channel, ChalkPacket packet)
    {
        canvas.Draw(packet);
        session.BroadcastPacket(NetChannel.Chalk, packet);
        playerLogManager.Append(sender, "draw", $"canvas_{packet.CanvasId}", new
        {
            canvas_id = packet.CanvasId,
            data = packet.GetData().Select(x => new { pos = new { x = x.pos.X, y = x.pos.Y }, color = x.color })
        });
    }
}