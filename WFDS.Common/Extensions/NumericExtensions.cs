using System.Numerics;
using Steamworks;

namespace WFDS.Common.Extensions;

public static class NumericExtensions
{
    public static float Angle(this Vector2 v) => MathF.Atan2(v.Y, v.X);

    private const float Deg2Rad = MathF.PI / 180f;

    public static Vector2 Rotated(this Vector2 v, float angle)
    {
        var rad = angle * Deg2Rad;
        var cos = MathF.Cos(rad);
        var sin = MathF.Sin(rad);

        return new Vector2(
            v.X * cos - v.Y * sin,
            v.X * sin + v.Y * cos
        );
    }

    public static dynamic ToDynamic(this Vector3 value) => new
    {
        value.X,
        value.Y,
        value.Z
    };
    
    public static CSteamID ToSteamID(this ulong value) => new(value);
}