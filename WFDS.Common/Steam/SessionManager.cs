using System.Buffers;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Steamworks;
using WFDS.Common.GameEvents;
using WFDS.Common.GameEvents.Events;
using WFDS.Common.Helpers;
using WFDS.Common.Network;
using WFDS.Common.Network.Packets;
using WFDS.Common.Types;
using WFDS.Godot.Binary;

namespace WFDS.Common.Steam;

public sealed class SessionManager : Singleton<SessionManager>, IDisposable
{
    private readonly ILogger _logger;

    private bool _closed;
    private readonly HashSet<ulong> _queue = [];
    private readonly HashSet<string> _banned = [];
    private readonly ConcurrentDictionary<ulong, Session> _sessions = [];

    const int MaxChatMessageByteLength = 4 * 1024; // 4KB

    private readonly Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;
    private readonly Callback<LobbyChatMsg_t> _lobbyChatMsgCallback;
    private readonly Callback<P2PSessionRequest_t> _p2pSessionRequestCallback;

    public SessionManager()
    {
        _logger = Log.Factory.CreateLogger<SessionManager>();
        _lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        _lobbyChatMsgCallback = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsg);
        _p2pSessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
    }

    public void Dispose()
    {
        ServerClose();

        _lobbyChatUpdateCallback.Dispose();
        _lobbyChatMsgCallback.Dispose();
        _p2pSessionRequestCallback.Dispose();
    }


    public bool IsServerClosed()
    {
        return _closed;
    }

    public void ServerClose()
    {
        if (_closed)
        {
            return;
        }

        _closed = true;
        var sessions = GetSessions();
        foreach (var player in sessions)
        {
            ServerClose(player.SteamId);
        }
    }

    public void ServerClose(CSteamID target)
    {
        if (target == SteamManager.Inst.SteamId)
        {
            return;
        }

        _logger.LogInformation("try server close player: {SteamId}", target.m_SteamID);
        SendP2PPacket(target, NetChannel.GameState, new ServerClosePacket(), false);
    }


    public int GetSessionCount()
    {
        return _sessions.Count;
    }

    public Session? GetSession(CSteamID steamId)
    {
        return _sessions.TryGetValue(steamId.m_SteamID, out var session) ? session : null;
    }

    public IEnumerable<Session> GetSessions()
    {
        return _sessions.Values;
    }

    public bool IsSessionValid(CSteamID steamId)
    {
        return _sessions.ContainsKey(steamId.m_SteamID);
    }

    private bool TryCreateSession(CSteamID steamId, out LobbyDenyReasons reason)
    {
        reason = LobbyDenyReasons.None;
        
        if (_sessions.TryGetValue(steamId.m_SteamID, out _))
        {
            reason = LobbyDenyReasons.Denied;
            _logger.LogWarning("session already exists: {SteamId}", steamId.m_SteamID);
            return false;
        }

        if (IsBannedPlayer(steamId))
        {
            _logger.LogWarning("banned player: {SteamId}", steamId.m_SteamID);
            reason = LobbyDenyReasons.Denied;
            // deny
            // SendP2PPacket(steamId, NetChannel.GameState, new UserLeftWebLobbyPacket { UserId = steamId.m_SteamID, Reason = LobbyDenyReasons.Denied }, false);
            // BanPlayerNoEvent(LobbyManager.Inst.GetLobbyId(), steamId);
            return false;
        }

        // accept
        BroadcastP2PPacket(LobbyManager.Inst.GetLobbyId(), NetChannel.GameState, new UserJoinedWebLobbyPacket { UserId = (long)steamId.m_SteamID }, false);
        SendP2PPacket(steamId, NetChannel.GameState, new ReceiveWebLobbyPacket { WebLobbyMembers = _sessions.Select(object (x) => (long)x.Key).ToList() }, false);

        _logger.LogInformation("try create session: {SteamId}", steamId.m_SteamID);
        var session = new Session(steamId);
        if (_sessions.TryAdd(steamId.m_SteamID, session))
        {
            GameEventBus.Publish(new PlayerJoinedEvent(steamId, session.Name));
            return true;
        }

        _logger.LogWarning("failed to create session: {SteamId}", steamId.m_SteamID);
        return false;
    }

    private bool TryRemoveSession(CSteamID steamId)
    {
        _logger.LogWarning("try remove session: {SteamId}", steamId.m_SteamID);
        SteamNetworking.CloseP2PSessionWithUser(steamId);

        if (!_sessions.TryRemove(steamId.m_SteamID, out var session))
        {
            _logger.LogWarning("failed to remove session: {SteamId}", steamId.m_SteamID);
            return false;
        }

        GameEventBus.Publish(new PlayerLeaveEvent(steamId, session.Name));
        return true;
    }

    public void KickPlayer(CSteamID target)
    {
        if (target == SteamManager.Inst.SteamId)
        {
            return;
        }

        _logger.LogInformation("try kick player: {SteamId}", target.m_SteamID);
        BroadcastP2PPacket(LobbyManager.Inst.GetLobbyId(), NetChannel.GameState, new PeerWasKickedPacket(), false);
        SendP2PPacket(target, NetChannel.GameState, new ClientWasKickedPacket(), false);
    }

    public bool IsBannedPlayer(CSteamID target)
    {
        return _banned.Contains(target.m_SteamID.ToString(CultureInfo.InvariantCulture));
    }

    public void BanPlayerNoEvent(CSteamID lobbyId, CSteamID target)
    {
        _logger.LogInformation("try ban player: {SteamId}", target.m_SteamID);
        SendP2PPacket(target, NetChannel.GameState, new ClientWasBannedPacket(), false);
        BroadcastP2PPacket(lobbyId, NetChannel.GameState, new PeerWasBannedPacket { UserId = (long)target.m_SteamID }, false);

        _banned.Add(target.m_SteamID.ToString(CultureInfo.InvariantCulture));
        LobbyManager.Inst.UpdateBannedPlayers(_banned);
    }

    public void BanPlayer(CSteamID lobbyId, CSteamID target)
    {
        BanPlayerNoEvent(lobbyId, target);
        GameEventBus.Publish(new PlayerBanEvent(target));
    }

    public void BanPlayers(CSteamID lobbyId, string[] banPlayers)
    {
        foreach (var banPlayer in banPlayers)
        {
            if (ulong.TryParse(banPlayer, NumberStyles.Any, CultureInfo.InvariantCulture, out var steamId))
            {
                BanPlayer(lobbyId, new CSteamID(steamId));
            }
        }
    }

    public void RemoveBanPlayer(CSteamID target)
    {
        if (!_banned.Remove(target.m_SteamID.ToString(CultureInfo.InvariantCulture)))
        {
            return;
        }

        LobbyManager.Inst.UpdateBannedPlayers(_banned);
        GameEventBus.Publish(new PlayerUnBanEvent(target));
    }

    public IEnumerable<string> GetBannedPlayers()
    {
        return _banned.Select(x => x.ToString(CultureInfo.InvariantCulture));
    }

    public void SendP2PPacket(CSteamID steamId, NetChannel channel, Packet packet, bool useSession = true)
    {
        var data = PacketHelper.ToDictionary(packet);
        SendP2PPacket(steamId, channel, data, useSession);
    }

    private void SendP2PPacket(CSteamID steamId, NetChannel channel, object data, bool useSession = true)
    {
        if (!steamId.IsValid())
        {
            return;
        }

        if (useSession)
        {
            if (!_sessions.TryGetValue(steamId.m_SteamID, out var session))
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
            SteamNetworkingHelper.SendP2PPacket(steamId, channel, compressed);
        }
    }

    public void BroadcastP2PPacket(CSteamID lobbyId, NetChannel channel, Packet packet, bool useSession = true)
    {
        var data = PacketHelper.ToDictionary(packet);
        BroadcastP2PPacket(lobbyId, channel, data, useSession);
    }

    private void BroadcastP2PPacket(CSteamID lobbyId, NetChannel channel, object data, bool useSession = true)
    {
        if (!lobbyId.IsValid() || !lobbyId.IsLobby())
        {
            return;
        }

        if (useSession)
        {
            if (_sessions.Count > 0)
            {
                var bytes = GodotBinaryConverter.Serialize(data);
                var compressed = GZipHelper.Compress(bytes);

                foreach (var session in _sessions.Values)
                {
                    session.Packets.Enqueue((channel, compressed));
                }
            }
        }
        else
        {
            SteamNetworkingHelper.BroadcastP2PPacket(lobbyId, channel, data);
        }
    }

    #region Steamworks Callbacks

    private void OnLobbyChatUpdate(LobbyChatUpdate_t param)
    {
        var lobbyId = new CSteamID(param.m_ulSteamIDLobby);
        var changedUser = new CSteamID(param.m_ulSteamIDUserChanged);
        var makingChange = new CSteamID(param.m_ulSteamIDMakingChange);

        var stateChange = (EChatMemberStateChange)param.m_rgfChatMemberStateChange;
        _logger.LogInformation("lobby member state changed: {LobbyId} {ChangedUserId} {MakingUserId} {StateChange}", lobbyId, changedUser, makingChange, stateChange);

        if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
        {
            _queue.Add(changedUser.m_SteamID);
        }
        else if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeLeft)
        {
            _logger.LogInformation("lobby member left: {ChangedUserId}", changedUser);
            if (TryRemoveSession(changedUser))
            {
                BroadcastP2PPacket(lobbyId, NetChannel.GameState, new UserLeftWebLobbyPacket { UserId = (long)changedUser.m_SteamID }, false);
            }
        }
        else if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
        {
            _logger.LogWarning("lobby member disconnected: {ChangedUserId}", changedUser);
            if (TryRemoveSession(changedUser))
            {
                BroadcastP2PPacket(lobbyId, NetChannel.GameState, new UserLeftWebLobbyPacket { UserId = (long)changedUser.m_SteamID }, false);
            }
        }
    }

    private void OnP2PSessionRequest(P2PSessionRequest_t param)
    {
        _logger.LogWarning("p2p session request: {SteamId}", param.m_steamIDRemote);

        if (IsBannedPlayer(param.m_steamIDRemote))
        {
            _logger.LogWarning("banned player request: {SteamId}", param.m_steamIDRemote);
            SteamNetworking.CloseP2PSessionWithUser(param.m_steamIDRemote);
            return;
        }

        if (IsServerClosed())
        {
            _logger.LogWarning("server closed: {SteamId}", param.m_steamIDRemote);
            ServerClose(param.m_steamIDRemote);
            return;
        }

        SteamNetworking.AcceptP2PSessionWithUser(param.m_steamIDRemote);
    }

    private void OnLobbyChatMsg(LobbyChatMsg_t param)
    {
        if (param.m_ulSteamIDLobby != LobbyManager.Inst.GetLobbyId().m_SteamID)
        {
            return;
        }
        
        if (param.m_ulSteamIDUser == SteamManager.Inst.SteamId.m_SteamID)
        {
            return;
        }
        
        if (!_queue.Contains(param.m_ulSteamIDUser))
        {
            return;
        }
        
        var lobbyId = new CSteamID(param.m_ulSteamIDLobby);
        var userId = new CSteamID(param.m_ulSteamIDUser);

        var chatEntryType = (EChatEntryType)param.m_eChatEntryType;
        var chatId = param.m_iChatID;

        var bytes = ArrayPool<byte>.Shared.Rent(MaxChatMessageByteLength);
        try
        {
            var length = SteamMatchmaking.GetLobbyChatEntry(lobbyId, (int)chatId, out var steamId, bytes, MaxChatMessageByteLength, out var entryType);
            var message = Encoding.UTF8.GetString(bytes, 0, length);
            
            _logger.LogInformation("lobby chat message: {LobbyId} {UserId} {ChatEntryType} {ChatId} {SteamId} {EntryType} {Message}", lobbyId, userId, chatEntryType, chatId, steamId, entryType, message);

            OnJoinRequest(userId, message);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
            _queue.Remove(userId.m_SteamID);
        }
    }

    private void OnJoinRequest(CSteamID sender, string message)
    {
        if (!message.StartsWith("$weblobby_join_request"))
        {
            return;
        }

        if (TryCreateSession(sender, out var reason))
        {
            SendAcceptMessage(sender);
            GameEventBus.Publish(new CreateSessionEvent(sender));
        }
        else
        {
            switch (reason)
            {
                case LobbyDenyReasons.LobbyFull:
                    SendLobbyFullMessage(sender);
                    break;
                case LobbyDenyReasons.Denied:
                    SendDenyMessage(sender);
                    break;
            }
        }
    }

    private void SendAcceptMessage(CSteamID target)
    {
        var lobbyId = LobbyManager.Inst.GetLobbyId();
        const string message = "$weblobby_request_accepted-";
        var body = Encoding.UTF8.GetBytes(message + target.m_SteamID);
        SteamMatchmaking.SendLobbyChatMsg(lobbyId, body, body.Length);
    }

    private void SendDenyMessage(CSteamID target)
    {
        var lobbyId = LobbyManager.Inst.GetLobbyId();
        const string message = "$weblobby_request_denied_deny-";
        var body = Encoding.UTF8.GetBytes(message + target.m_SteamID);
        SteamMatchmaking.SendLobbyChatMsg(lobbyId, body, body.Length);
    }

    private void SendLobbyFullMessage(CSteamID target)
    {
        var lobbyId = LobbyManager.Inst.GetLobbyId();
        const string message = "$weblobby_request_denied_full-";
        var body = Encoding.UTF8.GetBytes(message + target.m_SteamID);
        SteamMatchmaking.SendLobbyChatMsg(lobbyId, body, body.Length);
    }

    #endregion
}