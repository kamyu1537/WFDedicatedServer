using Steamworks;

namespace WFDS.Common.Types.Manager;

public interface IGameSessionManager
{
    string GetName();
    string GetCode();
    GameLobbyType GetLobbyType();
    bool IsPublic();
    bool IsAdult();
    int GetCapacity();

    bool IsLobbyValid();
    
    void CreateLobby(string serverName, string roomCode, GameLobbyType lobbyType, bool isPublic, bool isAdult, int maxPlayers);
    Task LeaveLobbyAsync();
    
    void UpdateBrowserValue();
    void KickNoHandshakePlayers();
    
    int GetSessionCount();
    IGameSession? GetSession(SteamId steamId);
    IEnumerable<IGameSession> GetSessions();
    bool IsSessionValid(SteamId steamId);
    void SelectSession(SteamId steamId, Action<IGameSession> action);
    void SelectSessions(Action<IGameSession> action);
    
    void ServerClose(SteamId steamId);
    void KickPlayer(SteamId steamId);
    void TempBanPlayer(SteamId steamId, bool update = true);
    void BanPlayers(string[] steamId);
    void RemoveBanPlayer(SteamId steamId);
    
    void SendP2PPacket(SteamId steamId, NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1);
    void BroadcastP2PPacket(NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1);
}