using System.Text;
using FSDS.Godot.Types;

// ReSharper disable InconsistentNaming

namespace FSDS.Godot.Binary;

public sealed class GodotBinaryWriter(Stream stream, GodotBinaryWriterOptions options) : IDisposable, IAsyncDisposable
{
    private BinaryWriter Writer { get; } = new(stream);
    private GodotBinaryWriterOptions Options { get; } = options;

    public void Write(object? obj)
    {
        switch (obj)
        {
            case null:
                WriteNull();
                break;
            case bool boolValue:
                WriteBool(boolValue);
                break;
            case int intValue:
                WriteInt32(intValue);
                break;
            case long longValue:
                WriteInt64(longValue);
                break;
            case float floatValue:
                WriteFloat32(floatValue);
                break;
            case double doubleValue:
                WriteFloat64(doubleValue);
                break;
            case string stringValue:
                WriteString(stringValue);
                break;
            case Vector2 vector2Value:
                WriteVector2(vector2Value);
                break;
            case Rect2 rect2Value:
                WriteRect2(rect2Value);
                break;
            case Vector3 vector3Value:
                WriteVector3(vector3Value);
                break;
            case Transform2D transform2DValue:
                WriteTransform2D(transform2DValue);
                break;
            case Plane planeValue:
                WritePlane(planeValue);
                break;
            case Quaternion quaternionValue:
                WriteQuaternion(quaternionValue);
                break;
            case AABB aabbValue:
                WriteAABB(aabbValue);
                break;
            case Basis basisValue:
                WriteBasis(basisValue);
                break;
            case Transform3D transform3DValue:
                WriteTransform3D(transform3DValue);
                break;
            case Color colorValue:
                WriteColor(colorValue);
                break;
            case NodePath nodePathValue:
                WriteNodePath(nodePathValue);
                break;
            case Dictionary<object, object> dictionaryValue:
                WriteDictionary(dictionaryValue);
                break;
            case List<object> arrayValue:
                WriteArray(arrayValue);
                break;
            case byte[] byteArrayValue:
                WriteByteArray(byteArrayValue);
                break;
            case int[] int32ArrayValue:
                WriteInt32Array(int32ArrayValue);
                break;
            case long[] int64ArrayValue:
                WriteInt64Array(int64ArrayValue);
                break;
            case float[] float32ArrayValue:
                WriteFloat32Array(float32ArrayValue);
                break;
            case double[] float64ArrayValue:
                WriteFloat64Array(float64ArrayValue);
                break;
            case string[] stringArrayValue:
                WriteStringArray(stringArrayValue);
                break;
            case Vector2[] vector2ArrayValue:
                WriteVector2Array(vector2ArrayValue);
                break;
            case Vector3[] vector3ArrayValue:
                WriteVector3Array(vector3ArrayValue);
                break;
            case Color[] colorArrayValue:
                WriteColorArray(colorArrayValue);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private void WriteNull()
    {
        Writer.Write(GodotType.Null);
    }

    private void WriteBool(bool value)
    {
        Writer.Write(GodotType.Bool);
        Writer.Write(value ? 1 : 0);
    }

    private void WriteInt32(int value)
    {
        Writer.Write(GodotType.Int32);
        Writer.Write(value);
    }

    private void WriteInt64(long value)
    {
        Writer.Write(GodotType.Int64);
        Writer.Write(value);
    }

    private void WriteFloat32(float value)
    {
        Writer.Write(GodotType.Float);
        Writer.Write(value);
    }

    private void WriteFloat64(double value)
    {
        Writer.Write(GodotType.Double);
        Writer.Write(value);
    }

    private void WriteString(string value)
    {
        Writer.Write(GodotType.String);
        WriteStringWithoutType(value);
    }
    
    private void WriteStringWithoutType(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Writer.Write(bytes.Length);
        Writer.Write(bytes);
        WritePadding(bytes.Length);
    }
    
    private void WritePadding(int length)
    {
        var padding = (4 - length % 4) % 4;
        Writer.Write(new byte[padding]);
    }

    private void WriteVector2(Vector2 value)
    {
        Writer.Write(GodotType.Vector2);
        WriteVector2WithoutType(value);
    }

    private void WriteVector2WithoutType(Vector2 value)
    {
        Writer.Write(value.X);
        Writer.Write(value.Y);
    }

    private void WriteRect2(Rect2 rect)
    {
        Writer.Write(GodotType.Rect2);
        Writer.Write(rect.X);
        Writer.Write(rect.Y);
        Writer.Write(rect.Width);
        Writer.Write(rect.Height);
    }

    private void WriteVector3(Vector3 value)
    {
        Writer.Write(GodotType.Vector3);
        WriteVector3WithoutType(value);
    }

    private void WriteVector3WithoutType(Vector3 value)
    {
        Writer.Write(value.X);
        Writer.Write(value.Y);
        Writer.Write(value.Z);
    }

    private void WriteTransform2D(Transform2D value)
    {
        Writer.Write(GodotType.Transform2D);
        Writer.Write(value.X.X);
        Writer.Write(value.X.Y);
        Writer.Write(value.Y.X);
        Writer.Write(value.Y.Y);
        Writer.Write(value.Origin.X);
        Writer.Write(value.Origin.Y);
    }

    private void WritePlane(Plane value)
    {
        Writer.Write(GodotType.Plane);
        Writer.Write(value.Normal.X);
        Writer.Write(value.Normal.Y);
        Writer.Write(value.Normal.Z);
        Writer.Write(value.Distance);
    }

    private void WriteQuaternion(Quaternion value)
    {
        Writer.Write(GodotType.Quaternion);
        Writer.Write(value.X);
        Writer.Write(value.Y);
        Writer.Write(value.Z);
        Writer.Write(value.W);
    }

    private void WriteAABB(AABB value)
    {
        Writer.Write(GodotType.AABB);
        Writer.Write(value.Position.X);
        Writer.Write(value.Position.Y);
        Writer.Write(value.Position.Z);
        Writer.Write(value.Size.X);
        Writer.Write(value.Size.Y);
        Writer.Write(value.Size.Z);
    }

    private void WriteBasis(Basis value)
    {
        Writer.Write(GodotType.Basis);
        Writer.Write(value.X.X);
        Writer.Write(value.X.Y);
        Writer.Write(value.X.Z);
        Writer.Write(value.Y.X);
        Writer.Write(value.Y.Y);
        Writer.Write(value.Y.Z);
        Writer.Write(value.Z.X);
        Writer.Write(value.Z.Y);
        Writer.Write(value.Z.Z);
    }

    private void WriteTransform3D(Transform3D value)
    {
        Writer.Write(GodotType.Transform3D);
        Writer.Write(value.X.X);
        Writer.Write(value.X.Y);
        Writer.Write(value.X.Z);
        Writer.Write(value.Y.X);
        Writer.Write(value.Y.Y);
        Writer.Write(value.Y.Z);
        Writer.Write(value.Z.X);
        Writer.Write(value.Z.Y);
        Writer.Write(value.Z.Z);
        Writer.Write(value.Origin.X);
        Writer.Write(value.Origin.Y);
        Writer.Write(value.Origin.Z);
    }

    private void WriteColor(Color value)
    {
        Writer.Write(GodotType.Color);
        WriteColorWithoutType(value);
    }

    private void WriteColorWithoutType(Color value)
    {
        Writer.Write(value.R);
        Writer.Write(value.G);
        Writer.Write(value.B);
        Writer.Write(value.A);
    }

    private void WriteNodePath(NodePath value)
    {
        Writer.Write(GodotType.NodePath);

        if (Options.NodePathOldFormat)
        {
            WriteStringWithoutType(value.ToString());
            return;
        }

        var length = (uint)value.Names.Length;
        var format = length | GodotBinaryConstant.NODE_PATH_NEW_FORMAT_FLAG;

        Writer.Write((int)format);
        Writer.Write(value.SubNames.Length);
        Writer.Write(value.Absolute ? 1 : 0);

        foreach (var name in value.Names)
        {
            WriteStringWithoutType(name);
        }

        foreach (var subName in value.SubNames)
        {
            WriteStringWithoutType(subName);
        }
    }
    
    private void WriteDictionary(Dictionary<object, object> value, bool shared = false)
    {
        Writer.Write(GodotType.Dictionary);

        var count = (uint)value.Count;
        if (shared)
        {
            count |= GodotBinaryConstant.SHARED_FLAG;
        }
        Writer.Write((int)count);
        
        foreach (var (key, property) in value)
        {
            Write(key);
            Write(property);
        }
    }
    
    private void WriteArray(List<object> value, bool shared = false)
    {
        Writer.Write(GodotType.Array);

        var count = (uint)value.Count;
        if (shared)
        {
            count |= GodotBinaryConstant.SHARED_FLAG;
        }
        Writer.Write((int)count);
        
        foreach (var item in value)
        {
            Write(item);
        }
    }
    
    private void WriteByteArray(byte[] value)
    {
        Writer.Write(GodotType.RawArray);
        Writer.Write(value.Length);
        Writer.Write(value);
        WritePadding(value.Length);
    }
    
    private void WriteInt32Array(int[] value)
    {
        Writer.Write(GodotType.Int32Array);
        Writer.Write(value.Length);
        foreach (var item in value)
        {
            Writer.Write(item);
        }
    }
    
    private void WriteInt64Array(long[] value)
    {
        Writer.Write(GodotType.Int64Array);
        Writer.Write(value.Length);
        foreach (var item in value)
        {
            Writer.Write(item);
        }
    }
    
    private void WriteFloat32Array(float[] value)
    {
        Writer.Write(GodotType.Float32Array);
        Writer.Write(value.Length);
        foreach (var item in value)
        {
            Writer.Write(item);
        }
    }
    
    private void WriteFloat64Array(double[] value)
    {
        Writer.Write(GodotType.Float64Array);
        Writer.Write(value.Length);
        foreach (var item in value)
        {
            Writer.Write(item);
        }
    }
    
    private void WriteStringArray(string[] value)
    {
        Writer.Write(GodotType.StringArray);
        Writer.Write(value.Length);
        foreach (var item in value)
        {
            WriteStringWithoutType(item);
        }
    }
    
    private void WriteVector2Array(Vector2[] value)
    {
        Writer.Write(GodotType.Vector2Array);
        Writer.Write(value.Length);
        foreach (var item in value)
        {
            WriteVector2WithoutType(item);
        }
    }
    
    private void WriteVector3Array(Vector3[] value)
    {
        Writer.Write(GodotType.Vector3Array);
        Writer.Write(value.Length);
        foreach (var item in value)
        {
            WriteVector3WithoutType(item);
        }
    }
    
    private void WriteColorArray(Color[] value)
    {
        Writer.Write(GodotType.ColorArray);
        Writer.Write(value.Length);
        foreach (var item in value)
        {
            WriteColorWithoutType(item);
        }
    }

    public void Dispose()
    {
        Writer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Writer.DisposeAsync();
    }
}