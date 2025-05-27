using EasyP2P.Web.Attributes;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;
using EasyP2P.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyP2P.Web.Controllers;

[Authorize]
public class PurchaseOrderRequestController : Controller
{
    private readonly ILogger<PurchaseOrderRequestController> _logger;
    private readonly IPurchaseOrderRequestService _purchaseOrderRequestService;
    private readonly IUserContextService _userContextService;

    public PurchaseOrderRequestController(
        ILogger<PurchaseOrderRequestController> logger,
        IPurchaseOrderRequestService purchaseOrderRequestService,
        IUserContextService userContextService)
    {
        _logger = logger;
        _purchaseOrderRequestService = purchaseOrderRequestService;
        _userContextService = userContextService;
    }

    public async Task<IActionResult> Index()
    {
        var filteredRequests = await _purchaseOrderRequestService.GetFilteredRequestsAsync();

        // Add user context to ViewBag for conditional UI rendering
        ViewBag.UserRole = _userContextService.GetCurrentUserRole();
        ViewBag.CanViewAllDepartments = _userContextService.CanViewAllDepartments();

        return View(filteredRequests);
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await _purchaseOrderRequestService.GetRequestByIdAsync(id, enforcePermissions: true);
        if (request == null)
        {
            TempData["ErrorMessage"] = "Request not found or you don't have permission to view it.";
            return RedirectToAction(nameof(Index));
        }

        // Set permission flags for the view
        ViewBag.CanApprove = request.CanApprove && _userContextService.HasPermission("ApprovePOR");
        ViewBag.CanReject = request.CanReject && _userContextService.HasPermission("RejectPOR");
        ViewBag.CanCancel = request.CanCancel && (
            _userContextService.HasPermission("CancelOwnPOR") && request.RequestedBy == _userContextService.GetCurrentUser() ||
            _userContextService.GetCurrentUserRole() == UserRole.Administrator
        );

        return View(request);
    }

    [RequiresPermission("CreatePOR")]
    public IActionResult Create()
    {
        ViewBag.Priorities = new List<string> { "Low", "Medium", "High", "Urgent" };
        ViewBag.Departments = new List<string> { "IT", "Finance", "HR", "Operations", "Marketing", "Sales" };
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequiresPermission("CreatePOR")]
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
                    string currentUser = _userContextService.GetCurrentUser();
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
    [RequiresPermission("ApprovePOR")]
    public async Task<IActionResult> Approve(int id)
    {
        // Double-check permission at entity level
        var request = await _purchaseOrderRequestService.GetRequestByIdAsync(id, enforcePermissions: true);
        if (request == null || !request.CanApprove)
        {
            TempData["ErrorMessage"] = "Cannot approve this request.";
            return RedirectToAction(nameof(Index));
        }

        // Check department-level permissions for approvers
        if (_userContextService.GetCurrentUserRole() == UserRole.Approver)
        {
            var currentDepartment = _userContextService.GetCurrentUserDepartment();
            if (request.Department != currentDepartment)
            {
                TempData["ErrorMessage"] = "You can only approve requests from your department.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Proceed with approval
        var success = await _purchaseOrderRequestService.ApproveRequestAsync(id, _userContextService.GetCurrentUser());

        if (success)
        {
            TempData["SuccessMessage"] = $"Purchase Order Request #{id} has been approved successfully.";
            TempData["CreatePOLink"] = id;
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to approve the request.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequiresPermission("RejectPOR")]
    public async Task<IActionResult> Reject(int id)
    {
        try
        {
            string currentUser = _userContextService.GetCurrentUser();
            string rejectionReason = "Rejected via web interface";

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
    [AllowAnonymous]
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

    public async Task<IActionResult> Dashboard(string? userFilter = null)
    {
        var dashboardData = await _purchaseOrderRequestService.GetDashboardDataAsync(userFilter);

        ViewBag.UserFilter = userFilter;
        return View(dashboardData);
    }
}