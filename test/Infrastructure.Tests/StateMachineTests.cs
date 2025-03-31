using System;
using System.IO;
using EasyP2P.Infrastructure;

namespace EasyP2P.Tests.Infrastructure;

public class StateMachineTests
{
    [Fact]
    public void InitialState_ShouldBeNotStarted()
    {
        var stateMachine = new StateMachine(State.NotStarted);

        Assert.Equal(State.NotStarted, stateMachine.CurrentState);
    }

    [Fact]
    public void Fire_Start_ShouldTransitionFromNotStartedToExecuting()
    {
        var stateMachine = new StateMachine(State.NotStarted);
        stateMachine.ConfigureTransition(State.NotStarted, State.Executing, Trigger.Start, () => { });

        stateMachine.Fire(Trigger.Start);

        Assert.Equal(State.Executing, stateMachine.CurrentState);
    }

    [Fact]
    public void Fire_Stop_ShouldTransitionFromExecutingToHibernated()
    {
        var stateMachine = new StateMachine(State.Executing);
        stateMachine.ConfigureTransition(State.Executing, State.Hibernated, Trigger.Stop, () => { });

        stateMachine.Fire(Trigger.Stop);

        Assert.Equal(State.Hibernated, stateMachine.CurrentState);
    }

    [Fact]
    public void Fire_Resume_ShouldTransitionFromHibernatedToExecuting()
    {
        var stateMachine = new StateMachine(State.Hibernated);
        stateMachine.ConfigureTransition(State.Hibernated, State.Executing, Trigger.Resume, () => { });

        stateMachine.Fire(Trigger.Resume);

        Assert.Equal(State.Executing, stateMachine.CurrentState);
    }

    [Fact]
    public void Fire_Finish_ShouldTransitionFromExecutingToFinished()
    {
        var stateMachine = new StateMachine(State.Executing);
        stateMachine.ConfigureTransition(State.Executing, State.Finished, Trigger.Finish, () => { });

        stateMachine.Fire(Trigger.Finish);

        Assert.Equal(State.Finished, stateMachine.CurrentState);
    }

    [Fact]
    public void Fire_Fail_ShouldTransitionFromExecutingToFinished()
    {
        var stateMachine = new StateMachine(State.Executing);
        stateMachine.ConfigureTransition(State.Executing, State.Finished, Trigger.Fail, () => { });

        stateMachine.Fire(Trigger.Fail);

        Assert.Equal(State.Finished, stateMachine.CurrentState);
    }

    [Fact]
    public void Fire_InvalidTrigger_ShouldThrow()
    {
        var stateMachine = new StateMachine(State.Finished);

        Assert.Throws<Exception>(() => stateMachine.Fire(Trigger.Start));
    }

    [Fact]
    public void TransitionAction_ShouldBeExecuted()
    {
        var stateMachine = new StateMachine(State.NotStarted);

        bool actionExecuted = false;
        stateMachine.ConfigureTransition(
            State.NotStarted,
            State.Executing,
            Trigger.Start,
            () => actionExecuted = true
        );

        stateMachine.Fire(Trigger.Start);

        Assert.True(actionExecuted);
    }

    [Fact]
    public void CompleteStateMachineFlow_VerifyAllTransitions()
    {
        var stateMachine = new StateMachine(State.NotStarted);

        stateMachine.ConfigureTransition(State.NotStarted, State.Executing, Trigger.Start, () => { });
        stateMachine.ConfigureTransition(State.Executing, State.Hibernated, Trigger.Stop, () => { });
        stateMachine.ConfigureTransition(State.Hibernated, State.Executing, Trigger.Resume, () => { });
        stateMachine.ConfigureTransition(State.Executing, State.Finished, Trigger.Finish, () => { });

        Assert.Equal(State.NotStarted, stateMachine.CurrentState);

        stateMachine.Fire(Trigger.Start);
        Assert.Equal(State.Executing, stateMachine.CurrentState);

        stateMachine.Fire(Trigger.Stop);
        Assert.Equal(State.Hibernated, stateMachine.CurrentState);

        stateMachine.Fire(Trigger.Resume);
        Assert.Equal(State.Executing, stateMachine.CurrentState);

        stateMachine.Fire(Trigger.Finish);
        Assert.Equal(State.Finished, stateMachine.CurrentState);
    }

    [Fact]
    public void Action_ShouldWriteToConsole()
    {
        var stateMachine = new StateMachine(State.NotStarted);

        using (StringWriter sw = new StringWriter())
        {
            Console.SetOut(sw);

            stateMachine.ConfigureTransition(
                State.NotStarted,
                State.Executing,
                Trigger.Start,
                () => Console.WriteLine("Starting execution")
            );

            stateMachine.Fire(Trigger.Start);

            string output = sw.ToString();
            Assert.Contains("Starting execution", output);
        }

        var standardOut = new StreamWriter(Console.OpenStandardOutput());
        standardOut.AutoFlush = true;
        Console.SetOut(standardOut);
    }
}