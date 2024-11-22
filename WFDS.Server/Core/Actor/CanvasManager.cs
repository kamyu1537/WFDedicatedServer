using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using WFDS.Common.Network.Packets;

namespace WFDS.Server.Core.Actor;

public class Canvas
{
    public long CanvasId { get; set; }
    private Dictionary<(int, int), long> Data { get; set; } = [];

    private static (int, int) GetKey(Vector2 pos) => ((int)pos.X, (int)pos.Y);
    private static Vector2 GetVector2((int, int) key) => new(key.Item1, key.Item2);

    public void Draw(IEnumerable<(Vector2 pos, long color)> data)
    {
        foreach (var (pos, color) in data)
        {
            var key = GetKey(pos);
            if (color < 0)
            {
                Data.Remove(key, out _);
            }
            else
            {
                Data[key] = color;
            }
        }
    }

    public ChalkPacket ToPacket()
    {
        return new ChalkPacket
        {
            CanvasId = CanvasId,
            Data = Data
                .ToImmutableArray()
                .Select(object (x) => new List<object>
                {
                    GetVector2(x.Key),
                    x.Value
                }).ToList()
        };
    }
}

public class CanvasManager
{
    private ConcurrentDictionary<long, Canvas> Canvases { get; set; } = [];

    public void Draw(ChalkPacket packet)
    {
        var canvas = GetCanvas(packet.CanvasId);
        canvas.Draw(packet.GetData());
    }

    private Canvas GetCanvas(long canvasId) => Canvases.GetOrAdd(canvasId, new Canvas
    {
        CanvasId = canvasId
    });

    public ChalkPacket[] GetCanvasPackets()
    {
        return Canvases.Select(x => x.Value.ToPacket()).ToArray();
    }
}