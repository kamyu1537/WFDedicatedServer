using Steamworks;

namespace WFDS.Common.Types;

public interface ILobbyManager
{
    void Initialize(string name, GameLobbyType lobbyType, int cap, bool adult, string code);
    void CreateLobby();
    bool LeaveLobby(out CSteamID lobbyId);
    
    bool IsInLobby();
    CSteamID GetLobbyId();

    string GetName();    
    GameLobbyType GetLobbyType();
    int GetCap();
    bool IsAdult();    
    string GetCode();


    
    void SetLobbyType(CSteamID lobbyId, GameLobbyType type);
    void UpdateBrowserValue();
}