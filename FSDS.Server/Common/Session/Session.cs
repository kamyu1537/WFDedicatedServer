using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using System.Text.Json;
using FSDS.Godot.Binary;
using FSDS.Server.Common.Extensions;
using FSDS.Server.Common.Helpers;
using FSDS.Server.Managers;
using FSDS.Server.Packets;
using FSDS.Server.Services;
using Steamworks;

namespace FSDS.Server.Common;

public class Session
{
    public LobbyManager LobbyManager { get; init; } = null!;
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

    public void SendMessage(string message, Color color, bool local = false)
    {   
        Send(NetChannel.GameState, new MessagePacket
        {
            Message = message,
            Color = color.ToHex(),
            Local = local,
            Position = Godot.Types.Vector3.Zero,
            Zone = "main_zone",
            ZoneOwner = -1,
        });
    }

    public void SendLetter(SteamId target, string body)
    {
        Send(NetChannel.GameState, new LetterReceivedPacket()
        {
            LatterId = new Random().Next(),
            From = SteamClient.SteamId.ToString(),
            To = target.Value.ToString(),
            Closing = "From, ",
            User = "[SERVER]",
            Header = "Letter",
            Body = body,
            Items = [],
        });
    }

    public void ProcessPackets()
    {
        var count = 2;
        // Console.WriteLine($"Processing {Packets.Count} packets for {SteamId}");
        while (Packets.TryDequeue(out var packet))
        {
            if (count-- <= 0)
            {
                break;
            }

            // Console.WriteLine(JsonSerializer.Serialize(GodotBinaryConverter.Deserialize(GZipHelper.Decompress(packet.Item2))));
            SteamNetworking.SendP2PPacket(SteamId, packet.Item2, nChannel: packet.Item1.Value);
        }
    }
}