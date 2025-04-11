using Microsoft.AspNetCore.Mvc;

namespace WebHost.Controllers;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult SubmitPurchaseOrderRequest()
    {
        return Ok();
    }

    public IActionResult ApprovePurchaseOrderRequest()
    {
        return Ok();
    }

    public IActionResult RejectPurchaseOrderRequest()
    {
        return Ok();
    }

    public IActionResult SubmitPurchaseOder()
    {
        return Ok();
    }

    public IActionResult SubmitThreeWayMatch()
    {
        return Ok();
    }

    public IActionResult ApprovePayment()
    {
        return Ok();
    }

    public IActionResult SendPayment()
    {
        return Ok();
    }
}
