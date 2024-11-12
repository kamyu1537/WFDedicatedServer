using WFDS.Common.Types;

namespace WFDS.Common.Extensions;

public static class PacketExtensions
{
    public static Dictionary<object, object> ToDictionary(this IPacket packet)
    {
        var data = new Dictionary<object, object>();
        packet.Write(data);
        return data;
    }
}