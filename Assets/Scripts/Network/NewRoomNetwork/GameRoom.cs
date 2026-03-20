using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameRoom
{
    public Guid id;
    public string name;
    public List<Player> Players = new();

    private GameLoopStateMachine _stateMachine;
    private MapService _mapService;    
    private GameplayService _gameplayService;

    [Server]
    public void InitRoom(Box woodBox, List<Box> concreteCommonBoxList)
    {
        _gameplayService = new GameplayService();
        _mapService = new MapService(id, woodBox, concreteCommonBoxList, _gameplayService);
        _stateMachine = new GameLoopStateMachine(_mapService, _gameplayService, id);
        _gameplayService.InitializeService(_stateMachine, _mapService, Players, id);

        Debug.Log($"ConcreteListCount is : {concreteCommonBoxList.Count}");
        _mapService.InitializeService();

        foreach (var player in Players)
        {
            var nm = player.GetComponent<NetworkMatch>();
            nm.matchId = id;
            player.SetMatchId(id);
            _mapService.RegisterPlayer(player.GetComponent<IMapObject>());
        }
    }

    [Server]
    public void RemovePlayerFromRoom(Player player)
    {
        _gameplayService.RemoveLeavePlayer(player);
        _mapService.UnregisterPlayer(player);

        if(Players.Contains(player))
            Players.Remove(player);
    }

    [Server]
    public void DestroyRoom()
    {
        Players.Clear();

        _stateMachine = null;
        _mapService.Dispose();
        _mapService = null;
        _gameplayService.Dispose();
        _gameplayService = null;
    }
}
