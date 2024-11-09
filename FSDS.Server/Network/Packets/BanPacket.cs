﻿using FSDS.Server.Common;

namespace FSDS.Server.Packets;

public record BanPacket : IPacket
{
    public void Parse(Dictionary<object, object> data)
    {
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            { "type", "ban" }
        };
    }
}