using System.Buffers;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.InteropServices;
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
    private readonly ConcurrentDictionary<ulong, SteamNetworkingIdentity> _identities = [];
    private readonly ConcurrentDictionary<ulong, Session> _sessions = [];

    private const int MaxChatMessageByteLength = 4 * 1024; // 4KB

    private readonly Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;
    private readonly Callback<LobbyChatMsg_t> _lobbyChatMsgCallback;
    private readonly Callback<SteamNetworkingMessagesSessionRequest_t> _sessionRequestCallback;

    public SessionManager()
    {
        _logger = Log.Factory.CreateLogger<SessionManager>();
        _lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        _lobbyChatMsgCallback = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsg);
        _sessionRequestCallback = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequest);
    }

    public void Dispose()
    {
        ServerClose();

        _lobbyChatUpdateCallback.Dispose();
        _lobbyChatMsgCallback.Dispose();
        _sessionRequestCallback.Dispose();
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
        SendPacket(target, NetChannel.GameState, new ServerClosePacket(), false);
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
            // BanPlayerNoEvent(steamId);
            return false;
        }

        _logger.LogInformation("try create session: {SteamId}", steamId.m_SteamID);
        var session = new Session(steamId);
        if (_sessions.TryAdd(steamId.m_SteamID, session))
        {
            SteamMatchmaking.SetLobbyData(LobbyManager.Inst.GetLobbyId(), "count", _sessions.Count.ToString());
            BroadcastPacket(NetChannel.GameState, new UserJoinedWebLobbyPacket { UserId = (long)steamId.m_SteamID }, false);
            GameEventBus.Publish(new PlayerJoinedEvent(session.SteamId, session.Name));
            return true;
        }
        
        _logger.LogWarning("failed to create session: {SteamId}", steamId.m_SteamID);
        return false;
    }

    private bool TryRemoveSession(CSteamID steamId)
    {
        _logger.LogWarning("try remove session: {SteamId}", steamId.m_SteamID);

        if (!_sessions.TryRemove(steamId.m_SteamID, out var session))
        {
            _logger.LogWarning("failed to remove session: {SteamId}", steamId.m_SteamID);
            return false;
        }

        var identity = session.Identity;
        SteamNetworkingMessages.CloseSessionWithUser(ref identity);
        _identities.TryRemove(steamId.m_SteamID, out _);
        
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
        BroadcastPacket(NetChannel.GameState, new PeerWasKickedPacket(), false);
        SendPacket(target, NetChannel.GameState, new ClientWasKickedPacket(), false);
    }

    public bool IsBannedPlayer(CSteamID target)
    {
        return _banned.Contains(target.m_SteamID.ToString(CultureInfo.InvariantCulture));
    }

    public void BanPlayerNoEvent(CSteamID target)
    {
        _logger.LogInformation("try ban player: {SteamId}", target.m_SteamID);

        if (_sessions.ContainsKey(target.m_SteamID))
        {
            SendPacket(target, NetChannel.GameState, new ClientWasBannedPacket(), false);
            BroadcastPacket(NetChannel.GameState, new PeerWasBannedPacket { UserId = (long)target.m_SteamID }, false);
        }

        _banned.Add(target.m_SteamID.ToString(CultureInfo.InvariantCulture));
        LobbyManager.Inst.UpdateBannedPlayers(_banned);
    }

    public void BanPlayer(CSteamID target)
    {
        BanPlayerNoEvent(target);
        GameEventBus.Publish(new PlayerBanEvent(target));
    }

    public void BanPlayers(string[] banPlayers)
    {
        foreach (var banPlayer in banPlayers)
        {
            if (ulong.TryParse(banPlayer, NumberStyles.Any, CultureInfo.InvariantCulture, out var steamId))
            {
                BanPlayer(new CSteamID(steamId));
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

    public void SendPacket(CSteamID steamId, NetChannel channel, Packet packet, bool useSession = true)
    {
        var data = PacketHelper.ToDictionary(packet);
        SendPacketObject(steamId, channel, data, useSession);
    }

    public void SendPacketObject(CSteamID steamId, NetChannel channel, object data, bool useSession = true)
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

            session.Packets.Enqueue((channel, data));
        }
        else
        {
            var bytes = GodotBinaryConverter.Serialize(data);
            var compressed = GZipHelper.Compress(bytes);
            var identity = GetIdentity(steamId);
            
            SteamNetworkHelper.SendMessageToUser(ref identity, channel, compressed);
        }
    }
    
    private SteamNetworkingIdentity GetIdentity(CSteamID steamId)
    {
        return _identities.TryGetValue(steamId.m_SteamID, out var identity) ? identity : new SteamNetworkingIdentity();
    }

    public void BroadcastPacket(NetChannel channel, Packet packet, bool useSession = true)
    {
        var data = PacketHelper.ToDictionary(packet);
        BroadcastPacketObject(channel, data, useSession);
    }

    public void BroadcastPacketObject(NetChannel channel, object data, bool useSession = true)
    {
        if (useSession)
        {
            if (_sessions.Count > 0)
            {
                foreach (var session in _sessions.Values)
                {
                    session.Packets.Enqueue((channel, data));
                }
            }
        }
        else
        {
            var bytes = GodotBinaryConverter.Serialize(data);
            var compressed = GZipHelper.Compress(bytes);
            SteamNetworkHelper.SendMessageToMultipleUsers(_sessions.Values.Select(x => x.Identity).ToArray(), channel, compressed);
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
                BroadcastPacket(NetChannel.GameState, new UserLeftWebLobbyPacket { UserId = (long)changedUser.m_SteamID, Reason = (long)LobbyLeftReason.UserLeave }, false);
            }
        }
        else if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
        {
            _logger.LogWarning("lobby member disconnected: {ChangedUserId}", changedUser);
            if (TryRemoveSession(changedUser))
            {
                BroadcastPacket(NetChannel.GameState, new UserLeftWebLobbyPacket { UserId = (long)changedUser.m_SteamID }, false);
            }
        }
    }

    private void OnSessionRequest(SteamNetworkingMessagesSessionRequest_t param)
    {
        var identity = param.m_identityRemote;
        var steamId = identity.GetSteamID64();
        _logger.LogWarning("session request: {SteamId}", steamId);
        
        if (_sessions.TryGetValue(steamId, out var session))
        {
            SteamNetworkingMessages.AcceptSessionWithUser(ref identity);
            session.Identity = identity;
            var webLobbyPacket = ReceiveWebLobbyPacket.Create(_sessions.Values.Select(x => x.SteamId).ToList());
            SendPacket(session.SteamId, NetChannel.GameState, webLobbyPacket, false);
            
            _identities.TryAdd(steamId, identity);
            return;
        }
        
        SteamNetworkingMessages.CloseSessionWithUser(ref identity);
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