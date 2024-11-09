using System.Drawing;
using WFDS.Server.Common;

namespace WFDS.Server.Network.Handlers
{
    [PacketType("new_player_join")]
    public class NewPlayerJoinHandler : PacketHandler
    {
        public override void HandlePacket(Session sender, NetChannel channel, Dictionary<object, object> data)
        {
            Logger.LogInformation("received new_player_join from {Sender} on channel {Channel}", sender.SteamId, channel);

            ActorManager.SendAllOwnedActors(sender);

            // var code = LobbyManager.Code;
            // sender.SendMessage($"Welcome to the server!\nServer Code: {code}\n서버에 오신것을 환영합니다!\n접속 코드: {code}", Color.Fuchsia);
        }
    }
}