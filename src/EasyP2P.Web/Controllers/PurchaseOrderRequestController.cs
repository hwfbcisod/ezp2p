using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Extensions;
using EasyP2P.Web.Models;
using EasyP2P.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyP2P.Web.Controllers;

public class PurchaseOrderRequestController : Controller
{
    private readonly ILogger<PurchaseOrderRequestController> _logger;
    private readonly IPurchaseOrderRequestService _purchaseOrderRequestService;

    public PurchaseOrderRequestController(
        ILogger<PurchaseOrderRequestController> logger,
        IPurchaseOrderRequestService purchaseOrderRequestService)
    {
        _logger = logger;
        _purchaseOrderRequestService = purchaseOrderRequestService;
    }

    public async Task<IActionResult> Index()
    {
        var pendingRequests = await _purchaseOrderRequestService.GetRequestsByStatusAsync(
            Enums.PurchaseOrderRequestState.PendingApproval);

        return View(pendingRequests);
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await _purchaseOrderRequestService.GetRequestByIdAsync(id);
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
            // Validate using service layer business rules
            var validationResult = await _purchaseOrderRequestService.ValidateRequestAsync(model);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                // Add warnings as informational messages
                foreach (var warning in validationResult.Warnings)
                {
                    TempData["WarningMessage"] = warning;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string currentUser = "CurrentUser"; // Replace with actual user
                    var id = await _purchaseOrderRequestService.CreateRequestAsync(model, currentUser);

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

            var success = await _purchaseOrderRequestService.ApproveRequestAsync(id, currentUser);

            if (success)
            {
                TempData["SuccessMessage"] = $"Purchase Order Request #{id} has been approved successfully.";
                TempData["CreatePOLink"] = id; // For direct PO creation link
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve the request. It may not be in the correct state for approval.";
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
            string rejectionReason = "Rejected via web interface"; // Could be from form input

            var success = await _purchaseOrderRequestService.RejectRequestAsync(id, currentUser, rejectionReason);

            if (success)
            {
                TempData["SuccessMessage"] = $"Purchase Order Request #{id} has been rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject the request. It may not be in the correct state for rejection.";
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
            string cancellationReason = "Cancelled via web interface"; // Could be from form input

            var success = await _purchaseOrderRequestService.CancelRequestAsync(id, currentUser, cancellationReason);

            if (success)
            {
                TempData["SuccessMessage"] = $"Purchase Order Request #{id} has been cancelled.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to cancel the request. It may not be in the correct state for cancellation.";
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
            await _purchaseOrderRequestService.CreateRequestAsync(request, "SampleUser");
        }

        TempData["SuccessMessage"] = $"Added {sampleRequests.Length} sample purchase order requests.";
        return RedirectToAction(nameof(Index));
    }

    // Dashboard action utilizing service layer
    public async Task<IActionResult> Dashboard(string? userFilter = null)
    {
        var dashboardData = await _purchaseOrderRequestService.GetDashboardDataAsync(userFilter);

        ViewBag.UserFilter = userFilter;
        return View(dashboardData);
    }
}