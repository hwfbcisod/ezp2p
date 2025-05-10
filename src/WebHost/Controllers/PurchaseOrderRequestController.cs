using Microsoft.AspNetCore.Mvc;
using WebHost.Models;

namespace WebHost.Controllers;

[Route("[controller]")]
public class PurchaseOrderRequestController : Controller
{
    private readonly ILogger<PurchaseOrderRequestController> _logger;

    public PurchaseOrderRequestController(ILogger<PurchaseOrderRequestController> logger)
    {
        _logger = logger;
    }

    // GET: PurchaseOrderRequest
    [Route("")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: PurchaseOrderRequest
    [Route("")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseOrderRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // In a real application, you would save the model to your database here
            // await _requestService.CreateAsync(model);

            // For demonstration, we'll just log it
            _logger.LogInformation($"Purchase order request created for {model.ItemName}, quantity: {model.Quantity}");

            // You could autopopulate the requestor name if using authentication
            // model.RequestorName = User.Identity.Name;

            // Redirect to a confirmation page
            TempData["SuccessMessage"] = "Your request has been submitted successfully!";
            return RedirectToAction("Confirmation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase order request");
            ModelState.AddModelError("", "An error occurred while processing your request.");
            return View(model);
        }
    }

    // GET: PurchaseOrderRequest/Confirm
    [Route("Confirm")]
    public IActionResult Confirm()
    {
        return View();
    }
}