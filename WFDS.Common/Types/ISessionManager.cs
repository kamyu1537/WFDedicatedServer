using Steamworks;
using WFDS.Common.Network;

namespace WFDS.Common.Types.Manager;

public interface ISessionManager
{
    string GetName();
    string GetCode();
    GameLobbyType GetLobbyType();
    bool IsPublic();
    bool IsAdult();
    int GetCapacity();
    
    void SetPublic(bool isPublic);
    void SetLobbyType(GameLobbyType lobbyType);

    bool IsLobbyValid();
    
    void CreateLobby(string serverName, string roomCode, GameLobbyType lobbyType, bool isPublic, bool isAdult, int maxPlayers);
    Task LeaveLobbyAsync();
    
    void UpdateBrowserValue();
    void KickNoHandshakePlayers();
    
    int GetSessionCount();
    Session? GetSession(SteamId steamId);
    IEnumerable<Session> GetSessions();
    bool IsSessionValid(SteamId steamId);
    
    void ServerClose(SteamId steamId);
    void KickPlayer(SteamId steamId);
    void TempBanPlayer(SteamId steamId, bool update = true);
    void BanPlayers(string[] steamId);
    void RemoveBanPlayer(SteamId steamId);
    
    void SendP2PPacket(SteamId steamId, NetChannel channel, Packet packet, bool useSession = true);
    void BroadcastP2PPacket(NetChannel channel, Packet packet, bool useSession = true);
}