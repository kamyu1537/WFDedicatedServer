using WFDS.Common.Types.Manager;

namespace WFDS.Server.Core.Zone;

internal static class ZoneManagerHostExtensions
{
    public static void LoadZones(this IHost host)
    {
        var zoneManager = host.Services.GetService<IZoneManager>();
        if (zoneManager == null)
        {
            throw new InvalidOperationException("IZoneManager service not found");
        }
        
        zoneManager.LoadZones();
    }
}