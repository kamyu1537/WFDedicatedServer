using Steamworks;
using WFDS.Common.Types;
using WFDS.Server.Network;
using WFDS.Server.Packets;

namespace WFDS.Server.Handlers;

[PacketType("letter_recieved")]
public class LetterReceivedHandler : PacketHandler<LetterReceivedPacket>
{
    protected override void HandlePacket(IGameSession sender, NetChannel channel, LetterReceivedPacket packet)
    {
        if (packet.To != SteamClient.SteamId.Value.ToString())
            return;

        Logger.LogDebug("received letter from {Sender} ({From} -> {To}) on channel {Channel} / {Header}: {Body} - {Closing} {User}", sender.SteamId, packet.Data.From, packet.Data.To, channel, packet.Data.Header, packet.Data.Body, packet.Data.Closing, packet.Data.User);

        packet.Data.LetterId = new Random().Next();
        (packet.Data.From, packet.Data.To) = (packet.To, packet.Data.From);
        packet.To = packet.Data.To;

        sender.SendP2PPacket(NetChannel.GameState, packet);
    }
}