using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Godot.Binary;
using ZLogger;

namespace WFDS.Common.Helpers;

public static class SteamNetworkingHelper
{
    private static readonly ILogger Logger = Log.Factory.CreateLogger(typeof(SteamNetworkingHelper).FullName ?? nameof(SteamNetworkingHelper));
    
    public static bool SendP2PPacket(CSteamID steamId, NetChannel channel, byte[] bytes)
    {
        if (steamId == SteamManager.Inst.SteamId)
        {
            return false;
        }

        try
        {
            var length = (uint)bytes.Length;
            var sendType = channel.SendType;
            var channelValue = channel.Value;

            if (length == 0) return false;
            
            return SteamNetworking.SendP2PPacket(steamId, bytes, length, sendType, channelValue);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"failed to rent bytes for P2P packet");
            return false;
        }
    }

    public static void BroadcastP2PPacket(CSteamID lobbyId, NetChannel channel, object data)
    {
        var memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
        if (memberCount <= 1) return;

        var serialized = GodotBinaryConverter.Serialize(data);
        var bytes = GZipHelper.Compress(serialized);

        try
        {
            var length = (uint)bytes.Length;
            var sendType = channel.SendType;
            var channelValue = channel.Value;

            if (length == 0) return;
            for (var i = 0; i < memberCount; ++i)
            {
                var member = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
                if (member == SteamManager.Inst.SteamId) // 자기 자신에게는 보내지 않는다.
                {
                    continue;
                }

                SteamNetworking.SendP2PPacket(member, bytes, length, sendType, channelValue);
            }
        }
        catch (Exception ex)
        {
            Logger.ZLogError($"failed to rent bytes for broadcast P2P packet : \n{ex}");
        }
    }
}