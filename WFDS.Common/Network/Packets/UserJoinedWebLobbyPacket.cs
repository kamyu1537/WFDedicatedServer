using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class UserJoinedWebLobbyPacket : Packet
{
    public long UserId { get; set; }
    
    public override void Deserialize(Dictionary<object, object> data)
    {
        UserId = data.GetInt("user_id");
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "user_joined_weblobby");
        data.TryAdd("user_id", UserId);
    }
}