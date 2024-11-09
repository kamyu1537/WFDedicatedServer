using WFDS.Server.Common;

namespace WFDS.Server;

public interface IServerSetting
{
    string ServerName { get; set; }
    string RoomCode { get; set; }
    int MaxPlayers { get; set; }
    GameLobbyType LobbyType { get; set; }
    bool Public { get; set; }
    bool Adult { get; set; }
}

public class ServerSetting : IServerSetting
{
    public string ServerName { get; set; } = "WFDS Server";
    public string RoomCode { get; set; } = string.Empty; // 비어있으면 랜덤 생성, 최대 5자
    public int MaxPlayers { get; set; } = 12;
    public GameLobbyType LobbyType { get; set; } = GameLobbyType.Public;
    public bool Public { get; set; } = true;
    public bool Adult { get; set; }
}