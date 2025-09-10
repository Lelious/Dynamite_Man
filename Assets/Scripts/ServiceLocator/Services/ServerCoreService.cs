using Mirror;

public class ServerCoreService : NetworkBehaviour, IService
{
    public readonly float BombExplodeTime = 2f;

    private GameLoopStateMachine _stateMachine;
    private ServerPlayersService _playersService;

    [ServerCallback]
    private void Start()
    {
        ServiceLocator<IService>.Instance.Register(this);

        _playersService = new ServerPlayersService();
        _stateMachine = new GameLoopStateMachine();
        _playersService.InitService(_stateMachine);       
        _stateMachine.Enter<GameLoadState>();

    }
}
