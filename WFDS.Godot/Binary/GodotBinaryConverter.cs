using System.Buffers;

namespace WFDS.Godot.Binary;

public static class GodotBinaryConverter
{
    public static Memory<byte> Serialize(object? value)
    {
        return Serialize(value, GodotBinaryWriterOptions.Default);
    }

    private static Memory<byte> Serialize(object? value, GodotBinaryWriterOptions options)
    {
        using var stream = new MemoryStream();
        using var writer = new GodotBinaryWriter(stream, options);
        writer.Write(value);
        return new Memory<byte>(stream.GetBuffer(), 0, (int)stream.Length);
    }

    public static unsafe object? Deserialize(Memory<byte> input)
    {
        fixed (byte* ptr = input.Span)
        {
            using var stream = new UnmanagedMemoryStream(ptr, input.Length);
            using var reader = new GodotBinaryReader(stream);
            if (!reader.TryRead(out var data))
            {
                throw new InvalidDataException("Failed to read Godot binary data.");
            }
            return data;
        }
    }
}