using EasyP2P.Infrastructure.Exceptions;
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
    CreatePurchaseOrderRequestApprovalTask,
    CreatePurchaseOrderCreationTask,
    CreatePurchaseOrderApprovalTask,
    SendPurchaseOrder,
    CreateThreeWayMatchTask,
    CreatePaymentApprovalTask,
    PayInvoice,
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

public class TransitionHistory
{
    public Guid Id { get; }
    public State FromState { get; }
    public State ToState { get; }
    public Trigger Trigger { get; }
    public DateTime Timestamp { get; }


    public TransitionHistory(State fromState, State toState, Trigger trigger)
    {
        Id = Guid.NewGuid();
        FromState = fromState;
        ToState = toState;
        Trigger = trigger;
        Timestamp = DateTime.UtcNow;
    }
}

public class StateMachine
{
    private readonly Dictionary<(State, Trigger), Transition> _configuredTransitions = new Dictionary<(State, Trigger), Transition>()
    {
        { (State.NotStarted, Trigger.Start), new Transition(State.NotStarted, State.Executing, Trigger.Start, () => { Console.WriteLine("Starting...");}) },
        { (State.Executing, Trigger.CreatePurchaseOrderRequestApprovalTask), new Transition(State.Executing, State.Hibernated, Trigger.CreatePurchaseOrderRequestApprovalTask, () => { Console.WriteLine("Creating a task for purchase order request approval..."); }) },
        { (State.Hibernated, Trigger.CreatePurchaseOrderCreationTask), new Transition(State.Hibernated, State.Hibernated, Trigger.CreatePurchaseOrderCreationTask, () => { Console.WriteLine("Creating a task for purchase order creation..."); }) },
        { (State.Hibernated, Trigger.CreatePurchaseOrderApprovalTask), new Transition(State.Hibernated, State.Hibernated, Trigger.CreatePurchaseOrderApprovalTask, () => { Console.WriteLine("Creating a purchase order approval task...") ;}) },
        { (State.Hibernated, Trigger.CreateThreeWayMatchTask), new Transition(State.Hibernated, State.Hibernated, Trigger.CreateThreeWayMatchTask, () => { Console.WriteLine("Creating a task for three way matching..."); }) },
        { (State.Hibernated, Trigger.CreatePaymentApprovalTask), new Transition(State.Hibernated, State.Hibernated, Trigger.CreatePaymentApprovalTask, () => { Console.WriteLine("Creating an invoice approval task...");}) },
        { (State.Hibernated, Trigger.PayInvoice), new Transition(State.Hibernated, State.Finished, Trigger.PayInvoice, () => { Console.WriteLine("Paying invoice...");})},
        { (State.Executing, Trigger.Stop), new Transition(State.Executing, State.Finished, Trigger.Stop, () => { Console.WriteLine("Stopping..."); }) }
    };

    private readonly List<TransitionHistory> _transitionHistory;

    public Guid Id { get; private set; }
    public State CurrentState { get; private set; }

    public IReadOnlyList<TransitionHistory> TransitionHistory => _transitionHistory.AsReadOnly();

    public StateMachine(State initialState)
    {
        Id = Guid.NewGuid();
        CurrentState = initialState;
        _transitionHistory = new List<TransitionHistory>();
    }

    public StateMachine(Guid id, State initialState)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        Id = id;
        CurrentState = initialState;
        _transitionHistory = new List<TransitionHistory>();
    }

    public void Fire(Trigger trigger)
    {
        if (!_configuredTransitions.TryGetValue((CurrentState, trigger), out var transition))
        {
            throw new TransitionNotFoundException($"No transition defined for state {CurrentState} and trigger {trigger}");
        }

        State previousState = CurrentState;

        transition.Action?.Invoke();
        CurrentState = transition.Destination;

        var historyEntry = new TransitionHistory(previousState, CurrentState, trigger);
        _transitionHistory.Add(historyEntry);

        Console.WriteLine($"Transitioned from {transition.Source} to {transition.Destination} via {trigger}");
    }
}