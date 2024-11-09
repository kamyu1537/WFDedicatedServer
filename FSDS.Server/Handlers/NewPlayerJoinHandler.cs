using FSDS.Server.Common;
using FSDS.Server.Packets;
using Steamworks;

namespace FSDS.Server.Network.Handlers
{
    [PacketType("new_player_join")]
    public class NewPlayerJoinHandler : PacketHandler
    {
        public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
        {
            Logger.LogInformation("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);
        }
    }
}