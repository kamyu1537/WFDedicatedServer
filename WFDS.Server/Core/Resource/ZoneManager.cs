using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

namespace WFDS.Server.Core.Resource;

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

    private readonly ZoneData[] _zones = [];

    public void LoadZones()
    {
        CheckAndCreateDirectory();
        var files = Directory.GetFiles(DirectoryPath, "*.tscn");

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);
            var zone = new ZoneData(fileName, filePath);
            zone.LoadZoneData(logger);
        }
    }

    public IZoneData GetZone()
    {
        return _zones.FirstOrDefault(x => x.FileName == ZoneFileName) ?? throw new InvalidOperationException("main_zone.tscn not found");
    }
}