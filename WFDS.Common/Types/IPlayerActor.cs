using WFDS.Common.Types.Manager;
using WFDS.Godot.Types;

namespace WFDS.Common.Types;

public interface IPlayerActor : IActor
{
    GameItem HeldItem { get; set; }
    Cosmetics Cosmetics { get; set; }

    ISessionManager? SessionManager { get; set; }

    void OnCosmeticsUpdated(Cosmetics cosmetics);
    void OnHeldItemUpdated(GameItem item);
    void OnLevelUp();
    void OnMessage(string message, string color, bool local, Vector3 position, string zone, long zoneOwner);
    void OnChatMessage(string message);
}