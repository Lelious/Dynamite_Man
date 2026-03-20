using Mirror;
using System;
using System.Threading.Tasks;

public class GameRestartState : State
{
    private GameLoopStateMachine _gameLoopStateMachine;
    private CollectablesService _collectablesService;
    private GameplayService _gameplayService;
    private MapService _mapService;
    private Guid _matchId;

    public GameRestartState(GameLoopStateMachine gameLoopStateMachine, GameplayService gameplayService, Guid id, MapService mapService) : base(gameLoopStateMachine) 
    { 
        _gameLoopStateMachine = gameLoopStateMachine;
        _collectablesService = ServiceLocator<IService>.Instance.Get<CollectablesService>();
        _gameplayService = gameplayService;
        _mapService = mapService;
        _matchId = id;
    }

    [Server]
    public override void Enter()
    {
        _gameplayService.DisablePlayersControll();
        _collectablesService.UnspawnAllCollectables(_matchId);
        _mapService.ResetMapObjects();

        DelayedRestart();
    }

    [Server]
    public override void Exit()
    {

    }
    
    [Server]
    private async void DelayedRestart()
    {       
        await Task.Delay(5000);
        _gameplayService.ResetGame();
        _gameLoopStateMachine.Enter<GameState>();
    }
}
