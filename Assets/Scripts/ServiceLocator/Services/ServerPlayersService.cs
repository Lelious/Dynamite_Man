using System.Collections.Generic;

public sealed class ServerPlayersService : IService
{
    private GameLoopStateMachine _stateMachine;
    private HashSet<Player> _connectedPlayers;
    private HashSet<IMapObject> _mapPlayers;
    private int _activePlayers;

    public ServerPlayersService()
    {
        ServiceLocator<IService>.Instance.Register(this);      
        Player.OnPlayerDead += ChangePlayerState;

        _mapPlayers = new HashSet<IMapObject>();
        _connectedPlayers = new HashSet<Player>();
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
        _mapPlayers.Remove(player.GetComponent<IMapObject>());
        _activePlayers--;
    }

    public void ResetGame()
    {
        foreach (var player in _connectedPlayers)
        {
            player.AppearPlayer();
        }

        _activePlayers = _connectedPlayers.Count;
    }

    public void StartGame()
    {
        foreach (var player in _connectedPlayers)
        {
            player.SetStartBombCount(1);
            player.SetBombPower(1);
            player.GetComponent<PlayerController>().SetSpeed(2f);
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

        if (_activePlayers <= 1)
        {
            _stateMachine.Enter<GameRestartState>();
        }
    }
}
