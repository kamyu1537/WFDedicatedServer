using System.Numerics;

namespace WFDS.Godot.Types;

public record struct Basis(
    Vector3 X,
    Vector3 Y,
    Vector3 Z
);