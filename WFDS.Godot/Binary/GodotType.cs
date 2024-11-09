namespace WFDS.Godot.Binary;

// ReSharper disable InconsistentNaming
public record GodotType
{
    private static readonly Dictionary<int, GodotType> Map = new();
    public static GodotType Get(int header) => Map.GetValueOrDefault(header, Max);

    public int Value { get; }
    public GodotTypes Type { get; }
    public string Name { get; }
    
    private GodotType(GodotTypes type, bool is64 = false)
    {
        Value = (int)type;
        if (is64) Value |= 1 << 16;
        
        Type = type;
        Name = type.ToString();
        
        Map.Add(Value, this);
    }
    
    public static readonly GodotType Null = new(GodotTypes.Null);
    public static readonly GodotType Bool = new(GodotTypes.Bool);
    public static readonly GodotType Int32 = new(GodotTypes.Integer);
    public static readonly GodotType Int64 = new(GodotTypes.Integer, true);
    public static readonly GodotType Float = new(GodotTypes.Float);
    public static readonly GodotType Double = new(GodotTypes.Float, true);
    public static readonly GodotType String = new(GodotTypes.String);
    public static readonly GodotType Vector2 = new(GodotTypes.Vector2);
    public static readonly GodotType Rect2 = new(GodotTypes.Rect2);
    public static readonly GodotType Vector3 = new(GodotTypes.Vector3);
    public static readonly GodotType Transform2D = new(GodotTypes.Transform2D);
    public static readonly GodotType Plane = new(GodotTypes.Plane);
    public static readonly GodotType Quaternion = new(GodotTypes.Quaternion);
    public static readonly GodotType AABB = new(GodotTypes.AABB);
    public static readonly GodotType Basis = new(GodotTypes.Basis);
    public static readonly GodotType Transform3D = new(GodotTypes.Transform3D);
    public static readonly GodotType Color = new(GodotTypes.Color);
    public static readonly GodotType NodePath = new(GodotTypes.NodePath);
    public static readonly GodotType RID = new(GodotTypes.RID);
    public static readonly GodotType Object = new(GodotTypes.Object);
    public static readonly GodotType Dictionary = new(GodotTypes.Dictionary);
    public static readonly GodotType Array = new(GodotTypes.Array);
    public static readonly GodotType RawArray = new(GodotTypes.RawArray);
    public static readonly GodotType Int32Array = new(GodotTypes.Int32Array);
    public static readonly GodotType Int64Array = new(GodotTypes.Int64Array);
    public static readonly GodotType Float32Array = new(GodotTypes.Float32Array);
    public static readonly GodotType Float64Array = new(GodotTypes.Float64Array);
    public static readonly GodotType StringArray = new(GodotTypes.StringArray);
    public static readonly GodotType Vector2Array = new(GodotTypes.Vector2Array);
    public static readonly GodotType Vector3Array = new(GodotTypes.Vector3Array);
    public static readonly GodotType ColorArray = new(GodotTypes.ColorArray);
    public static readonly GodotType Max = new(GodotTypes.Max);
}

public static class GodotTypeExtensions
{
    public static void Write(this BinaryWriter writer, GodotType type)
    {
        writer.Write(type.Value);
    }
}