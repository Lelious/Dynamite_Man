using Mirror;
using System;
using System.Collections.Generic;

public class LobbyManager : NetworkBehaviour
{
    public Dictionary<Guid, LobbyRoom> Rooms = new();
    public static LobbyManager Instance { get; private set; }

    private CustomNetwork _customNetwork;
    private int _minPlayerCount = 2;

    private void Awake()
    {
        Instance = this;
    }

    [Server]
    private void Start()
    {
        _customNetwork = NetworkManager.singleton as CustomNetwork;
    }

    [Server]
    public void CreateRoom(NetworkRoomPlayer host, string roomName)
    {
        Guid id = Guid.NewGuid();
        LobbyRoom room = new LobbyRoom();

        room.id = id;
        room.leader = host;
        room.name = roomName;
        room.Players.Add(host);

        Rooms.Add(id, room);

        host.MatchId = id;
        host.IsLeader = true;
        NetworkMatch match = host.GetComponent<NetworkMatch>();
        match.matchId = id;
        host.TargetOpenLobby(host.connectionToClient);

        UpdateRoom(room);
    }

    [Server]
    public void JoinRoom(NetworkRoomPlayer player, Guid id)
    {
        if (!Rooms.ContainsKey(id)) return;

        LobbyRoom room = Rooms[id];

        if (room.Players.Count >= room.maxPlayers)
            return;

        room.Players.Add(player);
        player.MatchId = id;

        NetworkMatch match = player.GetComponent<NetworkMatch>();
        match.matchId = id;
        player.TargetOpenLobby(player.connectionToClient);

        UpdateRoom(room);
    }

    [Server]
    public void LeaveRoom(NetworkRoomPlayer player)
    {
        if (player.MatchId == Guid.Empty) return;

        if (!Rooms.ContainsKey(player.MatchId)) return;

        LobbyRoom room = Rooms[player.MatchId];

        room.Players.Remove(player);
        player.MatchId = Guid.Empty;

        NetworkMatch match = player.GetComponent<NetworkMatch>();
        match.matchId = Guid.Empty;

        if (room.Players.Count == 0)
            Rooms.Remove(room.id);
        player.TargetCloseLobby(player.connectionToClient);
    }

    [Server]
    public void StartGame(Guid id)
    {
        if(Rooms.TryGetValue(id, out LobbyRoom room))
        {
            if (!room.AllReady())
                return;
            _customNetwork.StartGame(room);

            Rooms.Remove(id);
        }
    }

    [Server]
    public void UpdateRoomCheckState(NetworkRoomPlayer player)
    {
        LobbyRoom room = Rooms[player.MatchId];
        UpdateRoom(room);
    }

    [Server]
    private void UpdateRoom(LobbyRoom room)
    {
        room.Players.Sort((a, b) =>
        {
            if (a == room.leader) return -1;
            if (b == room.leader) return 1;
            return 0;
        });

        List<LobbyPlayerInfo> playerInfoList = new();

        foreach (var player in room.Players)
        {
            playerInfoList.Add(new LobbyPlayerInfo(player.DisplayName, player.IsReady));    
        }

        foreach (var item in room.Players)
        {
            item.TargetUpdateInRoom(item.connectionToClient, playerInfoList);
        }
    }
}
