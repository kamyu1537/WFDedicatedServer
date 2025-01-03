using Steamworks;

namespace WFDS.Common.Types;

public record NetworkMessage(SteamNetworkingIdentity Identity, byte[] Data);