using System.Globalization;
using System.Numerics;

namespace WFDS.Common.Extensions;

public static class DictionaryExtensions
{
    public static bool GetBool(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return false;

        return value is true;
    }

    public static long GetInt(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return 0;

        return value switch
        {
            int intValue => intValue,
            long longValue => longValue,
            _ => 0
        };
    }
    
    public static double GetFloat(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return 0;
        
        return value switch
        {
            float floatValue => floatValue,
            double doubleValue => doubleValue,
            _ => 0
        };
    }

    public static List<object> GetObjectList(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return [];

        return value as List<object> ?? [];
    }

    public static Dictionary<object, object> GetObjectDictionary(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return [];

        return value as Dictionary<object, object> ?? [];
    }

    public static Vector3 GetVector3(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return Vector3.Zero;

        if (value is Vector3 vector3)
        {
            return vector3;
        }

        return Vector3.Zero;
    }
    
    public static Vector2 GetVector2(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return Vector2.Zero;

        if (value is Vector2 vector2)
        {
            return vector2;
        }

        return Vector2.Zero;
    }

    public static string GetString(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return string.Empty;

        return value as string ?? string.Empty;
    }

    public static string[] GetStringArray(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return [];

        return value as string[] ?? [];
    }

    public static int GetParseInt(this Dictionary<object, object> dic, string key)
    {
        var value = dic.GetString(key);
        return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0;
    }

    public static long GetParseLong(this Dictionary<object, object> dic, string key)
    {
        var value = dic.GetString(key);
        return long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0;
    }

    public static ulong GetParseULong(this Dictionary<object, object> dic, string key)
    {
        var value = dic.GetString(key);
        return ulong.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0;
    }
}