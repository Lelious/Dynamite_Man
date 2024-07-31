using System.Threading.Tasks;

public class GameRestartState : State
{
    private GameLoopStateMachine _gameLoopStateMachine;
    private ServerPlayersService _serverPlayersService;
    public GameRestartState(GameLoopStateMachine gameLoopStateMachine) : base(gameLoopStateMachine) 
    { 
        _gameLoopStateMachine = gameLoopStateMachine;
        _serverPlayersService = ServiceLocator<IService>.Instance.Get<ServerPlayersService>();
    }

    public override void Enter()
    {
        _serverPlayersService.DisablePlayersControll();
        ServiceLocator<IService>.Instance.Get<ServerCollectablesService>().UnspawnAllCollectables();
        DelayedRestart();
    }

    public override void Exit()
    {

    }
    
    private async void DelayedRestart()
    {       
        await Task.Delay(5000);
        _gameLoopStateMachine.Enter<GameLoadState>();
    }
}
