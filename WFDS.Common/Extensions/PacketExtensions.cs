using WFDS.Common.Network;

namespace WFDS.Common.Extensions;

public static class PacketExtensions
{
    public static Dictionary<object, object> ToDictionary(this Packet packet)
    {
        var data = new Dictionary<object, object>();
        packet.Serialize(data);
        return data;
    }
}