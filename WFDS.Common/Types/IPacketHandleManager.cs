using Steamworks;

namespace WFDS.Common.Types.Manager;

public interface IPacketHandleManager
{
    void OnPacketReceived(SteamId sender, NetChannel channel, object? data);
}