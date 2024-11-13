namespace WFDS.Godot.Types;

public record struct Transform3D(
    Vector3 X,
    Vector3 Y,
    Vector3 Z,
    Vector3 Origin
)
{
    public static Transform3D Identity { get; } = new(Vector3.Right, Vector3.Up, Vector3.Forward, Vector3.Zero);
}