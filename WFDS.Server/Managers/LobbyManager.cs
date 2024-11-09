﻿using System.Collections.Concurrent;
using WFDS.Godot.Binary;
using WFDS.Server.Common;
using WFDS.Server.Common.Helpers;
using WFDS.Server.Packets;
using Microsoft.Extensions.Options;
using Steamworks;
using Steamworks.Data;

namespace WFDS.Server.Managers;

public sealed class LobbyManager : IDisposable
{
    private const int MaxPlayers = 16;
    private const string LobbyMode = "GodotsteamLobby";
    private const string LobbyRef = "webfishing_gamelobby";
    private const string GameVersion = "1.09";

    private readonly ILogger<LobbyManager> _logger;

    private readonly HashSet<ulong> _banned = [];
    private readonly ConcurrentDictionary<ulong, Session> _sessions = [];

    private bool _created;
    private string _name = string.Empty;
    private string _code = string.Empty;
    private GameLobbyType _lobbyType = GameLobbyType.Public;
    private int _cap;
    private bool _public;
    private bool _adult;

    private Lobby? _lobby;
    public string Code => _code;

    public LobbyManager(ILogger<LobbyManager> logger)
    {
        _logger = logger;

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
    }

    public void Dispose()
    {
        if (_lobby.HasValue)
        {
            var lobby = _lobby.Value;
            _lobby = null;
            SetPublic(false);

            BroadcastPacket(NetChannel.GameState, new ServerClosePacket());
            lobby.Leave();
        }

        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnected;
        SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequest;

        _banned.Clear();
        _sessions.Clear();
    }

    public void CreateLobby(string serverName, string code, GameLobbyType lobbyType, bool @public, bool adult, int cap = MaxPlayers)
    {
        if (_created)
        {
            return;
        }

        _code = code;
        if (_code.Length < 5 || _code.Length > 6 || string.IsNullOrWhiteSpace(_code))
        {
            _code = GenerationRoomCode();
        }

        _name = serverName;
        _public = @public;
        _lobbyType = lobbyType;
        _adult = adult;
        _cap = cap;
        _created = true;

        SteamMatchmaking.CreateLobbyAsync(_cap);
    }

    private void SetupLobby(Lobby lobby)
    {
        lobby.SetData("mode", LobbyMode);
        lobby.SetData("ref", LobbyRef);
        lobby.SetData("version", GameVersion);
        lobby.SetData("server_browser_value", "0");


        lobby.SetData("name", _name);
        lobby.SetData("lobby_name", _name);
        lobby.SetData("cap", _cap.ToString());
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

        SelectSession(target, session =>
        {
            _logger.LogInformation("try kicking player: {SteamId}", target);
            session.Send(NetChannel.GameState, new KickPacket());
        });
    }

    public void BanPlayer(SteamId target)
    {
        if (!_banned.Add(target))
        {
            return;
        }

        SelectSession(target, session =>
        {
            _logger.LogInformation("try kicking player: {SteamId}", target);
            session.Send(NetChannel.GameState, new KickPacket());
        });
        
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

    private void UpdateBannedPlayers()
    {
        _lobby?.SetData("banned_players", string.Join(",", _banned));
    }

    public bool IsSessionExists(SteamId steamId)
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

    public bool SelectSession(SteamId target, Action<Session> action)
    {
        if (!_sessions.TryGetValue(target.Value, out var session))
        {
            return false;
        }

        action(session);
        return true;
    }

    public void SendPacket(SteamId steamId, NetChannel channel, IPacket packet)
    {
        var data = packet.ToDictionary();
        SendPacket(steamId, channel, data);
    }

    public void SendPacket(SteamId steamId, NetChannel channel, object data)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        if (!_sessions.TryGetValue(steamId.Value, out var session))
        {
            return;
        }

        var bytes = GodotBinaryConverter.Serialize(data);
        var compressed = GZipHelper.Compress(bytes);
        session.Packets.Enqueue((channel, compressed));
    }

    public void BroadcastPacket(NetChannel channel, IPacket packet)
    {
        var data = packet.ToDictionary();
        BroadcastPacket(channel, data);
    }

    public void BroadcastPacket(NetChannel channel, object data)
    {
        if (!_lobby.HasValue)
        {
            return;
        }

        var bytes = GodotBinaryConverter.Serialize(data);
        var compressed = GZipHelper.Compress(bytes);

        foreach (var session in _sessions.Values)
        {
            if (session.SteamId.Value == SteamClient.SteamId.Value)
            {
                continue;
            }

            session.Packets.Enqueue((channel, compressed));
        }
    }

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
        _logger.LogInformation("lobby member joined: {DisplayName} [{SteamId}]", member.Name, member.Id);

        var session = new Session
        {
            LobbyManager = this,
            Friend = member,
            SteamId = member.Id,
            ConnectTime = DateTimeOffset.UtcNow
        };

        _sessions.TryAdd(member.Id.Value, session);
        UpdateConsoleTitle();
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend member)
    {
        _logger.LogInformation("lobby member left: {DisplayName} [{SteamId}]", member.Name, member.Id);
        _sessions.TryRemove(member.Id.Value, out _);
        UpdateConsoleTitle();
    }

    private void OnLobbyMemberDisconnected(Lobby lobby, Friend member)
    {
        _logger.LogWarning("lobby member disconnected: {DisplayName} [{SteamId}]", member.Name, member.Id);
        _sessions.TryRemove(member.Id.Value, out _);
        UpdateConsoleTitle();
    }

    private void OnP2PSessionRequest(SteamId requester)
    {
        _logger.LogWarning("P2P session request: {SteamId}", requester);
        SelectSession(requester, _ => { SteamNetworking.AcceptP2PSessionWithUser(requester); });
    }

    private static string GenerationRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var code = new char[6];
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
            if (!session.HandshakeReceived && now - session.ConnectTime > TimeSpan.FromSeconds(30)) // 30초 이상 핸드셰이크를 받지 않은 플레이어는 강퇴
            {
                KickPlayer(session.SteamId);
            }
        }
    }

    private void UpdateConsoleTitle()
    {
        Console.Title = $"[{_sessions.Count}/{_cap}] {_name} [{_code}]";
    }
}