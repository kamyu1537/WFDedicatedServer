﻿using System.Drawing;
using System.Numerics;
using WFDS.Common.Extensions;

namespace WFDS.Common.Network.Packets;

public class MessagePacket : Packet
{
    public string Message { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool Local { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public string Zone { get; set; } = string.Empty;
    public long ZoneOwner { get; set; }

    public override void Deserialize(Dictionary<object, object> data)
    {
        Message = data.GetString("message");
        Color = data.GetString("color");
        Local = data.GetBool("local");
        Position = data.GetVector3("position");
        Zone = data.GetString("zone");
        ZoneOwner = data.GetInt("zone_owner");
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "message");
        data.TryAdd("message", Message);
        data.TryAdd("color", Color);
        data.TryAdd("local", Local);
        data.TryAdd("position", Position);
        data.TryAdd("zone", Zone);
        data.TryAdd("zone_owner", ZoneOwner);
    }
    
    public static MessagePacket Create(string message, Color color, bool local = false)
    {
        return new MessagePacket
        {
            Message = message,
            Color = color.ToHex(true),
            Local = local,
            Position = Vector3.Zero,
            Zone = "",
            ZoneOwner = -1
        };
    }
}