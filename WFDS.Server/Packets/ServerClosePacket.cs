using WFDS.Server.Network;

namespace WFDS.Server.Packets;

public class ServerClosePacket : IPacket
{
    public void Parse(Dictionary<object, object> data)
    {
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["type"] = "server_close"
        };
    }
}