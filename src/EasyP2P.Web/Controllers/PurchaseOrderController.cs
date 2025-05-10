// Controllers/PurchaseOrderController.cs
using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyP2P.Web.Controllers;

public class PurchaseOrderController : Controller
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IPurchaseOrderRequestRepository _purchaseOrderRequestRepository;
    private readonly ILogger<PurchaseOrderController> _logger;

    public PurchaseOrderController(
        IPurchaseOrderRepository purchaseOrderRepository,
        IPurchaseOrderRequestRepository purchaseOrderRequestRepository,
        ILogger<PurchaseOrderController> logger)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseOrderRequestRepository = purchaseOrderRequestRepository;
        _logger = logger;
    }

    // GET: PurchaseOrder/Index
    public async Task<IActionResult> Index()
    {
        var orders = await _purchaseOrderRepository.GetAllAsync();
        return View(orders);
    }

    // GET: PurchaseOrder/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _purchaseOrderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    // GET: PurchaseOrder/Create/5 (where 5 is the purchase order request ID)
    public async Task<IActionResult> Create(int id)
    {
        // Get the purchase order request to prefill the form
        var request = await _purchaseOrderRequestRepository.GetByIdAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        // Check if the request is already approved
        if (request.Status != "Approved")
        {
            TempData["ErrorMessage"] = "Only approved purchase order requests can be converted to purchase orders.";
            return RedirectToAction("Index", "PurchaseApproval");
        }

        // Check if a purchase order already exists for this request
        var existingOrders = await _purchaseOrderRepository.GetByRequestIdAsync(id);
        if (existingOrders.Any())
        {
            TempData["ErrorMessage"] = "A purchase order has already been created for this request.";
            return RedirectToAction("Index");
        }

        // Create a new model pre-filled with the request data
        var model = new PurchaseOrderModel
        {
            PurchaseOrderRequestId = request.Id,
            ItemName = request.ItemName,
            Quantity = request.Quantity,
            UnitPrice = 0, // User needs to fill this
            TotalPrice = 0, // Will be calculated
            Supplier = "" // User needs to fill this
        };

        return View(model);
    }

    // POST: PurchaseOrder/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseOrderModel model)
    {
        if (ModelState.IsValid)
        {
            // Ensure the total price is correct
            model.TotalPrice = model.Quantity * model.UnitPrice;

            try
            {
                // In a real application, get the current user's username
                string currentUser = "CurrentUser"; // Replace with actual user name

                // Create the purchase order
                int orderId = await _purchaseOrderRepository.CreateAsync(model, currentUser);

                _logger.LogInformation("Purchase order created with ID: {OrderId}", orderId);
                TempData["SuccessMessage"] = "Purchase order created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order");
                ModelState.AddModelError("", "An error occurred while creating the purchase order.");
            }
        }

        return View(model);
    }

    [HttpPost, ActionName("Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelConfirmed(int id)
    {
        try
        {
            var result = await _purchaseOrderRepository.UpdateStatusAsync(id, "Cancelled");
            if (result)
            {
                _logger.LogInformation("Purchase order with ID {OrderId} has been cancelled", id);
                TempData["SuccessMessage"] = "Purchase order cancelled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Purchase order not found or could not be cancelled.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling purchase order with ID {OrderId}", id);
            TempData["ErrorMessage"] = "An error occurred while cancelling the purchase order.";
        }

        return RedirectToAction(nameof(Index));
    }

    // AJAX action to calculate the total price
    [HttpPost]
    public IActionResult CalculateTotalPrice(int quantity, decimal unitPrice)
    {
        decimal totalPrice = quantity * unitPrice;
        return Json(new { totalPrice = totalPrice.ToString("C") });
    }
}