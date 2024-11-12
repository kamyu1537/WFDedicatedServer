using WFDS.Common.Types;

namespace WFDS.Server.Packets;

public class ServerClosePacket : IPacket
{
    public void Parse(Dictionary<object, object> data)
    {
    }

    public Action? Write(Dictionary<object, object> data)
    {
        data.TryAdd("type", "server_close");

        return null;
    }
}