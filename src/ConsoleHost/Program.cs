using EasyP2P.Infrastructure;
using Infrastructure.Sql;
using System;
using System.Threading.Tasks;

namespace EasyP2P.ConsoleHost;

public class Program
{
    static async Task Main(string[] args)
    {
        var postgresConnection = "";
        var stateMachineRepository = new PostgresStateMachineRepository(postgresConnection);
        var stateMachineManager = new StateMachineManager(stateMachineRepository);
        var stateMachine = stateMachineManager.Create(State.NotStarted);

        stateMachine.Fire(Trigger.Start);
        stateMachine.Fire(Trigger.CreatePurchaseOrderRequestApprovalTask);
        stateMachine.Fire(Trigger.CreatePurchaseOrderTask);
        stateMachine.Fire(Trigger.CreatePurchaseOrderApprovalTask);
        stateMachine.Fire(Trigger.CreateThreeWayMatchTask);
        stateMachine.Fire(Trigger.CreatePaymentApprovalTask);
        stateMachine.Fire(Trigger.ExecutePayment);

        await stateMachineRepository.SaveAsync(stateMachine).ConfigureAwait(false);
        var loadedStateMachine = await stateMachineRepository.LoadAsync(stateMachine.Id).ConfigureAwait(false);
        Console.WriteLine("Loaded state: " + loadedStateMachine.CurrentState);
        loadedStateMachine.Fire(Trigger.Stop);
    }
}
