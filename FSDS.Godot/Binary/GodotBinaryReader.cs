using System.Text;
using FSDS.Godot.Types;

namespace FSDS.Godot.Binary;

// ReSharper disable InconsistentNaming
public sealed class GodotBinaryReader(Stream stream) : IDisposable
{
    private BinaryReader Reader { get; } = new(stream);

    public bool TryRead(out object? result)
    {
        result = null;
        try
        {
            result = Read();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }

        return true;
    }

    private object? Read()
    {
        var header = ReadHeader();

        if (header == GodotType.Null)
        {
            return null;
        }
        else if (header == GodotType.Bool)
        {
            return ReadBool();
        }
        else if (header == GodotType.Int32)
        {
            return ReadInt32();
        }
        else if (header == GodotType.Int64)
        {
            return ReadInt64();
        }

        else if (header == GodotType.Float)
        {
            return ReadFloat();
        }
        else if (header == GodotType.Double)
        {
            return ReadDouble();
        }
        else if (header == GodotType.String)
        {
            return ReadString();
        }
        else if (header == GodotType.Vector2)
        {
            return ReadVector2();
        }
        else if (header == GodotType.Rect2)
        {
            return ReadRect2();
        }
        else if (header == GodotType.Vector3)
        {
            return ReadVector3();
        }
        else if (header == GodotType.Transform2D)
        {
            return ReadTransform2D();
        }
        else if (header == GodotType.Plane)
        {
            return ReadPlane();
        }
        else if (header == GodotType.Quaternion)
        {
            return ReadQuaternion();
        }
        else if (header == GodotType.AABB)
        {
            return ReadAABB();
        }
        else if (header == GodotType.Basis)
        {
            return ReadBasis();
        }
        else if (header == GodotType.Transform3D)
        {
            return ReadTransform3D();
        }
        else if (header == GodotType.Color)
        {
            return ReadColor();
        }
        else if (header == GodotType.NodePath)
        {
            return ReadNodePath();
        }
        else if (header == GodotType.Dictionary)
        {
            return ReadDictionary();
        }
        else if (header == GodotType.Array)
        {
            return ReadArray();
        }
        else if (header == GodotType.RawArray)
        {
            return ReadRawArray();
        }
        else if (header == GodotType.Int32Array)
        {
            return ReadInt32Array();
        }
        else if (header == GodotType.Int64Array)
        {
            return ReadInt64Array();
        }
        else if (header == GodotType.Float32Array)
        {
            return ReadFloat32Array();
        }
        else if (header == GodotType.Float64Array)
        {
            return ReadFloat64Array();
        }
        else if (header == GodotType.StringArray)
        {
            return ReadStringArray();
        }
        else if (header == GodotType.Vector2Array)
        {
            return ReadVector2Array();
        }
        else if (header == GodotType.Vector3Array)
        {
            return ReadVector3Array();
        }
        else if (header == GodotType.ColorArray)
        {
            return ReadColorArray();
        }
        else if (header == GodotType.Max)
        {
            return null;
        }

        throw new NotSupportedException();
    }

    private GodotType ReadHeader()
    {
        var header = Reader.ReadInt32();
        return GodotType.Get(header);
    }

    private static int GetPadding(int length)
    {
        return (4 - (length % 4)) % 4;
    }

    private bool ReadBool()
    {
        var value = Reader.ReadInt32();
        return value != 0;
    }

    private int ReadInt32()
    {
        return Reader.ReadInt32();
    }

    private long ReadInt64()
    {
        return Reader.ReadInt64();
    }

    private float ReadFloat()
    {
        return Reader.ReadSingle();
    }

    private double ReadDouble()
    {
        return Reader.ReadDouble();
    }

    private string ReadString()
    {
        var length = Reader.ReadInt32();
        return ReadString(length);
    }

    private string ReadString(int length)
    {
        var bytes = Reader.ReadBytes(length);
        var skipSize = GetPadding(length);
        Reader.ReadBytes(skipSize);
        return Encoding.UTF8.GetString(bytes);
    }

    private Vector2 ReadVector2()
    {
        var x = ReadFloat();
        var y = ReadFloat();
        return new Vector2(x, y);
    }

    private Rect2 ReadRect2()
    {
        var x = ReadFloat();
        var y = ReadFloat();
        var width = ReadFloat();
        var height = ReadFloat();
        return new Rect2(x, y, width, height);
    }

    private Vector3 ReadVector3()
    {
        var x = ReadFloat();
        var y = ReadFloat();
        var z = ReadFloat();
        return new Vector3(x, y, z);
    }

    private Transform2D ReadTransform2D()
    {
        var x = ReadVector2();
        var y = ReadVector2();
        var origin = ReadVector2();
        return new Transform2D(x, y, origin);
    }

    private Plane ReadPlane()
    {
        var normal = ReadVector3();
        var d = ReadFloat();
        return new Plane(normal, d);
    }

