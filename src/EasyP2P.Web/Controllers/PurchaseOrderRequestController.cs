using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyP2P.Web.Controllers;

public class PurchaseOrderRequestController : Controller
{
    private readonly ILogger<PurchaseOrderRequestController> _logger;
    private readonly IPurchaseOrderRequestRepository _purchaseOrderRequestRepository;

    public PurchaseOrderRequestController(
        ILogger<PurchaseOrderRequestController> logger,
        IPurchaseOrderRequestRepository purchaseOrderRequestRepository)
    {
        _logger = logger;
        _purchaseOrderRequestRepository = purchaseOrderRequestRepository;
    }

    public async Task<IActionResult> Index()
    {
        var pendingRequests = await _purchaseOrderRequestRepository.GetByStatusAsync("PendingApproval");
        // Also get "Pending" status for backward compatibility
        var pendingLegacy = await _purchaseOrderRequestRepository.GetByStatusAsync("Pending");

        var allPending = pendingRequests.Concat(pendingLegacy).DistinctBy(x => x.Id);
        return View(allPending);
    }

    public async Task<IActionResult> All()
    {
        var allRequests = await _purchaseOrderRequestRepository.GetAllAsync();
        return View(allRequests);
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await _purchaseOrderRequestRepository.GetByIdAsync(id);
        if (request == null)
        {
            return NotFound();
        }
        return View(request);
    }

    public IActionResult Create()
    {
        ViewBag.Priorities = new List<string> { "Low", "Medium", "High", "Urgent" };
        ViewBag.Departments = new List<string> { "IT", "Finance", "HR", "Operations", "Marketing", "Sales" };
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseOrderRequestInputModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                string currentUser = "CurrentUser"; // Replace with actual user
                var id = await _purchaseOrderRequestRepository.CreateAsync(model, currentUser);

                _logger.LogInformation("New POR created with ID {Id}: Item={ItemName}, Quantity={Quantity}",
                    id, model.ItemName, model.Quantity);

                TempData["SuccessMessage"] = $"Purchase Order Request #{id} created successfully and is pending approval!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order request");
                ModelState.AddModelError("", "An error occurred while creating the request.");
            }
        }

        ViewBag.Priorities = new List<string> { "Low", "Medium", "High", "Urgent" };
        ViewBag.Departments = new List<string> { "IT", "Finance", "HR", "Operations", "Marketing", "Sales" };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            string currentUser = "ApproverUser"; // Replace with actual user

            if (!await _purchaseOrderRequestRepository.CanTransitionToStatus(id, PurchaseOrderRequestState.Approved))
            {
                TempData["ErrorMessage"] = "This request cannot be approved in its current state.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var success = await _purchaseOrderRequestRepository.UpdateStatusAsync(id, PurchaseOrderRequestState.Approved, currentUser);

            if (success)
            {
                _logger.LogInformation("Purchase Order Request {Id} approved by {User}", id, currentUser);
                TempData["SuccessMessage"] = $"Purchase Order Request #{id} has been approved successfully.";
                TempData["CreatePOLink"] = id; // For direct PO creation link
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve the request.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving purchase order request {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while approving the request.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        try
        {
            string currentUser = "ApproverUser"; // Replace with actual user

            if (!await _purchaseOrderRequestRepository.CanTransitionToStatus(id, PurchaseOrderRequestState.Rejected))
            {
                TempData["ErrorMessage"] = "This request cannot be rejected in its current state.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var success = await _purchaseOrderRequestRepository.UpdateStatusAsync(id, PurchaseOrderRequestState.Rejected, currentUser);

            if (success)
            {
                _logger.LogInformation("Purchase Order Request {Id} rejected by {User}", id, currentUser);
                TempData["SuccessMessage"] = $"Purchase Order Request #{id} has been rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject the request.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting purchase order request {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while rejecting the request.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            string currentUser = "CurrentUser"; // Replace with actual user

            if (!await _purchaseOrderRequestRepository.CanTransitionToStatus(id, PurchaseOrderRequestState.Cancelled))
            {
                TempData["ErrorMessage"] = "This request cannot be cancelled in its current state.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var success = await _purchaseOrderRequestRepository.UpdateStatusAsync(id, PurchaseOrderRequestState.Cancelled, currentUser);

            if (success)
            {
                _logger.LogInformation("Purchase Order Request {Id} cancelled by {User}", id, currentUser);
                TempData["SuccessMessage"] = $"Purchase Order Request #{id} has been cancelled.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to cancel the request.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling purchase order request {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while cancelling the request.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // Helper action to add sample data for testing
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSample()
    {
        var sampleRequests = new[]
        {
            new PurchaseOrderRequestInputModel
            {
                ItemName = "Laptop Computer",
                Quantity = 5,
                Comment = "For new employees",
                Justification = "New hires require laptops for daily work",
                Priority = "High",
                Department = "IT",
                BudgetCode = "IT-2025-001",
                ExpectedDeliveryDate = DateTime.Now.AddDays(14)
            },
            new PurchaseOrderRequestInputModel
            {
                ItemName = "Office Supplies",
                Quantity = 100,
                Comment = "Monthly office supplies order",
                Justification = "Regular monthly replenishment of office supplies",
                Priority = "Medium",
                Department = "Operations",
                BudgetCode = "OPS-2025-012",
                ExpectedDeliveryDate = DateTime.Now.AddDays(7)
            }
        };

        foreach (var request in sampleRequests)
        {
            await _purchaseOrderRequestRepository.CreateAsync(request, "SampleUser");
        }

        TempData["SuccessMessage"] = $"Added {sampleRequests.Length} sample purchase order requests.";
        return RedirectToAction(nameof(All));
    }
}