using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Mirror.Discovery;

public class CustomNetwork : NetworkManager
{
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    [SerializeField] private int _minPlayerCount = 2;
    [SerializeField] private NetworkRoomPlayerLobby _roomPlayerPrefab;
    [SerializeField] private string menuScene;

    private bool _isGameStarted;

    public List<NetworkRoomPlayerLobby> RoomPlayersList { get; } = new List<NetworkRoomPlayerLobby>();

    private List<Player> _gamePlayers = new List<Player>();
    private int _connectionsCount = 0;

    public override void Start()
    {
        base.Start();

        ServiceLocator<IService>.OnServiceRegistered += ValidateService;
    }

    private void ValidateService(IService service)
    {
        if (service.GetType() == typeof(ServerPlayersService))
        {
            _isGameStarted = true;

            for (int i = 0; i < _gamePlayers.Count; i++)
            {
                AddPlayerServer(_gamePlayers[i]);
            }
        }

        if (service.GetType() == typeof(ServerSpawnService))
        {
            for (int i = 0; i < _gamePlayers.Count; i++)
            {
                _gamePlayers[i].ReserveSpawnPoint(ServiceLocator<IService>.Instance.Get<ServerSpawnService>().GetPoint(i));
            }
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == menuScene)
        {
            bool isLeader = RoomPlayersList.Count == 0;
            NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(_roomPlayerPrefab);
            roomPlayerInstance.IsLeader = isLeader;
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }

        _connectionsCount++;
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (_connectionsCount >= maxConnections)
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        _connectionsCount--;
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayerLobby>();
            RoomPlayersList.Remove(player);
            NotifyPlayersOfReadyState();
        }

        if (_isGameStarted)
        {
            RemovePlayerServer(conn.identity.GetComponent<Player>());
        }

        NetworkServer.DestroyPlayerForConnection(conn);
    }



    public override void OnClientConnect()
    {
        base.OnClientConnect();
        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        OnClientDisconnected?.Invoke();
    }

    public void AddRoomPlayer(NetworkRoomPlayerLobby player)
    {
        RoomPlayersList.Add(player);
        NotifyPlayersOfReadyState();
    }

    public void RemoveRoomPlayer(NetworkRoomPlayerLobby player)
    {
        RoomPlayersList.Remove(player);
        NotifyPlayersOfReadyState();
        Debug.Log("RemoveRoomPlayer");
    }

    [Server]
    public void StartGame()
    {
        if (SceneManager.GetActiveScene().name == menuScene)
        {
            if (!IsReadyToStart()) return;

            ServerChangeScene("MainScene");
        }
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayersList)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < _minPlayerCount) return false;

        foreach (var player in RoomPlayersList)
        {
            if (!player.IsReady) return false;
        }

        return true;
    }

    [Server]
    public override void ServerChangeScene(string newSceneName)
    {
        if (SceneManager.GetActiveScene().name == menuScene && newSceneName.StartsWith("MainScene"))
        {
            for (int i = 0; i < RoomPlayersList.Count; i++)
            {
                var conn = RoomPlayersList[i].connectionToClient;
                GameObject player = Instantiate(playerPrefab, new Vector3(0f, 500f, 0f), Quaternion.identity);

                player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
                Player connectedPlayer = player.GetComponent<Player>();
                connectedPlayer.SetDisplayName(RoomPlayersList[i].DisplayName);
                DontDestroyOnLoad(connectedPlayer.gameObject);            
                NetworkServer.ReplacePlayerForConnection(conn, player.gameObject);
                //NetworkServer.Destroy(RoomPlayersList[i].gameObject);
                _gamePlayers.Add(connectedPlayer);
            }
        }

        base.ServerChangeScene(newSceneName);
    }

    [ServerCallback]
    private void AddPlayerServer(Player player)
    {
        ServiceLocator<IService>.Instance.Get<ServerPlayersService>().AddPlayer(player);
    }

    [ServerCallback]
    private void RemovePlayerServer(Player player)
    {
        ServiceLocator<IService>.Instance.Get<ServerPlayersService>().RemovePlayer(player);
    }
}
