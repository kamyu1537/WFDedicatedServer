namespace WFDS.Godot.Binary;

public sealed class GodotBinaryWriterOptions
{
    public static GodotBinaryWriterOptions Default { get; } = new();
    
    public bool NodePathOldFormat { get; init; } = false;
}