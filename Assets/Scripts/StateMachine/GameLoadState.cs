using Mirror;
using System.Threading.Tasks;

public class GameLoadState : State
{
    private GameLoopStateMachine _gameLoopStateMachine;
    private ServerPlayersService _serverPlayersService;
    public GameLoadState(GameLoopStateMachine gameLoopStateMachine) : base(gameLoopStateMachine) 
    { 
        _gameLoopStateMachine = gameLoopStateMachine;
        _serverPlayersService = ServiceLocator<IService>.Instance.Get<ServerPlayersService>();
    }

    public override void Enter()
    {
        _serverPlayersService.DisablePlayersControll();
        DelayedStart();
    }

    public override void Exit()
    {

    }

    private async void DelayedStart()
    {
        NetworkServer.SpawnObjects();
        await Task.Delay(3000);
        ServiceLocator<IService>.Instance.Get<MapService>().InitAllBoxes();
        _serverPlayersService.ResetGame();
        _gameLoopStateMachine.Enter<GameState>();
    }
}
