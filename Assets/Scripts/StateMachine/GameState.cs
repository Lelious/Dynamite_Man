using Mirror;

public class GameState : State
{
    private GameLoopStateMachine _gameLoopStateMachine;
    private GameplayService _gameplayService;

    public GameState(GameLoopStateMachine gameLoopStateMachine, GameplayService gameplayService) : base(gameLoopStateMachine) 
    { 
        _gameLoopStateMachine = gameLoopStateMachine;
        _gameplayService = gameplayService;
    }

    [Server]
    public override void Enter()
    {
        _gameplayService.StartGameRound();
    }

    [Server]
    public override void Exit()
    {

    }
}
