using EasyP2P.Infrastructure;
using System;

namespace EasyP2P.ConsoleHost;

public class Program
{
    static void Main(string[] args)
    {
        var stateMachine = new StateMachine(State.NotStarted);
        stateMachine.ConfigureTransition(State.NotStarted, State.Executing, Trigger.Start, () => Console.WriteLine("Doing WORK!"));
        stateMachine.Fire(Trigger.Start);
    }
}
