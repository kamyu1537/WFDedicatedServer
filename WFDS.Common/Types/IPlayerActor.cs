using Microsoft.Extensions.Logging;
using WFDS.Server.Network;

namespace WFDS.Common.Types;

public interface IPlayerActor : IActor
{
    GameItem HeldItem { get; set; }
    Cosmetics Cosmetics { get; set; }

    ISessionManager? SessionManager { get; set; }

    void OnCosmeticsUpdated(Cosmetics cosmetics);
    void OnHeldItemUpdated(GameItem item);
}