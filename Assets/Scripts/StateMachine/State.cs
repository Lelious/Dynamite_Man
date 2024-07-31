public abstract class State : IState
{
    protected GameLoopStateMachine GameLoopStateMachine;

    protected State(GameLoopStateMachine gameLoopStateMachine)
    {
        GameLoopStateMachine = gameLoopStateMachine;
    }
    public abstract void Enter();
    public abstract void Exit();
}
