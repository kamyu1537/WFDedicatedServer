using System.Buffers;
using Serilog;
using Steamworks;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Godot.Binary;

namespace WFDS.Common.Helpers;

public static class SteamNetworkingHelper
{
    public static bool SendP2PPacket(CSteamID steamId, NetChannel channel, Memory<byte> data)
    {
        if (steamId == SteamManager.Inst.SteamId)
        {
            return false;
        }

        byte[] bytes = null!;
        try
        {
            bytes = ArrayPool<byte>.Shared.Rent(data.Length);
            data.CopyTo(bytes);
            var length = (uint)data.Length;
            var sendType = channel.SendType;
            var channelValue = channel.Value;
            
            return SteamNetworking.SendP2PPacket(steamId, bytes, length, sendType, channelValue);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Failed to send P2P packet");
            return false;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }

    public static void BroadcastP2PPacket(CSteamID lobbyId, NetChannel channel, object data)
    {
        var memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
        if (memberCount <= 1) return;
        
        var serialized = GodotBinaryConverter.Serialize(data);
        var compressed = GZipHelper.Compress(serialized);
        
        byte[] bytes = null!;
        try
        {
            bytes = ArrayPool<byte>.Shared.Rent(compressed.Length);
            compressed.CopyTo(bytes);
            var length = (uint)compressed.Length;
            var sendType = channel.SendType;
            var channelValue = channel.Value;

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
            Log.Logger.Error(ex, "Failed to rent bytes for broadcast P2P packet");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }
}