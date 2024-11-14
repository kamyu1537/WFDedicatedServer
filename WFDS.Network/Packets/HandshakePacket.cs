using WFDS.Common.Extensions;
using WFDS.Common.Types;

namespace WFDS.Network.Packets;

public class HandshakePacket : IPacket
{
    public string UserId { get; set; } = string.Empty;

    public void Deserialize(Dictionary<object, object> data)
    {
        UserId = data.GetString("user_id");
    }

    public void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "handshake");
        data.TryAdd("user_id", UserId);
    }
}