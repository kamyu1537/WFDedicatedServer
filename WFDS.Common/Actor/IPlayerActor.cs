using WFDS.Common.Types;

namespace WFDS.Common.Actor;

public interface IPlayerActor : IActor
{
    GameItem HeldItem { get; set; }
    Cosmetics Cosmetics { get; set; }
}