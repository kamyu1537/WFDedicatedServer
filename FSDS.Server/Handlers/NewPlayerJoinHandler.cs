using System.Drawing;
using FSDS.Server.Common;

namespace FSDS.Server.Network.Handlers
{
    [PacketType("new_player_join")]
    public class NewPlayerJoinHandler : PacketHandler
    {
        public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
        {
            Logger.LogInformation("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);

            var code = LobbyManager.Code;
            sender.SendMessage($"Welcome to the server!\nServer Code: {code}\n서버에 오신것을 환영합니다!\n서버 접속 코드: {code}", Color.Fuchsia);
        }
    }
}