using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;

public class CustomNetwork : NetworkManager
{
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    [SerializeField] private int _minPlayerCount = 2;
    [SerializeField] private NetworkRoomPlayer _roomPlayerPrefab;
    [SerializeField] private Player _gamePlayerPrefab;
    [SerializeField] private Scene _gameScene;

    public override void Start()
    {
        base.Start();       

        if (Application.isBatchMode)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
            op.completed += OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(AsyncOperation op)
    {
        _gameScene = SceneManager.GetSceneByName("GameScene");
    }

    [Server]
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log("Client connected, try inst prefab lobby");
        NetworkRoomPlayer roomPlayerInstance = Instantiate(_roomPlayerPrefab);
        NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
    }

    [Server]
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            RemovePlayerServer(conn.identity.GetComponent<Player>());
        }      
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        NetworkClient.AddPlayer();
        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        OnClientDisconnected?.Invoke();
    }

    [Server]
    public void StartGame(LobbyRoom room)
    {
        GameRoom gameRoom = new GameRoom();
        gameRoom.id = room.id;

        foreach (var player in room.Players)
        {
            SceneMessage msg = new SceneMessage
            {
                sceneName = "GameScene",
                sceneOperation = SceneOperation.LoadAdditive
            };
            player.connectionToClient.Send(msg);

            Player gamePlayer = Instantiate(_gamePlayerPrefab);
            gamePlayer.SetDisplayName(player.DisplayName);
            SceneManager.MoveGameObjectToScene(gamePlayer.gameObject, _gameScene);
            NetworkServer.ReplacePlayerForConnection(
                player.connectionToClient,
                gamePlayer.gameObject,
                true
            );
            gameRoom.Players.Add(gamePlayer);
            NetworkServer.Destroy(player.gameObject);
        }

        ServiceLocator<IService>.Instance.Get<ServerGameRoomService>().SetNewGameRoom(gameRoom);
    }

    [ServerCallback]
    private void RemovePlayerServer(Player player)
    {
        if (ServiceLocator<IService>.Instance
            .Get<ServerGameRoomService>() == default)
            return;

        ServiceLocator<IService>.Instance
            .Get<ServerGameRoomService>()
            .RemovePlayer(player);
    }
}
