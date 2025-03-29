using System;
using System.Collections.Generic;

namespace EasyP2P.Infrastructure;

public enum State
{
    NotStarted = 0,
    Executing,
    Hibernated,
    Finished
}

public enum Trigger
{
    Start,
    Resume,
    Stop,
    Finish,
    Fail
}

public class Transition
{
    public State Source { get; }
    public State Destination { get; }
    public Trigger Trigger { get; }
    public Action Action { get; }

    public Transition(State source, State destination, Trigger trigger, Action action)
    {
        Source = source;
        Destination = destination;
        Trigger = trigger;
        Action = action;
    }
}

public class StateMachine
{
    private State _currentState;
    private readonly Dictionary<(State, Trigger), Transition> _transitions;

    public State CurrentState => _currentState;

    public StateMachine(State initialState)
    {
        _currentState = initialState;
        _transitions = new Dictionary<(State, Trigger), Transition>();
    }

    public void ConfigureTransition(State source, State destination, Trigger trigger, Action action)
    {
        var transition = new Transition(source, destination, trigger, action);
        _transitions[(source, trigger)] = transition;
    }

    public void Fire(Trigger trigger)
    {
        if (!_transitions.TryGetValue((_currentState, trigger), out var transition))
        {
            throw new Exception($"No transition defined for state {_currentState} and trigger {trigger}");
        }

        transition.Action?.Invoke();
        _currentState = transition.Destination;
        Console.WriteLine($"Transitioned from {transition.Source} to {transition.Destination} via {trigger}");
    }
}