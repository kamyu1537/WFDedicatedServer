namespace WFDS.Common.Steam;

public sealed class LobbyTag
{
    private static readonly Dictionary<string, LobbyTag> Tags = new();
    public string Value { get; }
    
    private LobbyTag(string value)
    {
        Value = value;
        Tags.Add(value, this);
    }
    
    public static LobbyTag Get(string value)
    {
        return Tags.TryGetValue(value.ToLowerInvariant(), out var tag) ? tag : throw new ArgumentException($"invalid lobby tag: {value}");
    }
    
    public static LobbyTag[] GetAll()
    {
        return Tags.Values.ToArray();
    }
    
    public static readonly LobbyTag Talkative = new("talkative");
    public static readonly LobbyTag Quiet = new("quiet");
    public static readonly LobbyTag Grinding = new("grinding");
    public static readonly LobbyTag Chill = new("chill");
    public static readonly LobbyTag Silly = new("silly");
    public static readonly LobbyTag Hardcore = new("hardcore");
    public static readonly LobbyTag Mature = new("mature");
    public static readonly LobbyTag Modded = new("modded");
}