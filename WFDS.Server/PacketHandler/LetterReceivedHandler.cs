using System.Globalization;
using Steamworks;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using Session = WFDS.Common.Network.Session;

namespace WFDS.Server.Handlers;

[PacketType("letter_recieved")]
public class LetterReceivedHandler(ILogger<LetterReceivedHandler> logger, ISessionManager sessionManager) : PacketHandler<LetterReceivedPacket>
{
    protected override void Handle(Session sender, NetChannel channel, LetterReceivedPacket packet)
    {
        if (packet.To != SteamUser.GetSteamID().m_SteamID.ToString(CultureInfo.InvariantCulture))
            return;

        logger.LogDebug("received letter from {Sender} ({From} -> {To}) on channel {Channel} / {Header}: {Body} - {Closing} {User}", sender.SteamId, packet.Data.From, packet.Data.To, channel, packet.Data.Header, packet.Data.Body, packet.Data.Closing, packet.Data.User);

        packet.Data.LetterId = new Random().Next();
        (packet.Data.From, packet.Data.To) = (packet.To, packet.Data.From);
        packet.To = packet.Data.To;

        sessionManager.SendP2PPacket(sender.SteamId, NetChannel.GameState, packet);
    }
}