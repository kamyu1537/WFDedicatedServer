namespace WFDS.Common.Types;

public interface IZoneData
{
    string FileName { get; }
    string FilePath { get; }
    List<PositionNode> FishSpawnPoints { get; }
    List<PositionNode> TrashPoints { get; }
    List<PositionNode> ShorelinePoints { get; }
    List<PositionNode> HiddenSpots { get; }
}