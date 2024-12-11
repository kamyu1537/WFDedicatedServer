using System.Numerics;
using Microsoft.Extensions.Logging;
using WFDS.Common.Extensions;


namespace WFDS.Common.Network.Packets;

public class ChalkPacket : Packet
{
    private static readonly ILogger Logger = Log.Factory.CreateLogger<ChalkPacket>();
    
    public long CanvasId { get; set; }
    public List<object> Data { get; set; } = [];


    public override void Deserialize(Dictionary<object, object> data)
    {
        CanvasId = data.GetInt("canvas_id");
        Data = data.GetObjectList("data");
    }

    public override void Serialize(Dictionary<object, object> data)
    {
        data.TryAdd("type", "chalk_packet");
        data.TryAdd("canvas_id", CanvasId);
        data.TryAdd("data", Data);
    }

    public IEnumerable<(Vector2 pos, long color)> GetData()
    {
        foreach (var item in Data.Select(x => x.GetObjectList()))
        {
            if (item.Count != 2)
            {
                Logger.LogError("Invalid chalk data item");
                continue;
            }

            var pos = item[0].GetVector2();
            var color = item[1].GetInt();
            
            yield return (pos, color);
        }
    }
}