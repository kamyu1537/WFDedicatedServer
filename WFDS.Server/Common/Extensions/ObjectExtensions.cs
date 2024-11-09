namespace WFDS.Server.Common.Extensions;

public static class ObjectExtensions
{
    public static long GetNumber(this object obj)
    {
        return obj switch
        {
            int intValue => intValue,
            long longValue => longValue,
            _ => 0
        };
    }

    public static string GetString(this object obj)
    {
        return obj switch
        {
            string strValue => strValue,
            _ => string.Empty
        };
    }
}