using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyP2P.Web.Controllers;

public class PurchaseController : Controller
{
    private readonly ILogger<PurchaseController> _logger;
    private readonly IPurchaseOrderRequestRepository _purchaseOrderRequestRepository;

    public PurchaseController(ILogger<PurchaseController> logger, IPurchaseOrderRequestRepository purchaseOrderRequestRepository)
    {
        _logger = logger;
        _purchaseOrderRequestRepository = purchaseOrderRequestRepository;
    }

    // GET: Purchase/CreateRequest
    public IActionResult CreateRequest()
    {
        return View();
    }

    // POST: Purchase/CreateRequest
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRequest(PurchaseOrderRequestInputModel model)
    {
        if (ModelState.IsValid)
        {
            await _purchaseOrderRequestRepository.CreateAsync(model, "george");

            _logger.LogInformation($"New POR created: Item={model.ItemName}, Quantity={model.Quantity}");

            // Redirect to a confirmation page or back to home
            TempData["SuccessMessage"] = "Purchase Order Request created successfully!";
            return RedirectToAction("Index", "Home");
        }

        // If we got this far, something failed; redisplay form
        throw new Exception("Something went wrong!");
    }
}