using System.Collections.Generic;
using UnityEngine;

public sealed class ServerPlayersService : IService
{
    private GameLoopStateMachine _stateMachine;
    private HashSet<Player> _connectedPlayers;
    private HashSet<IMapObject> _mapPlayers;
    private int _activePlayers;
    private List<Player> _alivePlayers;

    public ServerPlayersService()
    {
        _mapPlayers = new HashSet<IMapObject>();
        _connectedPlayers = new HashSet<Player>();
        _alivePlayers = new List<Player>();

        Player.OnPlayerDead += ChangePlayerState;
        ServiceLocator<IService>.Instance.Register(this);
    }

    public void InitService(GameLoopStateMachine loopStateMachine)
    {
        _stateMachine = loopStateMachine;
    }

    public void AddPlayer(Player player)
    {
        _connectedPlayers.Add(player);
        _mapPlayers.Add(player.GetComponent<IMapObject>());

        if (_stateMachine == null)
        {
            _activePlayers++;
        }
        else
        {
            var state = _stateMachine.GetCurrentState();

            if (state.Equals(typeof(GameLoadState)))
            {
                _activePlayers++;
                player.AppearPlayer();
            }
            else
            {
                player.RpcDisableControll();
                player.HidePlayer();
            }
        }

        player.SetBombExplodeTime();
    }

    public HashSet<IMapObject> GetMapPlayers() => _mapPlayers;

    public void RemovePlayer(Player player)
    {
        _connectedPlayers.Remove(player);
        _mapPlayers.Remove(player);
        _activePlayers--;
    }

    public void ResetGame()
    {
        _alivePlayers.Clear();

        foreach (var player in _connectedPlayers)
        {
            player.AppearPlayer();
            _alivePlayers.Add(player);
        }

        _activePlayers = _connectedPlayers.Count;
    }

    public void StartGame()
    {
        Debug.Log($"start game, {_connectedPlayers.Count}");
        foreach (var player in _connectedPlayers)
        {
            player.InitServices();
            player.SetStartBombCount(1);
            player.SetBombPower(1);
            var controller = player.GetComponent<PlayerController>();
            controller.SetSpeed(3f);
            controller.InitController();
            player.AppearPlayer();
            player.RpcEnableControll();
        }
    }

    public void DisablePlayersControll()
    {
        foreach (var plr in _connectedPlayers)
        {
            plr.RpcDisableControll();
        }
    }

    public void EnablePlayersControll()
    {
        foreach (var player in _connectedPlayers)
        {
            player.RpcEnableControll();
        }
    }

    public void ImprovePlayerStats(Player player, ref StatImprovement stat)
    {
        switch (stat.StatType)
        {
            case StatType.Bomb:
                player.IncreaceMaxBombCount((int)stat.Amount);
                break;
            case StatType.Power:
                player.IncreaceBombPower((int)stat.Amount);
                break;
            case StatType.Speed:
                player.GetComponent<PlayerController>().SetSpeed(stat.Amount);
                break;
        }
    }

    private void ChangePlayerState(Player player)
    {
        _activePlayers--;
        player.Deaths++;
        _alivePlayers.Remove(player);

        if (_activePlayers == 1)
        {
            _stateMachine.Enter<GameRestartState>();
            _alivePlayers[0].Wins++;
        }
    }
}
