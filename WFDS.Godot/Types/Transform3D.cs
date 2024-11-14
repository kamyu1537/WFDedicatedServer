using System.Numerics;

namespace WFDS.Godot.Types;

public record Transform3D(
    Vector3 X,
    Vector3 Y,
    Vector3 Z,
    Vector3 Origin
)
{
    public static Transform3D Identity { get; } = new(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ, Vector3.Zero);
}