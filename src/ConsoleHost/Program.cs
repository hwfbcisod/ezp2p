using EasyP2P.Infrastructure;
using System;

namespace EasyP2P.ConsoleHost;

public class Program
{
    static void Main(string[] args)
    {
        var stateMachine = new StateMachine(State.NotStarted);
        stateMachine.Fire(Trigger.Start);
        stateMachine.Fire(Trigger.CreatePurchaseOrderRequestApprovalTask);
        stateMachine.Fire(Trigger.CreatePurchaseOrderCreationTask);
        stateMachine.Fire(Trigger.CreatePurchaseOrderApprovalTask);
        stateMachine.Fire(Trigger.CreateThreeWayMatchTask);
        stateMachine.Fire(Trigger.CreateInvoiceApprovalTask);
        stateMachine.Fire(Trigger.PayInvoice);
    }
}
