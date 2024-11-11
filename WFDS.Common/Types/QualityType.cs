using WFDS.Common.Extensions;

namespace WFDS.Common.Types;

public class QualityType : IPacket
{
    public string Color { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float Diff { get; set; } = 0.0f;
    public float BDiff { get; set; } = 0.0f;
    public float Worth { get; set; } = 0.0f;
    public string Mod { get; set; } = string.Empty;
    public float Op { get; set; } = 0.0f;
    public long Particle { get; set; } = 0;
    public string Title { get; set; } = string.Empty;

    public void Parse(Dictionary<object, object> data)
    {
        Color = data.GetString("color");
        Name = data.GetString("name");
        Diff = data.GetFloat("diff");
        BDiff = data.GetFloat("b_diff");
        Worth = data.GetFloat("worth");
        Mod = data.GetString("mod");
        Op = data.GetFloat("op");
        Particle = data.GetNumber("particle");
        Title = data.GetString("title");
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["color"] = Color,
            ["name"] = Name,
            ["diff"] = Diff,
            ["b_diff"] = BDiff,
            ["worth"] = Worth,
            ["mod"] = Mod,
            ["op"] = Op,
            ["particle"] = Particle,
            ["title"] = Title
        };
    }

    private QualityType()
    {
    }

    private static QualityType Create(string color, string name, float diff, float bDiff, float worth, string mod, float op, long particle, string title)
    {
        var type = new QualityType
        {
            Color = color,
            Name = name,
            Diff = diff,
            BDiff = bDiff,
            Worth = worth,
            Mod = mod,
            Op = op,
            Particle = particle,
            Title = title
        };

        return type;
    }

    public static QualityType FromDictionary(Dictionary<object, object> data)
    {
        var fishQualityData = new QualityType();
        fishQualityData.Parse(data);
        return fishQualityData;
    }


    // {"color": "#d5aa73", "name": "", "diff": 1.0, "bdiff": 0.0, "worth": 1.0, "mod": "#ffffff", "op": 1.0, "particle": - 1, "title": "Normal "},
    public static readonly QualityType Normal = Create("#d5aa73", "", 1.0f, 0.0f, 1.0f, "#ffffff", 1.0f, -1, "Normal");

    // {"color": "#d5aa73", "name": "Shining ", "diff": 1.5, "bdiff": 3.0, "worth": 1.8, "mod": "#e5f5f0", "op": 1.0, "particle": 0, "title": "Shining "},
    public static readonly QualityType Shining = Create("#d5aa73", "Shining ", 1.5f, 3.0f, 1.8f, "#e5f5f0", 1.0f, 0, "Shining ");

    // {"color": "#a49d9c", "name": "Glistening ", "diff": 2.5, "bdiff": 8.0, "worth": 4.0, "mod": "#eafcf5", "op": 1.0, "particle": 1, "title": "Glistening "},
    public static readonly QualityType Glistening = Create("#a49d9c", "Glistening ", 2.5f, 8.0f, 4.0f, "#eafcf5", 1.0f, 1, "Glistening ");

    // {"color": "#008583", "name": "Opulent ", "diff": 4.0, "bdiff": 14.0, "worth": 6.0, "mod": "#d5fcf5", "op": 1.0, "particle": 2, "title": "Opulent "},
    public static readonly QualityType Opulent = Create("#008583", "Opulent ", 4.0f, 14.0f, 6.0f, "#d5fcf5", 1.0f, 2, "Opulent ");

    // {"color": "#e69d00", "name": "Radiant ", "diff": 5.0, "bdiff": 24.0, "worth": 10.0, "mod": "#fcf0d5", "op": 1.0, "particle": 3, "title": "Radiant "},
    public static readonly QualityType Radiant = Create("#e69d00", "Radiant ", 5.0f, 24.0f, 10.0f, "#fcf0d5", 1.0f, 3, "Radiant ");

    // {"color": "#cd0462", "name": "Alpha ", "diff": 5.5, "bdiff": 32.0, "worth": 15.0, "mod": "#fcd5df", "op": 1.0, "particle": 4, "title": "Alpha "},
    public static readonly QualityType Alpha = Create("#cd0462", "Alpha ", 5.5f, 32.0f, 15.0f, "#fcd5df", 1.0f, 4, "Alpha ");
}