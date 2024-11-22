using WFDS.Common.Network.Packets;

namespace WFDS.Common.Types;

public interface ICanvasManager
{
    ChalkPacket[] GetCanvasPackets();
    void Draw(ChalkPacket packet);
    void LoadAll();
    void SaveChanges();
}