using System.Drawing;

namespace WFDS.Common.Extensions;

public static class ColorExtensions
{
    public static string ToHex(this Color color, bool withAlpha = false)
    {
        return (withAlpha
            ? $"{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}"
            : $"{color.R:X2}{color.G:X2}{color.B:X2}").ToLower();
    }
}