    private Quaternion ReadQuaternion()
    {
        var x = ReadFloat();
        var y = ReadFloat();
        var z = ReadFloat();
        var w = ReadFloat();
        return new Quaternion(x, y, z, w);
    }

    private AABB ReadAABB()
    {
        var position = ReadVector3();
        var size = ReadVector3();
        return new AABB(position, size);
    }

    private Basis ReadBasis()
    {
        var x = ReadVector3();
        var y = ReadVector3();
        var z = ReadVector3();
        return new Basis(x, y, z);
    }

    private Transform3D ReadTransform3D()
    {
        var x = ReadVector3();
        var y = ReadVector3();
        var z = ReadVector3();
        var origin = ReadVector3();
        return new Transform3D(x, y, z, origin);
    }

    private Color ReadColor()
    {
        var r = ReadFloat();
        var g = ReadFloat();
        var b = ReadFloat();
        var a = ReadFloat();
        return new Color(r, g, b, a);
    }

    private NodePath ReadNodePath()
    {
        var format = Reader.ReadInt32();
        var isNewFormat = ((uint)format & GodotBinaryConstant.NODE_PATH_NEW_FORMAT_FLAG) != 0;

        if (!isNewFormat)
        {
            var path = ReadString(format);
            return new NodePath(path);
        }

        var length = (int)((uint)format & GodotBinaryConstant.NODE_PATH_NEW_FORMAT_COUNT_MASK);
        var subNamesLength = ReadInt32();
        var absolute = ReadBool();

        var names = new string[length];
        for (var i = 0; i < length; ++i)
        {
            names[i] = ReadString();
        }

        var subNames = new string[subNamesLength];
        for (var i = 0; i < subNamesLength; ++i)
        {
            subNames[i] = ReadString();
        }

        return new NodePath(names, subNames, absolute);
    }

    private Dictionary<object, object> ReadDictionary()
    {
        var format = ReadInt32();
        _ = ((uint)format & GodotBinaryConstant.SHARED_FLAG) != 0;
        var count = (int)(format & GodotBinaryConstant.COUNT_MASK);

        var dictionary = new Dictionary<object, object>(count);

        for (var i = 0; i < count; ++i)
        {
            var key = Read();
            var value = Read();
            dictionary.Add(key!, value!);
        }

        return dictionary;
    }

    private List<object> ReadArray()
    {
        var format = ReadInt32();
        _ = ((uint)format & GodotBinaryConstant.SHARED_FLAG) != 0;
        var count = (int)(format & GodotBinaryConstant.COUNT_MASK);

        var array = new List<object>(count);
        for (var i = 0; i < count; ++i)
        {
            var value = Read();
            array.Add(value!);
        }

        return array;
    }

    private byte[] ReadRawArray()
    {
        var length = ReadInt32();
        var bytes = Reader.ReadBytes(length);
        var skipSize = GetPadding(length);
        Reader.ReadBytes(skipSize);
        return bytes;
    }

    private int[] ReadInt32Array()
    {
        var length = ReadInt32();
        var array = new int[length];

        for (var i = 0; i < length; ++i)
        {
            array[i] = ReadInt32();
        }

        return array;
    }

    private long[] ReadInt64Array()
    {
        var length = ReadInt32();
        var array = new long[length];

        for (var i = 0; i < length; ++i)
        {
            array[i] = ReadInt64();
        }

        return array;
    }

    private float[] ReadFloat32Array()
    {
        var length = ReadInt32();
        var array = new float[length];

        for (var i = 0; i < length; ++i)
        {
            array[i] = ReadFloat();
        }

        return array;
    }

    private double[] ReadFloat64Array()
    {
        var length = ReadInt32();
        var array = new double[length];

        for (var i = 0; i < length; ++i)
        {
            array[i] = ReadDouble();
        }

        return array;
    }

    private string[] ReadStringArray()
    {
        var length = ReadInt32();
        var array = new string[length];

        for (var i = 0; i < length; ++i)
        {
            array[i] = ReadString();
        }

        return array;
    }

    private Vector2[] ReadVector2Array()
    {
        var length = ReadInt32();
        var array = new Vector2[length];

        for (var i = 0; i < length; ++i)
        {
            array[i] = ReadVector2();
        }

        return array;
    }

    private Vector3[] ReadVector3Array()
    {
        var length = ReadInt32();
        var array = new Vector3[length];

        for (var i = 0; i < length; ++i)
        {
            array[i] = ReadVector3();
        }

        return array;
    }

    private Color[] ReadColorArray()
    {
        var length = ReadInt32();
        var array = new Color[length];

        for (var i = 0; i < length; ++i)
        {
            array[i] = ReadColor();
        }

        return array;
    }

    public void Dispose()
    {
        Reader.Dispose();
    }
}