using FSDS.Server.Managers;
using FSDS.Server.Services;
using Steamworks;

namespace FSDS.Server.Common;

public class Session
{
    public LobbyManager LobbyManager { get; init; }
    public Friend Friend { get; init; }
    public SteamId SteamId { get; init; }
    
    public bool HandshakeReceived { get; set; }
    
    public DateTimeOffset ConnectTime { get; set; }
    public DateTimeOffset HandshakeReceiveTime { get; set; }
    public DateTimeOffset PingReceiveTime { get; set; }
    public DateTimeOffset PacketReceiveTime { get; set; }

    public void Send(NetChannel channel, IPacket packet)
    {
        LobbyManager.SendPacket(SteamId, channel, packet.ToDictionary());
    }
}