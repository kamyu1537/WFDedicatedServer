namespace WFDS.Godot.Extension;

public static class NumericExtension
{
    public static bool IsInt(this long value)
    {
        return value is >= int.MinValue and <= int.MaxValue;
    }

    public static bool IsFloat(this double value)
    {
        return value is >= float.MinValue and <= float.MaxValue;
    }
}