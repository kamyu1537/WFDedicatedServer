namespace WFDS.Common.Types;

public interface IServerSetting
{
    ushort AdminPort { get; set; }
    string ServerName { get; set; }
    string RoomCode { get; set; }
    int MaxPlayers { get; set; }
    string LobbyType { get; set; }
    bool SaveChalkData { get; set; }
    string[] LobbyTags { get; set; }
}