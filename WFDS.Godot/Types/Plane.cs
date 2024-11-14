using System.Numerics;

namespace WFDS.Godot.Types;

public record Plane(
    Vector3 Normal,
    float Distance
);