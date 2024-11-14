using WFDS.Common.Types;

namespace WFDS.Common.Network;

public static class PacketHelper
{
    public static T FromDictionary<T>(Dictionary<object, object> data) where T : IPacket, new()
    {
        var packet = new T();
        packet.Deserialize(data);
        data.Clear();
        return packet;
    }
    
    public static Dictionary<object, object> ToDictionary<T>(T packet) where T : IPacket
    {
        var data = new Dictionary<object, object>();
        packet.Serialize(data);
        return data;
    }
}