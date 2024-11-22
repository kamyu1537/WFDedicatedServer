using System.Numerics;

namespace WFDS.Common.Extensions;

public static class ObjectExtensions
{
    public static long GetInt(this object obj)
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

    public static Vector2 GetVector2(this object obj)
    {
        return obj switch
        {
            Vector2 vector2 => vector2,
            _ => Vector2.Zero
        };
    }
    
    public static Dictionary<object, object> GetObjectDictionary(this object obj)
    {
        return obj is Dictionary<object, object> dic ? dic : new Dictionary<object, object>();
    }
    
    public static List<object> GetObjectList(this object obj)
    {
        return obj as List<object> ?? [];
    }
}