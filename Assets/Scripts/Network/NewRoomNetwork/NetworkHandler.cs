using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkHandler : NetworkBehaviour
{
    [SerializeField] private NetworkRoomPlayer _roomPlayerPrefab;
    [SerializeField] private kcp2k.KcpTransport _transport;

    [ClientCallback]
    public void TargetConnectGame()
    {
        NetworkManager.singleton.StopClient();
        NetworkManager.singleton.networkAddress = "127.0.0.1";
        _transport.Port = (ushort)7778;
        NetworkManager.singleton.StartClient();
    }
}
