namespace WFDS.Godot.Types;

public record struct Vector3(float X, float Y, float Z)
{
    public static Vector3 Zero => new(0, 0, 0);
    public static Vector3 One => new(1, 1, 1);
    public static Vector3 Up => new(0, 1, 0);
    public static Vector3 Down => new(0, -1, 0);
    public static Vector3 Left => new(-1, 0, 0);
    public static Vector3 Right => new(1, 0, 0);
    public static Vector3 Forward => new(0, 0, 1);
    public static Vector3 Backward => new(0, 0, -1);
    
    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    
    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    
    public static Vector3 operator *(Vector3 a, float b)
    {
        return new Vector3(a.X * b, a.Y * b, a.Z * b);
    }
    
    public Vector3 Normalized()
    {
        var length = MathF.Sqrt(X * X + Y * Y + Z * Z);
        return new Vector3(X / length, Y / length, Z / length);
    }
    
    public float Distance(Vector3 other)
    {
        return MathF.Sqrt(MathF.Pow(other.X - X, 2) + MathF.Pow(other.Y - Y, 2) + MathF.Pow(other.Z - Z, 2));
    }
}