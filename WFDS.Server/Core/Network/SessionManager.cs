using System.Collections.Concurrent;
using Steamworks;
using Steamworks.Data;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Helpers;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Godot.Binary;
using WFDS.Server.Core.GameEvent;

namespace WFDS.Server.Core.Network;

internal sealed class SessionManager : ISessionManager
{
    private const int MaxPlayers = 16;
    private const int RoomCodeLength = 6;
    private const string LobbyMode = "GodotsteamLobby";
    private const string LobbyRef = "webfishing_gamelobby";
    private const string GameVersion = "1.1";

    private readonly ILogger<SessionManager> _logger;

    private readonly HashSet<ulong> _banned = [];
    private readonly ConcurrentDictionary<ulong, Session> _sessions = [];

    private bool _created;
    private string _name = string.Empty;
    private GameLobbyType _lobbyType = GameLobbyType.Public;
    private int _cap;
    private bool _public;
    private bool _adult;
    private string _code = string.Empty;
    private bool _closed;

    private Lobby? _lobby;

    public SessionManager(ILogger<SessionManager> logger)
    {
        _logger = logger;

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberDataChanged += OnLobbyMemberDataChanged;

        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;
        SteamMatchmaking.OnLobbyMemberKicked += OnLobbyMemberKicked;
        SteamMatchmaking.OnLobbyMemberBanned += OnLobbyMemberBanned;
        SteamMatchmaking.OnChatMessage += OnChatMessage;

        SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
    }

    public string GetName()
    {
        return _name;
    }

    public string GetCode()
    {
        return _code;
    }

    public GameLobbyType GetLobbyType()
    {
        return _lobbyType;
    }

    public bool IsPublic()
    {
        return _public;
    }

    public bool IsAdult()
    {
        return _adult;
    }

    public int GetCapacity()
    {
        return _cap;
    }

    public bool IsLobbyValid()
    {
        if (!_lobby.HasValue)
        {
            return true;
        }

        var lobby = _lobby.Value;
        if (!lobby.IsOwnedBy(SteamClient.SteamId))
        {
            return false;
        }

        uint ip = 0;
        ushort port = 0;
        SteamId serverId = 0;
        if (!lobby.GetGameServer(ref ip, ref port, ref serverId))
        {
            _lobby = null;
            _created = false;
            return false;
        }

        // _logger.LogInformation("lobby game server: {Ip}:{Port} {SteamId}", new IPAddress(ip), port, serverId);
        return true;
    }

    public void CreateLobby(string serverName, string code, GameLobbyType lobbyType, bool @public, bool adult, int cap = MaxPlayers)
    {
        if (_created)
        {
            return;
        }

        _code = code;
        if (_code.Length != RoomCodeLength || string.IsNullOrWhiteSpace(_code))
        {
            _code = GenerationRoomCode();
        }

        _name = serverName;
        _public = @public;
        _lobbyType = lobbyType;
        _adult = adult;
        _cap = cap;
        _created = true;

        SteamMatchmaking.CreateLobbyAsync(_cap + 1);
    }

    private void SetupLobby(Lobby lobby)
    {
        lobby.SetGameServer(SteamClient.SteamId);
        lobby.SetData("mode", LobbyMode);
        lobby.SetData("ref", LobbyRef);
        lobby.SetData("version", GameVersion);
        lobby.SetData("server_browser_value", "0");


        lobby.SetData("name", _name);
        lobby.SetData("lobby_name", _name);
        lobby.SetData("cap", (_cap + 1).ToString());
        lobby.SetData("age_limit", _adult ? "true" : "");

        lobby.SetData("banned_players", "");

        SetLobbyType(_lobbyType);
        SetPublic(_public);

        UpdateBannedPlayers();
        UpdateConsoleTitle();
    }

