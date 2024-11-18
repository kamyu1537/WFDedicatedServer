using Steamworks;
using WFDS.Common.Network;

namespace WFDS.Common.Types.Manager;

public interface ISessionManager
{
    bool IsServerClosed();
    void ServerClose();
    void ServerClose(CSteamID steamId);
    
    int GetSessionCount();
    Session? GetSession(CSteamID steamId);
    IEnumerable<Session> GetSessions();
    bool IsSessionValid(CSteamID steamId);

    void KickPlayer(CSteamID steamId);
    
    bool IsBannedPlayer(CSteamID steamId);
    void TempBanPlayer(CSteamID lobbyId, CSteamID steamId);
    void BanPlayers(CSteamID lobbyId, string[] steamId);
    void RemoveBanPlayer(CSteamID steamId);
    IEnumerable<string> GetBannedPlayers();
    
    void SendP2PPacket(CSteamID steamId, NetChannel channel, Packet packet, bool useSession = true);
    void BroadcastP2PPacket(CSteamID lobbyId, NetChannel channel, Packet packet, bool useSession = true);
}