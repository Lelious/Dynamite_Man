using Mirror;
using System.Threading.Tasks;

public class GameLoadState : State
{
    private GameLoopStateMachine _gameLoopStateMachine;
    private GameplayService _gameplayService;
    private MapService _mapService;

    public GameLoadState(GameLoopStateMachine gameLoopStateMachine, MapService mapService, GameplayService gameplayService) : base(gameLoopStateMachine) 
    { 
        _gameLoopStateMachine = gameLoopStateMachine;
        _gameplayService = gameplayService;
        _mapService = mapService;
    }

    [Server]
    public override void Enter()
    {
        _gameplayService.DisablePlayersControll();
        DelayedStart();
    }

    [Server]
    public override void Exit()
    {

    }

    [Server]
    private async void DelayedStart()
    {
        await Task.Delay(3000);
        _gameplayService.ResetGame();
        _gameLoopStateMachine.Enter<GameState>();
    }
}
