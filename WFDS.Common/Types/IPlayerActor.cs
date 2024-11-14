using System.Numerics;

namespace WFDS.Common.Types;

public interface IPlayerActor : IActor
{
    GameItem HeldItem { get; set; }
    Cosmetics Cosmetics { get; set; }
}