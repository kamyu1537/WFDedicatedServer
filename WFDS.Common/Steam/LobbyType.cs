using Steamworks;

namespace WFDS.Common.Types;

public sealed class GameLobbyType
{
    public int Key { get; }
    public string Name { get; }
    public ELobbyType LobbyType { get; }
    public bool IsCodeButton { get; }
    public bool IsBrowserVisible { get; }
    public bool IsOffline { get; }

    private GameLobbyType(int key, string name, ELobbyType lobbyType, bool codeButton, bool browserVisible, bool offline)
    {
        Key = key;
        Name = name;
        LobbyType = lobbyType;
        IsCodeButton = codeButton;
        IsBrowserVisible = browserVisible;
        IsOffline = offline;
        
        Types.Add(name, this);
    }
    
    private static readonly Dictionary<string, GameLobbyType> Types = new();
    public static GameLobbyType GetByName(string name)
    {
        return Types.TryGetValue(name, out var type) ? type : throw new ArgumentException($"invalid lobby type: {name}");
    }
    
    public static readonly GameLobbyType Public = new(0, "Public", ELobbyType.k_ELobbyTypePublic, true, true, false);
    public static readonly GameLobbyType Unlisted = new(1, "Unlisted", ELobbyType.k_ELobbyTypePublic, true, false, false);
    public static readonly GameLobbyType Private = new(2, "Private", ELobbyType.k_ELobbyTypePrivate, false, false, false);
    public static readonly GameLobbyType Offline = new(3, "Offline", ELobbyType.k_ELobbyTypePrivate, true, false, true);
}