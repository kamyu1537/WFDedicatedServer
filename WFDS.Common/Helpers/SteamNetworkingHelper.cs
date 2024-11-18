using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.Helpers;

public static class SteamNetworkingHelper
{
    public static bool SendP2PPacket(CSteamID steamId, NetChannel channel, byte[] data)
    {
        if (steamId == SteamUser.GetSteamID())
        {
            return false;
        }
        
        return SteamNetworking.SendP2PPacket(steamId, data, (uint)data.Length, channel.SendType, channel.Value);
    }
    
    public static void BroadcastP2PPacket(CSteamID lobbyId, NetChannel channel, byte[] data)
    {
        var memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
        for (var i = 0; i < memberCount; ++i)
        {
            var member = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
            if (member == SteamUser.GetSteamID()) // 자기 자신에게는 보내지 않는다.
            {
                return;
            }
            
            SendP2PPacket(member, channel, data);
        }
    }
}