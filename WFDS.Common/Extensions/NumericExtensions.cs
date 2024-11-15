﻿using System.Numerics;

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
}