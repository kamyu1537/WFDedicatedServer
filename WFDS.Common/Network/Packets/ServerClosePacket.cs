using WFDS.Common.Types;

namespace WFDS.Common.Network.Packets;

public class ServerClosePacket : IPacket
{
    public void Deserialize(Dictionary<object, object> data)
    {
    }

    public void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "server_close");
    }
}