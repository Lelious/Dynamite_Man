using Mirror;
using System;
using System.Collections.Generic;

public class GameplayService : IDisposable
{
    private Guid _matchId;
    private CollectablesService _collectableService;
    private SpawnPointsService _spawnPointsService;
    private GameLoopStateMachine _stateMachine;
    private HashSet<Player> _playersList = new();
    private List<Player> _alivePlayers = new();
    private BombService _bombService;
    private MapService _mapService;

    [Server]
    public void InitializeService(GameLoopStateMachine stateMachine, MapService mapService, List<Player> players, Guid matchId)
    {
        _collectableService = ServiceLocator<IService>.Instance.Get<CollectablesService>();
        _spawnPointsService = ServiceLocator<IService>.Instance.Get<SpawnPointsService>();
        _bombService = ServiceLocator<IService>.Instance.Get<BombService>();
        _stateMachine = stateMachine;
        _mapService = mapService;
        _matchId = matchId;

        for (int i = 0; i < players.Count; i++)
        {
            players[i].InitServices();
            players[i].HidePlayer();
            players[i].SetGameplayService(this);
            players[i].SetStartBombCount(1);
            players[i].SetBombPower(1);
            players[i].SetBombExplodeTime(_bombService.GetBombExplodeTime());
            players[i].ReserveSpawnPoint(_spawnPointsService.GetPoint(i));
            var controller = players[i].GetComponent<PlayerController>();
            controller.InitController();

            _playersList.Add(players[i]);
        }

        _stateMachine.Enter<GameLoadState>();
    }

    [Server]
    public void RemoveLeavePlayer(Player player)
    {
        if(_playersList.Contains(player))
            _playersList.Remove(player);

        if(_alivePlayers.Contains(player))
            _alivePlayers.Remove(player);


    }

    [Server]
    public void ResetGame()
    {
        _alivePlayers.Clear();

        foreach (var player in _playersList)
        {
            player.AppearPlayer();
            _alivePlayers.Add(player);
        }

        UnityEngine.Debug.Log($"Reset game, {_alivePlayers.Count}");
    }

    [Server]
    public void ImprovePlayerStats(Player player, ref StatImprovement improvement)
    {
        switch (improvement.StatType)
        {
            case StatType.Bomb:
                player.IncreaceMaxBombCount((int)improvement.Amount);
                break;
            case StatType.Power:
                player.IncreaceBombPower((int)improvement.Amount);
                break;
            case StatType.Speed:
                player.IncreaceSpeed(improvement.Amount);
                break;
        }
    }

    [Server]
    public void ProcessBombDamage(Player bombOwner, List<IMapObject> mapObjectList)
    {
        UnityEngine.Debug.Log($"Bomb process, {mapObjectList.Count} : {_alivePlayers.Count}");
        foreach (var mapObject in mapObjectList)
        {
            if(mapObject.GetMapObjectType().Equals(MapObjectType.Player))
            {
                var player = mapObject.GetObject().GetComponent<Player>();
                if (player.Equals(bombOwner))
                {
                    bombOwner.Deaths++;                   
                }
                else
                {
                    bombOwner.Kills++;
                }

                _alivePlayers.Remove(player);
            }
            mapObject.GetObject().GetComponent<IDamagable>().TakeDamage();
        }

        CheckRoundConditions();
    }

    [Server]
    public void PlaceBomb(Player player)
    {
        _bombService.PlaceBomb(player, _mapService, this);
    }

    [Server]
    public void StartGameRound()
    {
        foreach (var player in _playersList)
        {
            player.RpcEnableControll();
            player.AppearPlayer();
        }
    }

    [Server]
    public void DisablePlayersControll()
    {
        foreach (var player in _playersList)
        {
            player.RpcDisableControll();
            player.HidePlayer();
        }
    }

    [Server]
    private void CheckRoundConditions()
    {
        if (_alivePlayers.Count > 1) return;

        if (_alivePlayers.Count == 1)
        {
            _alivePlayers[0].Wins++;
            UnityEngine.Debug.Log("one player alive, it's winner!");
            //Win screen;
        }
        else if(_alivePlayers.Count == 0)
        {
            //Draw screen;
            UnityEngine.Debug.Log("No winner, draw!");
        }
        _stateMachine.Enter<GameRestartState>();
    }

    [Server]
    public void Dispose()
    {
        _collectableService.UnspawnAllCollectables(_matchId);
        _collectableService = null;
        _spawnPointsService = null;
        _stateMachine = null;
        _playersList.Clear();
        _alivePlayers.Clear();
        _bombService = null;
        _mapService = null;
    }
}
