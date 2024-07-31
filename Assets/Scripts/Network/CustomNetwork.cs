using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;

public class CustomNetwork : NetworkManager
{
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    [SerializeField] private NetworkRoomPlayerLobby _roomPlayerPrefab;
    [Scene] [SerializeField] private string menuScene;

    private int _connectionsCount = 0;
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == menuScene)
        {
            NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(_roomPlayerPrefab);
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
        else
        {
            GameObject player = Instantiate(playerPrefab, new Vector3(0f, 500f, 0f), Quaternion.identity);

            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            Player connectedPlayer = player.GetComponent<Player>();

            AddPlayerServer(connectedPlayer, ServiceLocator<IService>.Instance.Get<ServerSpawnService>().GetPoint(_connectionsCount));
            NetworkServer.AddPlayerForConnection(conn, player);
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

        RemovePlayerServer(conn.identity.GetComponent<Player>());
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

    [ServerCallback]
    private void AddPlayerServer(Player player, Transform transform)
    {
        player.ReserveSpawnPoint(transform);
        ServiceLocator<IService>.Instance.Get<ServerPlayersService>().AddPlayer(player);
    }

    [ServerCallback]
    private void RemovePlayerServer(Player player)
    {
        ServiceLocator<IService>.Instance.Get<ServerPlayersService>().RemovePlayer(player);
    }
}
