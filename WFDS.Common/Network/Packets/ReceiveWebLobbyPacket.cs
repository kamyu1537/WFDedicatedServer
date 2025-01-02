using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class ReceiveWebLobbyPacket : Packet
{
    public List<object> WebLobbyMembers { get; set; }

    public override void Deserialize(Dictionary<object, object> data)
    {
        WebLobbyMembers = data.GetObjectList("weblobby");
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "receive_weblobby");
        data.TryAdd("weblobby", WebLobbyMembers);
    }
}