namespace WFDS.Common.Types.Manager;

public interface IZoneManager
{
    void LoadZones();
    IZoneData GetZone();
    // IZoneData? GetZone(string zoneName);
}