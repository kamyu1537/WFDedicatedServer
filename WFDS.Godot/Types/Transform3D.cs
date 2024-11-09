namespace WFDS.Godot.Types;

public record Transform3D(
    Vector3 X,
    Vector3 Y,
    Vector3 Z,
    Vector3 Origin
)
{
    public static Transform3D Zero { get; } = new(Vector3.Right, Vector3.Up, Vector3.Forward, Vector3.Zero);
}