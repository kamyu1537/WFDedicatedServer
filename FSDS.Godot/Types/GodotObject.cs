namespace FSDS.Godot.Types;

public record GodotObject(
    long InstanceId,
    string ClassName,
    Dictionary<string, object> Properties
);