namespace WFDS.Common.Types.Manager;

public interface IMapManager
{
    public List<PositionNode> FishSpawnPoints { get; }   
    public List<PositionNode> TrashPoints { get; }
    public List<PositionNode> ShorelinePoints { get; }
    public List<PositionNode> HiddenSpots { get; }

    void LoadSpawnPoints();
}