using System;
using System.Collections.Generic;

public class GameLoopStateMachine : IService
{
    private Dictionary<Type, IState> _states;
    private IState _activeState;

    public IState ActiveState => _activeState;

    public GameLoopStateMachine(MapService mapService, GameplayService gameplayService, Guid id)
    {
        _states = new Dictionary<Type, IState>
        {
            { typeof(GameLoadState), new GameLoadState(this, mapService, gameplayService) },
            { typeof(GameState), new GameState(this, gameplayService) },
            { typeof(GameRestartState), new GameRestartState(this, gameplayService, id, mapService) },
        };
    }

    public void Enter<TState>() where TState : class, IState
    {
        IState state = ChangeState<TState>();
        state.Enter();
    }

    private TState ChangeState<TState>() where TState : class, IState
    {
        _activeState?.Exit();

        TState state = GetState<TState>();
        _activeState = state;

        return state;
    }

    private TState GetState<TState>() where TState : class, IState =>
      _states[typeof(TState)] as TState;

    public Type GetCurrentState() => _activeState.GetType();
}
