using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json;
using WFDS.Common.Extensions;
using WFDS.Common.Types;
using WFDS.Godot.Types;


namespace WFDS.Server.Core.Zone;

internal sealed class ZoneData(string fileName, string filePath) : IZoneData
{
    public string FileName { get; } = fileName;
    public string FilePath { get; } = filePath;

    public List<PositionNode> FishSpawnPoints { get; } = [];
    public List<PositionNode> TrashPoints { get; } = [];
    public List<PositionNode> ShorelinePoints { get; } = [];
    public List<PositionNode> HiddenSpots { get; } = [];

    public void LoadZoneData(ILogger logger)
    {
        using var file = new FileStream(FilePath, FileMode.Open);
        using var reader = new StreamReader(file, Encoding.UTF8);

        logger.LogInformation("loading zone from {FileName}", FileName);
        var nodes = LoadPositionNodeList(reader)
            .Select(ParsePositionNode)
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray();

        FishSpawnPoints.Clear();
        TrashPoints.Clear();
        ShorelinePoints.Clear();
        HiddenSpots.Clear();
        
        FishSpawnPoints.AddRange(nodes.Where(x => x.Groups.Contains("fish_spawn")));
        TrashPoints.AddRange(nodes.Where(x => x.Groups.Contains("trash_point")));
        ShorelinePoints.AddRange(nodes.Where(x => x.Groups.Contains("shoreline_point")));
        HiddenSpots.AddRange(nodes.Where(x => x.Groups.Contains("hidden_spot")));
    }

    private static List<string> LoadPositionNodeList(StreamReader reader)
    {
        var nodes = new List<string>();

        var start = false;
        var builder = new StringBuilder();
        while (reader.ReadLine() is { } line)
        {
            if (line.StartsWith("[node") && line.Contains("type=\"Position3D\"", StringComparison.CurrentCultureIgnoreCase))
            {
                if (start)
                {
                    nodes.Add(builder.ToString());
                    builder.Clear();
                }

                start = true;
            }

            if (!start || string.IsNullOrWhiteSpace(line)) continue;
            builder.AppendLine(line);
        }

        return nodes;
    }

    private PositionNode? ParsePositionNode(string value)
    {
        var split = value.Split('\n');
        if (split.Length < 2)
        {
            return null;
        }

        var infoLine = split[0].Trim();
        var transformLine = split[1].Trim();
        var properties = GetProperties(infoLine);
        var transform = GetTransform3D(transformLine);

        if (transform == null)
        {
            return null;
        }

        return new PositionNode
        {
            Name = properties.GetString("name"),
            Type = properties.GetString("type"),
            Parent = properties.GetString("parent"),
            Groups = properties.GetStringArray("groups"),
            Transform = transform
        };
    }

    private static Dictionary<object, object> GetProperties(string info)
    {
        var substring = info.Substring(6, info.Length - 7);
        var properties = new Dictionary<object, object>();
        var split = substring.Split(' ');
        foreach (var property in split)
        {
            var propertySplit = property.Split('=');
            if (propertySplit.Length != 2)
            {
                continue;
            }

            var key = propertySplit[0];
            object? value;
            if (key == "groups")
            {
                value = JsonSerializer.Deserialize<string[]>(string.Join('=', propertySplit.Skip(1)));
            }
            else
            {
                value = JsonSerializer.Deserialize<string>(string.Join('=', propertySplit.Skip(1)));
            }


            properties.Add(key, value!);
        }

        return properties;
    }

    private static Transform3D? GetTransform3D(string line)
    {
        var substring = line.Substring(23, line.Length - 24).Trim();
        var split = substring.Split(',').Select(x => x.Trim()).Select(x => float.Parse(x, NumberStyles.Any, CultureInfo.InvariantCulture)).ToArray();
        if (split.Length < 12)
        {
            return null;
        }

        var x = new Vector3(split[0], split[1], split[2]);
        var y = new Vector3(split[3], split[4], split[5]);
        var z = new Vector3(split[6], split[7], split[8]);
        var origin = new Vector3(split[9], split[10], split[11]);
        return new Transform3D(x, y, z, origin);
    }
}