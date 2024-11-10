using WFDS.Common.Types;
using WFDS.Godot.Types;
using WFDS.Server.Common.Extensions;
using WFDS.Server.Network;

namespace WFDS.Server.Packets;

public class MessagePacket : IPacket
{
    public string Message { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool Local { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public string Zone { get; set; } = string.Empty;
    public long ZoneOwner { get; set; }

    public void Parse(Dictionary<object, object> data)
    {
        Message = data.GetString("message");
        Color = data.GetString("color");
        Local = data.GetBool("local");
        Position = data.GetVector3("position");
        Zone = data.GetString("zone");
        ZoneOwner = data.GetInt("zone_owner");
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["type"] = "message",
            ["message"] = Message,
            ["color"] = Color,
            ["local"] = Local,
            ["position"] = Position,
            ["zone"] = Zone,
            ["zone_owner"] = ZoneOwner
        };
    }
}