    public void SetPublic(bool @public)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        _lobby.Value.SetJoinable(@public);
    }

    public void SetLobbyType(GameLobbyType type)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        var lobbyType = type switch
        {
            GameLobbyType.Public => "public",
            GameLobbyType.CodeOnly => "code_only",
            GameLobbyType.FriendsOnly => "friends_only",
            _ => "code_only"
        };

        _lobby.Value.SetData("type", lobbyType);
        _lobby.Value.SetData("public", _lobbyType == GameLobbyType.Public ? "true" : "false");

        if (_lobbyType is GameLobbyType.Public or GameLobbyType.CodeOnly)
        {
            _lobby.Value.SetData("code", _code);
        }
    }

    public async Task LeaveLobbyAsync()
    {
        if (_lobby.HasValue)
        {
            while (_sessions.Count > 0)
            {
                await Task.Delay(500);
            }
        }

        _lobby?.Leave();
    }

    private void UpdateBannedPlayers()
    {
        _lobby?.SetData("banned_players", string.Join(",", _banned));
    }

    private void RemoveSession(Friend member)
    {
        _logger.LogWarning("try remove session: {Member}", member);
        SteamNetworking.CloseP2PSessionWithUser(member.Id);

        if (!_sessions.TryRemove(member.Id.Value, out _))
        {
            _logger.LogWarning("failed to remove session: {Member}", member);
            return;
        }

        GameEventBus.Publish(new PlayerLeaveEvent(member.Id));
        UpdateConsoleTitle();
    }

    private static string GenerationRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var code = new char[RoomCodeLength];
        var random = new Random();
        for (var i = 0; i < code.Length; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }

        return new string(code);
    }

    public void UpdateBrowserValue()
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        var random = new Random();

        _lobby.Value.SetData("server_browser_value", (random.Next() % 20).ToString());
    }

    public void KickNoHandshakePlayers()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var session in _sessions.Values)
        {
            if (!session.HandshakeReceived && now - session.ConnectTime > TimeSpan.FromMinutes(5))
            {
                _logger.LogWarning("no handshake player: {Member}", session.Friend);
                // KickPlayer(session.SteamId);
            }
        }
    }

    private void UpdateConsoleTitle()
    {
        var newTitle = $"[{_sessions.Count}/{_cap}] {_name} [{_code}]";
        _logger.LogInformation("update console title: {Title}", newTitle);
        Console.Title = newTitle;
    }

    public int GetSessionCount()
    {
        return _sessions.Count;
    }

    public Session? GetSession(SteamId steamId)
    {
        return _sessions.TryGetValue(steamId.Value, out var session) ? session : null;
    }

    public IEnumerable<Session> GetSessions()
    {
        return _sessions.Values;
    }

    public bool IsSessionValid(SteamId steamId)
    {
        return _sessions.ContainsKey(steamId.Value);
    }

    public void SelectSessions(Action<Session> action)
    {
        foreach (var session in _sessions.Values)
        {
            action(session);
        }
    }

    public void SelectSession(SteamId target, Action<Session> action)
    {
        if (!_sessions.TryGetValue(target.Value, out var session))
        {
            return;
        }

        action(session);
    }

    public void SendP2PPacket(SteamId steamId, NetChannel channel, Packet packet, bool useSession = true)
    {
        var data = PacketHelper.ToDictionary(packet);
        SendP2PPacket(steamId, channel, data, useSession);
    }

    private void SendP2PPacket(SteamId steamId, NetChannel channel, object data, bool useSession = true)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        if (useSession)
        {
            if (!_sessions.TryGetValue(steamId.Value, out var session))
            {
                return;
            }

            var bytes = GodotBinaryConverter.Serialize(data);
            var compressed = GZipHelper.Compress(bytes);
            session.Packets.Enqueue((channel, compressed));
        }
        else
        {
            var bytes = GodotBinaryConverter.Serialize(data);
            var compressed = GZipHelper.Compress(bytes);
            SteamNetworking.SendP2PPacket(steamId, compressed, nChannel: channel.Value);
        }
    }

    public void BroadcastP2PPacket(NetChannel channel, Packet packet, bool useSession = true)
    {
        var data = PacketHelper.ToDictionary(packet);
        BroadcastP2PPacket(channel, data, useSession);
    }

    private void BroadcastP2PPacket(NetChannel channel, object data, bool useSession = true)
    {
        var bytes = GodotBinaryConverter.Serialize(data);
        var compressed = GZipHelper.Compress(bytes);

        if (useSession)
        {
            foreach (var session in _sessions.Values)
            {
                session.Packets.Enqueue((channel, compressed));
            }
        }
        else if (_lobby.HasValue)
        {
            foreach (var member in _lobby.Value.Members)
            {
                if (member.Id.Value == SteamClient.SteamId.Value)
                {
                    continue;
                }

                SteamNetworking.SendP2PPacket(member.Id, compressed, nChannel: channel.Value);
            }
        }
    }

    public bool IsServerClosed()
    {
        return _closed;
    }

    public void ServerClose()
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        _closed = true;
        var sessions = GetSessions();
        foreach (var player in sessions)
        {
            ServerClose(player.SteamId);
        }
        
        LeaveLobbyAsync().Wait();
    }

    public void ServerClose(SteamId target)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        if (target.Value == SteamClient.SteamId)
        {
            return;
        }

        _logger.LogInformation("try kick player: {SteamId}", target);
        SendP2PPacket(target, NetChannel.GameState, new ServerClosePacket(), false);
        // SteamNetworking.CloseP2PSessionWithUser(target);
    }

    public void KickPlayer(SteamId target)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        // 자기 자신은 킥을 하지 못한다.
        if (target.Value == SteamClient.SteamId)
        {
            return;
        }

        _logger.LogInformation("try kick player: {Member}", target);
        SendP2PPacket(target, NetChannel.GameState, new KickPacket(), false);
        // SteamNetworking.CloseP2PSessionWithUser(target);
    }

    public void TempBanPlayer(SteamId target, bool update = true)
    {
        if (!_banned.Add(target))
            return;

        _logger.LogInformation("try ban player: {Member}", target);
        SendP2PPacket(target, NetChannel.GameState, new BanPacket(), false);
        BroadcastP2PPacket(NetChannel.GameState, new ForceDisconnectPlayerPacket { UserId = target }, false);
        // SteamNetworking.CloseP2PSessionWithUser(target);
        
        if (update) UpdateBannedPlayers();
    }

    public void BanPlayers(string[] banPlayers)
    {
        foreach (var banPlayer in banPlayers)
        {
            if (ulong.TryParse(banPlayer, out var steamId))
            {
                TempBanPlayer(steamId, false);
            }
        }

        UpdateBannedPlayers();
    }

    public void RemoveBanPlayer(SteamId target)
    {
        if (!_banned.Remove(target))
        {
            return;
        }

        UpdateBannedPlayers();
    }

    ////////////////////////////////////////////////////////////////////////////////

    #region Steamworks Callbacks

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            _logger.LogError("failed to create lobby: {Result}", result);
            return;
        }

        _logger.LogInformation("lobby created: id({LobbyId}) roomCode({RoomCode}) lobbyType({LobbyType}) public({Public}) adult({Adult}) cap({Cap})", lobby.Id, _code, _lobbyType, _public, _adult, _cap);

        _lobby = lobby;
        SetupLobby(lobby);
        UpdateBrowserValue();
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend member)
    {
        _logger.LogInformation("lobby member joined: {Member}", member);

        var session = new Session
        {
            Friend = member,
            SteamId = member.Id,
            ConnectTime = DateTimeOffset.UtcNow
        };

        if (!_sessions.TryAdd(member.Id.Value, session))
        {
            _logger.LogWarning("failed to add session: {Member}", member);
            KickPlayer(session.SteamId);
            return;
        }

        GameEventBus.Publish(new PlayerJoinedEvent(member.Id));
        UpdateConsoleTitle();
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend member)
    {
        SteamNetworking.CloseP2PSessionWithUser(member.Id);

        _logger.LogInformation("lobby member left: {Member}", member);
        RemoveSession(member);
    }

    private void OnLobbyMemberDisconnected(Lobby lobby, Friend member)
    {
        SteamNetworking.CloseP2PSessionWithUser(member.Id);

        _logger.LogWarning("lobby member disconnected: {Member}", member);
        if (member.Id == SteamClient.SteamId)
        {
            _logger.LogWarning("host disconnected: {Member}", member);
            _created = false;
            return;
        }

        RemoveSession(member);
    }

    private void OnLobbyMemberDataChanged(Lobby lobby, Friend member)
    {
        _logger.LogInformation("lobby member data changed: {Member}", member);

        SelectSession(member.Id, session => { session.Friend = member; });
    }

    private void OnLobbyInvite(Friend arg1, Lobby arg2)
    {
        _logger.LogWarning("lobby invite: {Friend} {Lobby}", arg1, arg2.Id);
    }

    private void OnLobbyEntered(Lobby obj)
    {
        _logger.LogInformation("lobby entered: {Lobby}", obj.Id);
    }

    private void OnLobbyGameCreated(Lobby arg1, uint arg2, ushort arg3, SteamId arg4)
    {
        _logger.LogWarning("lobby game created: {Lobby} {Ip} {Port} {SteamId}", arg1.Id, arg2, arg3, arg4);
    }

    private void OnLobbyDataChanged(Lobby obj)
    {
        _logger.LogDebug("lobby data changed: {Lobby}", obj.Id);
    }

    private void OnLobbyMemberKicked(Lobby arg1, Friend arg2, Friend arg3)
    {
        _logger.LogWarning("lobby member kicked: {Friend} {Friend}", arg2, arg3);
    }

    private void OnLobbyMemberBanned(Lobby arg1, Friend arg2, Friend arg3)
    {
        _logger.LogWarning("lobby member banned: {Friend} {Friend}", arg2, arg3);
    }

    private void OnChatMessage(Lobby arg1, Friend arg2, string arg3)
    {
        _logger.LogWarning("chat message: {Friend} {Message}", arg2, arg3);
    }

    private void OnP2PSessionRequest(SteamId requester)
    {
        _logger.LogWarning("p2p session request: {SteamId}", requester);

        if (_banned.Contains(requester))
        {
            _logger.LogWarning("banned player request: {SteamId}", requester);
            SteamNetworking.CloseP2PSessionWithUser(requester);
            return;
        }

        if (IsServerClosed())
        {
            _logger.LogWarning("server closed: {SteamId}", requester);
            ServerClose(requester);
            return;
        }
        
        _logger.LogWarning("accept p2p session: {SteamId}", requester);
        SteamNetworking.AcceptP2PSessionWithUser(requester);
        
        if (GetSession(requester) == null)
        {
            _logger.LogWarning("but.. not found session: {SteamId}", requester);
            ServerClose(requester);
            return;
        }
        
        _logger.LogWarning("and.. found session: {SteamId}", requester);
        BroadcastP2PPacket(NetChannel.GameState, new HandshakePacket());
    }

    #endregion
}