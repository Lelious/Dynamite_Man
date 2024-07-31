using UnityEngine;

public class GameState : State
{
    private GameLoopStateMachine _gameLoopStateMachine;
    private ServerPlayersService _serverPlayersService;

    public GameState(GameLoopStateMachine gameLoopStateMachine) : base(gameLoopStateMachine) 
    { 
        _gameLoopStateMachine = gameLoopStateMachine;
        _serverPlayersService = ServiceLocator<IService>.Instance.Get<ServerPlayersService>();
    }

    public override void Enter()
    {
        _serverPlayersService.StartGame();
    }

    public override void Exit()
    {

    }
}
