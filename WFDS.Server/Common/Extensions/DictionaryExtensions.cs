using WFDS.Godot.Types;

namespace WFDS.Server.Common.Extensions;

public static class DictionaryExtensions
{
    public static bool GetBool(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return false;

        return value is true;
    }
    
    public static long GetNumber(this Dictionary<object, object> dic, string key)
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
    
    public static int GetInt(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return 0;

        return value is int result ? result : 0;
    }
    
    public static long GetLong(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
        {
            return 0;   
        }
        
        return value is long result ? result : 0;
    }
    
    public static ulong GetULong(this Dictionary<object, object> dic, string key)
    {
        if (!dic.TryGetValue(key, out var value))
            return 0;

        return value is ulong result ? result : 0;
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

        return value as Vector3 ?? Vector3.Zero;
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
        return int.TryParse(value, out var result) ? result : 0;
    }
    
    public static long GetParseLong(this Dictionary<object, object> dic, string key)
    {
        var value = dic.GetString(key);
        return long.TryParse(value, out var result) ? result : 0;
    }
    
    public static ulong GetParseULong(this Dictionary<object, object> dic, string key)
    {
        var value = dic.GetString(key);
        return ulong.TryParse(value, out var result) ? result : 0;
    }
}