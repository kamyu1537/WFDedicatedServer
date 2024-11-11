using WFDS.Common.Extensions;

namespace WFDS.Common.Types;

// {"species": "species_cat", "pattern": "pattern_none", "primary_color": "pcolor_white", "secondary_color": "scolor_tan", "hat": "hat_none", "undershirt": "shirt_none", "overshirt": "overshirt_none", "title": "title_rank_1", "bobber": "bobber_default", "eye": "eye_halfclosed", "nose": "nose_cat", "mouth": "mouth_default", "accessory": [], "tail": "tail_cat", "legs": "legs_none"}
public class Cosmetics : IPacket
{
    public string Species { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string Hat { get; set; } = string.Empty;
    public string Undershirt { get; set; } = string.Empty;
    public string Overshirt { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Bobber { get; set; } = string.Empty;
    public string Eye { get; set; } = string.Empty;
    public string Nose { get; set; } = string.Empty;
    public string Mouth { get; set; } = string.Empty;
    public List<object> Accessory { get; set; } = [];
    public string Tail { get; set; } = string.Empty;
    public string Legs { get; set; } = string.Empty;
    
    public void Parse(Dictionary<object, object> data)
    {
        Species = data.GetString("species");
        Pattern = data.GetString("pattern");
        PrimaryColor = data.GetString("primary_color");
        SecondaryColor = data.GetString("secondary_color");
        Hat = data.GetString("hat");
        Undershirt = data.GetString("undershirt");
        Overshirt = data.GetString("overshirt");
        Title = data.GetString("title");
        Bobber = data.GetString("bobber");
        Eye = data.GetString("eye");
        Nose = data.GetString("nose");
        Mouth = data.GetString("mouth");
        Accessory = data.GetObjectList("accessory");
        Tail = data.GetString("tail");
        Legs = data.GetString("legs");
    }

    public Dictionary<object, object> ToDictionary()
    {
        return new Dictionary<object, object>
        {
            ["species"] = Species,
            ["pattern"] = Pattern,
            ["primary_color"] = PrimaryColor,
            ["secondary_color"] = SecondaryColor,
            ["hat"] = Hat,
            ["undershirt"] = Undershirt,
            ["overshirt"] = Overshirt,
            ["title"] = Title,
            ["bobber"] = Bobber,
            ["eye"] = Eye,
            ["nose"] = Nose,
            ["mouth"] = Mouth,
            ["accessory"] = Accessory,
            ["tail"] = Tail,
            ["legs"] = Legs
        };
    }

    public static Cosmetics Default => new()
    {
        Species = "species_cat",
        Pattern = "pattern_none",
        PrimaryColor = "pcolor_white",
        SecondaryColor = "scolor_tan",
        Hat = "hat_none",
        Undershirt = "shirt_none",
        Overshirt = "overshirt_none",
        Title = "title_rank_1",
        Bobber = "bobber_default",
        Eye = "eye_halfclosed",
        Nose = "nose_cat",
        Mouth = "mouth_default",
        Accessory = [],
        Tail = "tail_cat",
        Legs = "legs_none"
    };
}