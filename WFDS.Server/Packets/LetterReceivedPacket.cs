using WFDS.Common.Types;
using WFDS.Server.Common.Extensions;
using WFDS.Server.Network;

namespace WFDS.Server.Packets;

public class LetterReceivedPacket : IPacket
{
    public long LatterId { get; set; }
    public string To { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Closing { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public List<object> Items { get; set; } = [];

    public void Parse(Dictionary<object, object> data)
    {
        LatterId = data.GetNumber("letter_id");
        To = data.GetString("to");

        var dic = data.GetObjectDictionary("data");
        From = dic.GetString("from");
        Header = dic.GetString("header");
        Body = dic.GetString("body");
        Closing = dic.GetString("closing");
        User = dic.GetString("user");
        Items = dic.GetObjectList("items");
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["type"] = "letter_recieved",
            ["to"] = To,
            ["data"] = new Dictionary<object, object>
            {
                ["letter_id"] = LatterId,
                ["to"] = To,
                ["from"] = From,
                ["header"] = Header,
                ["body"] = Body,
                ["closing"] = Closing,
                ["user"] = User,
                ["items"] = Items
            }
        };
    }
}