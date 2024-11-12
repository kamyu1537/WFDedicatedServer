using Microsoft.Extensions.ObjectPool;
using WFDS.Common.Types;
using WFDS.Godot.Binary;

namespace WFDS.Common.Helpers;

public static class PacketHelper
{
    
    public static T FromDictionary<T>(Dictionary<object, object> data) where T : IPacket, new()
    {
        var packet = new T();
        packet.Parse(data);
        data.Clear();
        return packet;
    }
}