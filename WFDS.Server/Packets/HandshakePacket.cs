using WFDS.Common.Extensions;
using WFDS.Common.Types;
using WFDS.Server.Network;

namespace WFDS.Server.Packets;

public class HandshakePacket : IPacket
{
    public string UserId { get; set; } = string.Empty;

    public void Parse(Dictionary<object, object> data)
    {
        UserId = data.GetString("user_id");
    }

    public void Write(Dictionary<object, object> data)
    {
        data.TryAdd("type", "handshake");
        data.TryAdd("user_id", UserId);
    }
}