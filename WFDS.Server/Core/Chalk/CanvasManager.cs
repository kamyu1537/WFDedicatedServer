using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Serilog;
using WFDS.Common.Extensions;
using WFDS.Common.Network.Packets;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Godot.Binary;
using WFDS.Server.Core.Configuration;
using WFDS.Server.Core.Network;
using ILogger = Serilog.ILogger;

namespace WFDS.Server.Core.Chalk;

internal class CanvasManager(IOptions<ServerSetting> setting, SessionManager session, LobbyManager lobby) : ICanvasManager
{
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan ForceUpdateInterval = TimeSpan.FromSeconds(30);
    
    private static readonly ILogger Logger = Log.ForContext<CanvasManager>();
    private ConcurrentDictionary<long, Canvas> Canvases { get; set; } = [];
    private bool Updated { get; set; } = true;
    private DateTimeOffset UpdateTime { get; set; } = DateTimeOffset.UtcNow;
    private DateTimeOffset SaveTime { get; set; } = DateTimeOffset.UtcNow;

    public void Draw(ChalkPacket packet)
    {
        var canvas = GetCanvas(packet.CanvasId);
        canvas.Draw(packet.GetData());
        
        UpdateTime = DateTimeOffset.UtcNow;
        Updated = false;
    }

    private Canvas GetCanvas(long canvasId) => Canvases.GetOrAdd(canvasId, new Canvas
    {
        CanvasId = canvasId
    });

    public ChalkPacket[] GetCanvasPackets()
    {
        return Canvases.Select(x => x.Value.ToPacket()).ToArray();
    }
    
    private static readonly string CanvasPath = Path.Join(Environment.CurrentDirectory, "Canvas");

    private static void CreateDirectory()
    {
        if (Path.Exists(CanvasPath)) return;
        Directory.CreateDirectory(CanvasPath);
    }

    public void SaveChanges()
    {
        if (!setting.Value.SaveChalkData) return;
        
        if (Updated) return;
        if (DateTimeOffset.UtcNow - UpdateTime < UpdateInterval)
        {
            if (DateTimeOffset.UtcNow - SaveTime < ForceUpdateInterval)
            {
                return;
            }
        }
        
        Updated = true;
        SaveTime = DateTimeOffset.UtcNow;
        
        CreateDirectory();
        
        foreach (var item in Canvases)
        {
            var fileName = Path.Join(CanvasPath, $"canvas_{item.Key}.bin");
            File.WriteAllBytes(fileName, GodotBinaryConverter.Serialize(item.Value.ToPacket().ToDictionary()));
            Logger.Information($"saved canvas {item.Key}");
        }
    }

    public void LoadAll()
    {
        if (!setting.Value.SaveChalkData) return;
        CreateDirectory();
        
        var files = Directory.GetFiles(CanvasPath, "canvas_*.bin");
        foreach (var file in files)
        {
            var bytes = File.ReadAllBytes(file);
            var data = GodotBinaryConverter.Deserialize(bytes);
            if (data is not Dictionary<object, object> dict) continue;
            var packet = new ChalkPacket();
            packet.Deserialize(dict);
            
            var canvasId = packet.CanvasId;
            var canvas = new Canvas
            {
                CanvasId = canvasId
            };
            
            canvas.Draw(packet.GetData());
            Canvases[canvasId] = canvas;
        }
        
        Logger.Information($"loaded {files.Length} canvas");
    }

    public void ClearAll()
    {
        foreach (var canvas in Canvases.Values)
        {
            var packet = canvas.ToClearPacket();
            session.BroadcastP2PPacket(lobby.GetLobbyId(), NetChannel.Chalk, packet);
            canvas.Clear();
        }
        
        UpdateTime = DateTimeOffset.UtcNow.Subtract(UpdateInterval);
        Updated = false; // 바로 업데이트 하도록
        SaveChanges();
    }
}