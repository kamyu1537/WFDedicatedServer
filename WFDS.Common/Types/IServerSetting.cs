namespace WFDS.Common.Types;

public interface IServerSetting
{
    ushort AdminPort { get; set; }
    string ServerName { get; set; }
    string RoomCode { get; set; }
    int MaxPlayers { get; set; }
    GameLobbyType LobbyType { get; set; }
    bool Adult { get; set; }
    string[] BannedPlayers { get; set; }
    bool SaveChalkData { get; set; }
}