﻿using WFDS.Common.Extensions;
using WFDS.Common.Helpers;
using WFDS.Common.Types;

namespace WFDS.Server.Packets;

public class LetterData : IPacket
{
    public static readonly LetterData Default = new();
    
    public long LetterId { get; set; }
    public string To { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Closing { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public List<object> Items { get; set; } = [];
    
    public void Parse(Dictionary<object, object> data)
    {
        LetterId = data.GetInt("letter_id");
        To = data.GetString("to");
        From = data.GetString("from");
        Header = data.GetString("header");
        Body = data.GetString("body");
        Closing = data.GetString("closing");
        User = data.GetString("user");
        Items = data.GetObjectList("items");
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["letter_id"] = LetterId,
            ["to"] = To,
            ["from"] = From,
            ["header"] = Header,
            ["body"] = Body,
            ["closing"] = Closing,
            ["user"] = User,
            ["items"] = Items
        };
    }
}

public class LetterReceivedPacket : IPacket
{
    public string To { get; set; } = string.Empty;
    public LetterData Data { get; set; } = LetterData.Default;

    public void Parse(Dictionary<object, object> data)
    {
        To = data.GetString("to");
        Data = PacketHelper.FromDictionary<LetterData>(data.GetObjectDictionary("data"));
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["type"] = "letter_recieved",
            ["to"] = To,
            ["data"] = Data.ToDictionary()
        };
    }
}