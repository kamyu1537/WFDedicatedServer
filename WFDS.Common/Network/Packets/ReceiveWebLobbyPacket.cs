using Steamworks;
using WFDS.Common.Extensions;
using WFDS.Common.Steam;

namespace WFDS.Common.Network.Packets;

public class ReceiveWebLobbyPacket : Packet
{
    public List<object> WebLobbyMembers { get; set; } = [];

    public override void Deserialize(Dictionary<object, object> data)
    {
        WebLobbyMembers = data.GetObjectList("weblobby");
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "receive_weblobby");
        data.TryAdd("weblobby", WebLobbyMembers);
    }
    
    public static ReceiveWebLobbyPacket Create(List<CSteamID> webLobbyMembers)
    {
        return new ReceiveWebLobbyPacket
        {
            WebLobbyMembers = [
                (long)SteamManager.Inst.SteamId.m_SteamID,
                ..webLobbyMembers.Select(object (x) => (long)x.m_SteamID)
            ]
        };
    }
}