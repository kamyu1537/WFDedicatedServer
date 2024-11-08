namespace FSDS.Godot.Types;

public record Vector2(float X, float Y)
{
    public static readonly Vector2 Zero = new(0, 0);
    
    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X + b.X, a.Y + b.Y);
    }
    
    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X - b.X, a.Y - b.Y);
    }
    
    public static Vector2 operator *(Vector2 a, float b)
    {
        return new Vector2(a.X * b, a.Y * b);
    }

    public Vector2 Normalized()
    {
        var length = MathF.Sqrt(X * X + Y * Y);
        return new Vector2(X / length, Y / length);
    }

    public float Angle()
    {
        return MathF.Atan2(Y, X);
    }
    
    public Vector2 Rotate(float angle)
    {
        var x = X * MathF.Cos(angle) - Y * MathF.Sin(angle);
        var y = X * MathF.Sin(angle) + Y * MathF.Cos(angle);
        return new Vector2(x, y);
    }
}