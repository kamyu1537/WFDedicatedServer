using WFDS.Common.Types;

namespace WFDS.Server.Core.Configuration;

internal class ServerSetting : IServerSetting
{
    public ushort AdminPort { get; set; } = 18300;
    public string ServerName { get; set; } = "WFDS Server";
    public string RoomCode { get; set; } = string.Empty; // 비어있으면 랜덤 생성, 최대 5자
    public int MaxPlayers { get; set; } = 12;
    public string LobbyType { get; set; } = GameLobbyType.Public.Name;
    public bool Adult { get; set; }
    public bool SaveChalkData { get; set; }
    public string[] LobbyTags { get; set; } = [];
}