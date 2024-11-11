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
    
    public void Parse(Dictionary<object, object> data)
    {
        Id = data.GetString("id");
        Size = data.GetFloat("size");
        Ref = data.GetNumber("ref");
        var qualityDic = data.GetObjectDictionary("quality");
        Quality = QualityType.FromDictionary(qualityDic);
        Tags = data.GetObjectList("tags");
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["id"] = Id,
            ["size"] = Size,
            ["ref"] = Ref,
            ["quality"] = Quality,
            ["tags"] = Tags
        };
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