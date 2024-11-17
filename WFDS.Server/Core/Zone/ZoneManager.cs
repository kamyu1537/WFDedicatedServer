using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Core.Zone;

internal class ZoneManager(ILogger<ZoneManager> logger) : IZoneManager
{
    private const string DirectoryName = "Zones";
    private const string ZoneFileName = "main_zone.tscn";
    private static string DirectoryPath => Path.Combine(Directory.GetCurrentDirectory(), DirectoryName);
    
    private static void CheckAndCreateDirectory()
    {
        var zonePath = DirectoryPath;
        if (!Directory.Exists(zonePath))
        {
            Directory.CreateDirectory(zonePath);
        }
    }

    private readonly Dictionary<string, ZoneData> _zones = [];

    public void LoadZones()
    {
        CheckAndCreateDirectory();
        var files = Directory.GetFiles(DirectoryPath, "*.tscn");

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);
            var zone = new ZoneData(fileName, filePath);
            zone.LoadZoneData(logger);

            _zones.Add(zone.FileName, zone);
        }
    }

    public IZoneData GetZone()
    {
        return _zones.TryGetValue(ZoneFileName, out var zone) ? zone : throw new InvalidOperationException("main_zone.tscn not found");
    }
}