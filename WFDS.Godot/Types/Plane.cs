using System.Numerics;

namespace WFDS.Godot.Types;

public record struct Plane(
    Vector3 Normal,
    float Distance
);