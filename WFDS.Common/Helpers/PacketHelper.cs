using Microsoft.Extensions.ObjectPool;
using WFDS.Common.Policies;
using WFDS.Common.Types;
using WFDS.Godot.Binary;

namespace WFDS.Common.Helpers;

public static class PacketHelper
{
    public static ObjectPool<Dictionary<object, object>> Pool { get; } = new DefaultObjectPool<Dictionary<object, object>>(new DictionaryPooledObjectPolicy());
    
    public static T FromDictionary<T>(Dictionary<object, object> data) where T : IPacket, new()
    {
        var packet = new T();
        packet.Parse(data);
        data.Clear();
        return packet;
    }
}