using WFDS.Common.Extensions;
using WFDS.Common.Types;
using WFDS.Godot.Types;

namespace WFDS.Server.Packets;

public class MessagePacket : IPacket
{
    public string Message { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool Local { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public string Zone { get; set; } = string.Empty;
    public long ZoneOwner { get; set; }

    public void Deserialize(Dictionary<object, object> data)
    {
        Message = data.GetString("message");
        Color = data.GetString("color");
        Local = data.GetBool("local");
        Position = data.GetVector3("position");
        Zone = data.GetString("zone");
        ZoneOwner = data.GetInt("zone_owner");
    }

    public void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "message");
        data.TryAdd("message", Message);
        data.TryAdd("color", Color);
        data.TryAdd("local", Local);
        data.TryAdd("position", Position);
        data.TryAdd("zone", Zone);
        data.TryAdd("zone_owner", ZoneOwner);
    }
}