using Steamworks;
using WFDS.Server.Common;
using WFDS.Server.Network;

namespace WFDS.Common.Types;

public interface ISessionManager
{
    int GetSessionCount();
    bool IsSessionValid(SteamId steamId);
    void SelectSession(SteamId steamId, Action<ISession> action);
    void SelectSessions(Action<ISession> action);
    
    void ServerClose(SteamId steamId);
    void KickPlayer(SteamId steamId);
    void TempBanPlayer(SteamId steamId, bool update = true);
    void BanPlayers(string[] steamId);
    void RemoveBanPlayer(SteamId steamId);
    
    
    void SendP2PPacket(SteamId steamId, NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1);
    void SendP2PPacket(SteamId steamId, NetChannel channel, object data, string zone = "", long zoneOwner = -1);
    
    void BroadcastP2PPacket(NetChannel channel, IPacket packet, string zone = "", long zoneOwner = -1);
    void BroadcastP2PPacket(NetChannel channel, object data, string zone = "", long zoneOwner = -1);
}