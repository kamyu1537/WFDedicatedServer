using WFDS.Common.Extensions;
using WFDS.Common.Helpers;

namespace WFDS.Common.Types;

// {"id": "empty_hand", "ref": 0, "size": 1.0, "quality": ITEM_QUALITIES.NORMAL, "tags": []}
public class GameItem : IPacket
{
    public string Id { get; set; } = string.Empty;
    public float Size { get; set; }
    public long Ref { get; set; }
    public QualityType Quality { get; set; } = QualityType.Normal;
    public List<object> Tags { get; set; } = [];
    
    public void Parse(Dictionary<object, object> data)
    {
        Id = data.GetString("id");
        Size = (float)data.GetFloat("size");
        Ref = data.GetInt("ref");
        Tags = data.GetObjectList("tags");
        
        Quality = PacketHelper.FromDictionary<QualityType>(data.GetObjectDictionary("quality"));
    }

    public void Write(Dictionary<object, object> data)
    {
        data.TryAdd("id", Id);
        data.TryAdd("size", Size);
        data.TryAdd("ref", Ref);
        data.TryAdd("quality", Quality);
        data.TryAdd("tags", Tags);
    }

    public static GameItem Default => new()
    {
        Id = "empty_hand",
        Size = 1.0f,
        Ref = 0,
        Quality = QualityType.Normal,
        Tags = []
    };
}