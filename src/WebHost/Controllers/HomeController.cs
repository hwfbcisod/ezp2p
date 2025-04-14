using Infrastructure.Sql.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebHost.Controllers;

[Route("[controller]/[action]")]
public class HomeController : Controller
{
    private readonly IStateMachineManager _stateMachineManager;

    public HomeController(IStateMachineManager stateMachineManager)
    {
        _stateMachineManager = stateMachineManager;
    }

    public IActionResult GetData()
    {
        return Ok();
    }

    public async Task<IActionResult> SubmitPurchaseOrderRequest()
    {
        var stateMachine = _stateMachineManager.Create(EasyP2P.Infrastructure.State.NotStarted);
        stateMachine.Fire(EasyP2P.Infrastructure.Trigger.Start);
        stateMachine.Fire(EasyP2P.Infrastructure.Trigger.CreatePurchaseOrderRequestApprovalTask);

        await _stateMachineManager.SaveAsync(stateMachine);

        return Ok(stateMachine.Id);
    }

    public async Task<IActionResult> ApprovePurchaseOrderRequest(Guid processId)
    {
        var stateMachine = await _stateMachineManager.LoadAsync(processId);
        stateMachine.Fire(EasyP2P.Infrastructure.Trigger.CreatePurchaseOrderTask);

        return Ok();
    }

    public async Task<IActionResult> RejectPurchaseOrderRequest(Guid processId)
    {
        var stateMachine = await _stateMachineManager.LoadAsync(processId);
        stateMachine.Fire(EasyP2P.Infrastructure.Trigger.Finish);

        return Ok();
    }

    public async Task<IActionResult> SubmitPurchaseOder(Guid processId)
    {
        var stateMachine = await _stateMachineManager.LoadAsync(processId);
        stateMachine.Fire(EasyP2P.Infrastructure.Trigger.CreateThreeWayMatchTask);

        return Ok();
    }

    public async Task<IActionResult> SubmitThreeWayMatch(Guid processId)
    {
        var stateMachine = await _stateMachineManager.LoadAsync(processId);
        stateMachine.Fire(EasyP2P.Infrastructure.Trigger.CreatePaymentApprovalTask);

        return Ok();
    }

    public async Task<IActionResult> ApprovePayment(Guid processId)
    {
        var stateMachine = await _stateMachineManager.LoadAsync(processId);
        stateMachine.Fire(EasyP2P.Infrastructure.Trigger.ExecutePayment);

        return Ok();
    }
}
