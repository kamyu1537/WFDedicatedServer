using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class UserLeftWebLobbyPacket : Packet
{
    public long UserId { get; set; }
    public long Reason { get; set; }

    public override void Deserialize(Dictionary<object, object> data)
    {
        UserId = data.GetInt("user_id");
        Reason = data.GetInt("reason");
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "user_left_weblobby");
        data.TryAdd("user_id", UserId);
        data.TryAdd("reason", (int)Reason);
    }
}