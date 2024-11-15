using WFDS.Common.Extensions;

namespace WFDS.Common.Types;

// {"id": "empty_hand", "ref": 0, "size": 1.0, "quality": ITEM_QUALITIES.NORMAL, "tags": []}
public class GameItem : IPacket
{
    public string Id { get; set; } = string.Empty;
    public float Size { get; set; }
    public long Ref { get; set; }
    public QualityType Quality { get; set; } = QualityType.Normal;
    public List<object> Tags { get; set; } = [];
    
    public void Deserialize(Dictionary<object, object> data)
    {
        Id = data.GetString("id");
        Size = (float)data.GetFloat("size");
        Ref = data.GetInt("ref");
        Tags = data.GetObjectList("tags");

        var qualityValue = data.GetInt("quality");
        Quality = QualityType.Types.TryGetValue(qualityValue, out var quality) ? quality : QualityType.Normal;
    }

    public void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("id", Id);
        data.TryAdd("size", Size);
        data.TryAdd("ref", Ref);
        data.TryAdd("quality", Quality.Value);
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