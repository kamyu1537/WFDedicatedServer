using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.Types;

namespace WFDS.Common.Helpers;

public class SteamNetworkHelper
{
    private const int k_nSteamNetworkingSend_Reliable = 8;
    private static readonly ILogger<SteamNetworkHelper> Logger = Log.Factory.CreateLogger<SteamNetworkHelper>();
    
    public static void SendMessageToUser(ref SteamNetworkingIdentity identity, NetChannel channel, byte[] data)
    {
        if (identity.IsInvalid())
        {
            return;
        }
        
        var ptr = Marshal.AllocHGlobal(data.Length);
        try
        {
            Marshal.Copy(data, 0, ptr, data.Length);
            
            var result = SteamNetworkingMessages.SendMessageToUser(ref identity, ptr, (uint)data.Length, k_nSteamNetworkingSend_Reliable, channel.Value);
            if (result != EResult.k_EResultOK)
            {
                Logger.LogError("failed to send message to user: {Result}", result);
            }
            
            Logger.LogDebug("Sent message to user: {Channel}", channel);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "failed to send message to user");
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
    
    public static void SendMessageToMultipleUsers(SteamNetworkingIdentity[] identities, NetChannel channel, byte[] data)
    {
        var ptr = Marshal.AllocHGlobal(data.Length);
        try
        {
            Marshal.Copy(data, 0, ptr, data.Length);
            
            for (var i = 0; i < identities.Length; i++)
            {
                var identity = identities[i];
                if (identity.IsInvalid())
                {
                    continue;
                }
                
                var result = SteamNetworkingMessages.SendMessageToUser(ref identity, ptr, (uint)data.Length, k_nSteamNetworkingSend_Reliable, channel.Value);
                if (result != EResult.k_EResultOK)
                {
                    Logger.LogError("Failed to send message to multiple users: {Result}", result);
                }
            
                Logger.LogDebug("Sent message to multiple users: {Channel}", channel);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "failed to send message to multiple users");
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    public static NetworkMessage[] ReadMessagesOnChannel(NetChannel channel, int maxMessages, out int recvMsgCount)
    {
        var msgPtrArr = new IntPtr[maxMessages];
        recvMsgCount = SteamNetworkingMessages.ReceiveMessagesOnChannel(channel.Value, msgPtrArr, maxMessages);
        if (recvMsgCount < 1)
        {
            return [];
        }
        
        var result = new NetworkMessage[recvMsgCount];
        for (var i = 0; i < recvMsgCount; i++)
        {
            var msgPtr = msgPtrArr[i];
            try
            {
                var netMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(msgPtr);
                var identity = netMessage.m_identityPeer;
                var recvData = new byte[netMessage.m_cbSize];
                Marshal.Copy(netMessage.m_pData, recvData, 0, netMessage.m_cbSize);
                Logger.LogDebug("received message on channel {Channel} from {Identity} : {Size}", channel, identity, netMessage.m_cbSize);
                result[i] = new NetworkMessage(identity, recvData);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "failed to read message on channel {Channel}", channel);
            }
            finally
            {
                SteamNetworkingMessage_t.Release(msgPtr);
            }
        }

        return result;
    }
}