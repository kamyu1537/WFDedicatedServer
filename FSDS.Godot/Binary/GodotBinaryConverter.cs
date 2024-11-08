namespace FSDS.Godot.Binary;

public static class GodotBinaryConverter
{   
    public static byte[] Serialize(object? value)
    {
        return Serialize(value, GodotBinaryWriterOptions.Default);
    }
    
    private static byte[] Serialize(object? value, GodotBinaryWriterOptions options)
    {   
        using var stream = new MemoryStream();
        using var writer = new GodotBinaryWriter(stream, options);
        
        writer.Write(value);
        
        return stream.ToArray();
    }

    public static object? Deserialize(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var reader = new GodotBinaryReader(stream);

        if (!reader.TryRead(out var data))
        {
            throw new InvalidDataException("Failed to read Godot binary data.");
        }

        return data;
    }
}