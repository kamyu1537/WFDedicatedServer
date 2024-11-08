using System.Collections.Concurrent;
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
    
    public ConcurrentQueue<(NetChannel, byte[])> Packets { get; } = [];

    public void Send(NetChannel channel, IPacket packet)
    {
        LobbyManager.SendPacket(SteamId, channel, packet.ToDictionary());
    }

    public void ProcessPackets()
    {
        var count = 1;
        // Console.WriteLine($"Processing {Packets.Count} packets for {SteamId}");
        while (Packets.TryDequeue(out var packet))
        {
            if (count-- <= 0)
            {
                break;
            }

            // Console.WriteLine(JsonSerializer.Serialize(GodotBinaryConverter.Deserialize(GZipHelper.Decompress(packet.Item3))));
            SteamNetworking.SendP2PPacket(SteamId, packet.Item2, nChannel: packet.Item1.Value);
        }
    }
}