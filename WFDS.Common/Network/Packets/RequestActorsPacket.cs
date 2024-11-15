using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class RequestActorsPacket : Packet
{
    public string UserId { get; set; } = string.Empty;

    public override void Deserialize(Dictionary<object, object> data)
    {
        UserId = data.GetString("user_id");
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "request_actors");
        data.TryAdd("user_id", UserId);
    }
}