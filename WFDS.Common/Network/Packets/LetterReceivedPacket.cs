using System.Globalization;
using Steamworks;
using WFDS.Common.Extensions;
using WFDS.Common.Steam;
using WFDS.Common.Types;

namespace WFDS.Common.Network.Packets;

public class LetterData : Packet
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
    
    public override void Deserialize(Dictionary<object, object> data)
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

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("letter_id", LetterId);
        data.TryAdd("to", To);
        data.TryAdd("from", From);
        data.TryAdd("header", Header);
        data.TryAdd("body", Body);
        data.TryAdd("closing", Closing);
        data.TryAdd("user", User);
        data.TryAdd("items", Items);
    }
}

public class LetterReceivedPacket : Packet
{
    public string To { get; set; } = string.Empty;
    public LetterData Data { get; set; } = LetterData.Default;

    public override void Deserialize(Dictionary<object, object> data)
    {
        To = data.GetString("to");
        Data = PacketHelper.FromDictionary<LetterData>(data.GetObjectDictionary("data"));
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "letter_recieved");
        data.TryAdd("to", To);
        data.TryAdd("data", Data.ToDictionary());
    }
    
    public static LetterReceivedPacket Create(CSteamID to, string body, List<GameItem> items)
    {
        var steamId = SteamManager.Inst.SteamId;
        return new LetterReceivedPacket
        {
            To = to.m_SteamID.ToString(CultureInfo.InvariantCulture),
            Data = new LetterData
            {
                LetterId = new Random().Next(),
                To = steamId.m_SteamID.ToString(CultureInfo.InvariantCulture),
                From = steamId.m_SteamID.ToString(CultureInfo.InvariantCulture),
                Closing = "From, ",
                User = "[SERVER]",
                Header = "Letter",
                Body = body,
                Items = items.Select(object (x) => PacketHelper.ToDictionary(x)).ToList()
            }
        };
    }
}