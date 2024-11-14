using System.Numerics;

namespace WFDS.Godot.Types;

public record struct Transform2D(
    Vector2 X,
    Vector2 Y,
    Vector2 Origin
);