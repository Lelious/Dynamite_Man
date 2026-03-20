using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ServerGameRoomService : NetworkBehaviour, IService
{
    [SerializeField] private Box _woodBoxPrefab;
    [SerializeField] private List<Box> _concreteCommonBoxesList;
    private Dictionary<Guid, GameRoom> _rooms = new();

    [Server]
    public override void OnStartServer()
    {
        ServiceLocator<IService>.Instance.Register(this);
        NetworkServer.SpawnObjects();
    }

    [Server]
    public void SetNewGameRoom(GameRoom room)
    {
        Debug.Log($"Recieved gameRoom, clients : {room.Players.Count}");

        _rooms.Add(room.id, room);
        room.InitRoom(_woodBoxPrefab, _concreteCommonBoxesList);       
    }

    [Server]
    public void RemovePlayer(Player player)
    {
        if(player == null)
        {
            Debug.Log("no player");
        }
        else
        {
            Guid matchId = player.GetMatchGuid();
            if (_rooms.TryGetValue(matchId, out GameRoom room))
            {
                room.RemovePlayerFromRoom(player);

                if (room.Players.Count == 0)
                {
                    room.DestroyRoom();
                    _rooms.Remove(matchId);
                    Debug.Log("No players in room, destroying...");
                }
            }

            NetworkServer.DestroyPlayerForConnection(player.connectionToClient);        
        }
    }
}
