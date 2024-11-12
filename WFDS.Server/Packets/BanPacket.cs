﻿using WFDS.Common.Types;
using WFDS.Server.Network;

namespace WFDS.Server.Packets;

public record BanPacket : IPacket
{
    public void Parse(Dictionary<object, object> data)
    {
    }

    public Action? Write(Dictionary<object, object> data)
    {
        data.TryAdd("type", "ban");

        return null;
    }
}