using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public interface IState
{
    void OnEnter();
    void Tick();
    void FixedTick();
    IState GetTransition(); // null = stay | non-null = transition to that state
    void OnExit();
} 

public class StateMachine
{
    private readonly Dictionary<Type, IState> _states = new();
    public IState Current { get; private set; }

    public void RegisterState(IState state)
    {
        _states[state.GetType()] = state;
    }

    public T GetState<T>() where T : class, IState
    {
        return _states[typeof(T)] as T;
    }

    public void Init<TState>() where TState : class, IState
    {
        Current = GetState<TState>();
        Current.OnEnter();
    }

    public void Tick()
    {
        var next = Current.GetTransition();
        if (next != null) Transition(next);
        else Current.Tick();
    }

    public void FixedTick() => Current.FixedTick();

    public void ForceTransition<TState>() where TState : class, IState
        => Transition(GetState<TState>());

    public void ForceTransition(IState next) => Transition(next);

    private void Transition(IState next)
    {
        Current.OnExit();
        Current = next;
        Current.OnEnter();
    }
}