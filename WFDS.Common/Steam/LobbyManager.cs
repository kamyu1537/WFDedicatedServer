using System.Globalization;
using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Helpers;
using WFDS.Common.Types;


namespace WFDS.Common.Steam;

public sealed class LobbyManager : Singleton<LobbyManager>, IDisposable
{
    private const int RoomCodeLength = 6;
    private const string LobbyMode = "GodotsteamLobby";
    private const string LobbyRef = "webfishing_gamelobby";
    private const string GameVersion = "1.1";

    private readonly ILogger _logger;
    private CSteamID _lobbyId = CSteamID.Nil;

    private bool _initialized;
    private string _name = string.Empty;
    private GameLobbyType _lobbyType = GameLobbyType.Public;
    private int _cap;
    private bool _adult;
    private string _code = string.Empty;

    private readonly Callback<LobbyEnter_t> _lobbyEnterCallback;
    private readonly Callback<LobbyCreated_t> _lobbyCreatedCallback;
    private readonly Callback<LobbyDataUpdate_t> _lobbyDataUpdateCallback;

    public LobbyManager()
    {
        _logger = Log.Factory.CreateLogger<LobbyManager>();

        _lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        _lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        _lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataChanged);
    }

    public void Dispose()
    {
        _lobbyEnterCallback.Dispose();
        _lobbyCreatedCallback.Dispose();
        _lobbyDataUpdateCallback.Dispose();
    }

    public void Initialize(string name, GameLobbyType lobbyType, int cap, bool adult, string code)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        _name = name;
        _lobbyType = lobbyType;
        _cap = cap;
        _adult = adult;

        _code = code;
        if (string.IsNullOrEmpty(_code) || RoomCodeLength < _code.Length)
        {
            _code = RandomHelper.RandomRoomCode(RoomCodeLength);
        }

        _logger.LogInformation("lobby initialized: {LobbyName}, {LobbyType}, {LobbyCap}, {LobbyAgeLimit}, {LobbyCode}", _name, _lobbyType, _cap, _adult, _code);
    }

    public bool LeaveLobby(out CSteamID lobbyId)
    {
        lobbyId = CSteamID.Nil;
        if (!SteamAPI.IsSteamRunning())
        {
            return false;
        }

        if (!IsInLobby())
        {
            return false;
        }

        SteamMatchmaking.SetLobbyJoinable(_lobbyId, false);
        SteamMatchmaking.DeleteLobbyData(_lobbyId, "mode");
        SteamMatchmaking.DeleteLobbyData(_lobbyId, "ref");
        SteamMatchmaking.DeleteLobbyData(_lobbyId, "version");
        SteamMatchmaking.DeleteLobbyData(_lobbyId, "server_browser_value");
        SteamMatchmaking.DeleteLobbyData(_lobbyId, "name");
        SteamMatchmaking.DeleteLobbyData(_lobbyId, "lobby_name");
        SteamMatchmaking.DeleteLobbyData(_lobbyId, "age_limit");
        SteamMatchmaking.DeleteLobbyData(_lobbyId, "code");
        SteamMatchmaking.DeleteLobbyData(_lobbyId, "cap");
        SteamMatchmaking.DeleteLobbyData(_lobbyId, "banned_players");
        SteamMatchmaking.SetLobbyMemberLimit(_lobbyId, 1);

        // close all p2p session
        var memberCount = SteamMatchmaking.GetNumLobbyMembers(_lobbyId);
        for (var i = 0; i < memberCount; ++i)
        {
            var member = SteamMatchmaking.GetLobbyMemberByIndex(_lobbyId, i);
            SteamNetworking.CloseP2PSessionWithUser(member);
        }

        lobbyId = _lobbyId;
        _lobbyId = CSteamID.Nil;
        return true;
    }

    public bool IsInLobby()
    {
        if (!_lobbyId.IsValid() || !_lobbyId.IsLobby())
        {
            return false;
        }

        if (!SteamManager.Inst.Initialized)
        {
            return false;
        }

        try
        {
            var lobbyOwner = SteamMatchmaking.GetLobbyOwner(_lobbyId);
            return lobbyOwner == SteamManager.Inst.SteamId;
        }
        catch
        {
            return false;
        }
    }

    public CSteamID GetLobbyId()
    {
        return _lobbyId;
    }

    public string GetName()
    {
        return _name;
    }

    public GameLobbyType GetLobbyType()
    {
        return _lobbyType;
    }

    public int GetCap()
    {
        return _cap;
    }

    public bool IsAdult()
    {
        return _adult;
    }

    public string GetCode()
    {
        return _code;
    }


    public void SetLobbyType(CSteamID lobbyId, GameLobbyType type)
    {
        var lobbyType = type switch
        {
            GameLobbyType.Public => "public",
            GameLobbyType.CodeOnly => "code_only",
            GameLobbyType.FriendsOnly => "friends_only",
            _ => "code_only"
        };

        SteamMatchmaking.SetLobbyData(lobbyId, "type", lobbyType);
        SteamMatchmaking.SetLobbyData(lobbyId, "public", type == GameLobbyType.Public ? "true" : "false");

        if (_lobbyType is GameLobbyType.Public or GameLobbyType.CodeOnly)
        {
            SteamMatchmaking.SetLobbyData(lobbyId, "code", _code);
        }
    }

    public void CreateLobby()
    {
        if (!_initialized) return;
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 1);
    }

    private void UpdateLobbyData(in CSteamID lobbyId)
    {
        SteamMatchmaking.SetLobbyJoinable(lobbyId, false);
        SteamMatchmaking.SetLobbyOwner(lobbyId, SteamManager.Inst.SteamId);

        SteamMatchmaking.SetLobbyData(lobbyId, "mode", LobbyMode);
        SteamMatchmaking.SetLobbyData(lobbyId, "ref", LobbyRef);
        SteamMatchmaking.SetLobbyData(lobbyId, "version", GameVersion);
        SteamMatchmaking.SetLobbyData(lobbyId, "server_browser_value", "0");
        SteamMatchmaking.SetLobbyData(lobbyId, "name", _name);
        SteamMatchmaking.SetLobbyData(lobbyId, "lobby_name", _name);
        SteamMatchmaking.SetLobbyData(lobbyId, "age_limit", _adult ? "true" : "false");
        SteamMatchmaking.SetLobbyData(lobbyId, "code", _code);
        SteamMatchmaking.SetLobbyData(lobbyId, "cap", _cap.ToString());
        SteamMatchmaking.SetLobbyData(lobbyId, "banned_players", "");

        SetLobbyType(lobbyId, _lobbyType);

        SteamMatchmaking.SetLobbyMemberLimit(lobbyId, _cap);
        SteamMatchmaking.SetLobbyJoinable(lobbyId, true);

        ConsoleHelper.UpdateConsoleTitle(_name, _code, 0, _cap);
    }

    public void UpdateBrowserValue()
    {
        if (!IsInLobby())
        {
            return;
        }

        var random = new Random();
        SteamMatchmaking.SetLobbyData(_lobbyId, "server_browser_value", (random.Next() % 20).ToString(CultureInfo.InvariantCulture));
    }

    public void UpdateBannedPlayers(IEnumerable<string> bannedPlayers)
    {
        if (!IsInLobby())
        {
            return;
        }

        SteamMatchmaking.SetLobbyData(_lobbyId, "banned_players", string.Join(",", bannedPlayers));
    }

    #region steam callbacks

    private void OnLobbyCreated(LobbyCreated_t param)
    {
        if (param.m_eResult != EResult.k_EResultOK)
        {
            _logger.LogError("failed to create lobby: {Result}", param.m_eResult);
            return;
        }

        var lobbyId = new CSteamID(param.m_ulSteamIDLobby);
        _logger.LogInformation("lobby created: {LobbyId}", param.m_ulSteamIDLobby);
        UpdateLobbyData(lobbyId);

        GameEventBus.Publish(new LobbyCreatedEvent(lobbyId));
    }

    private void OnLobbyEntered(LobbyEnter_t param)
    {
        if (param.m_EChatRoomEnterResponse != 1) // not success
        {
            _logger.LogError("failed to enter lobby: {EnterResponse}", param.m_EChatRoomEnterResponse);
            return;
        }

        _lobbyId = new CSteamID(param.m_ulSteamIDLobby);
        _logger.LogInformation("lobby entered: {LobbyId}", _lobbyId);
    }

    private void OnLobbyDataChanged(LobbyDataUpdate_t param)
    {
        var lobbyId = new CSteamID(param.m_ulSteamIDLobby);
        _logger.LogDebug("lobby data updated: {LobbyId}", lobbyId);
    }

    #endregion
}