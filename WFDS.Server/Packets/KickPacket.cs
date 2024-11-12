using WFDS.Common.Types;

namespace WFDS.Server.Packets;

public record KickPacket : IPacket
{
    public void Parse(Dictionary<object, object> data)
    {
    }

    public Action? Write(Dictionary<object, object> data)
    {
        data.TryAdd("type", "kick");

        return null;
    }
}