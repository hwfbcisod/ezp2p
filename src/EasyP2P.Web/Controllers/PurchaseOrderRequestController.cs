using EasyP2P.Web.Attributes;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;
using EasyP2P.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        var allRequests = await _purchaseOrderRequestService.GetFilteredRequestsAsync(); // Your existing method
        ViewBag.UserRole = _userContextService.GetCurrentUserRole(); // Keep existing ViewBag items if needed by other parts of the view
        ViewBag.CanViewAllDepartments = _userContextService.CanViewAllDepartments();

        var filterModel = new FilterViewModel
        {
            CurrentFilterType = FilterType.PurchaseOrderRequest,
            StatusOptions = // Populate from PurchaseOrderRequestState enum or a service
                Enum.GetValues(typeof(PurchaseOrderRequestState))
                    .Cast<PurchaseOrderRequestState>()
                    .Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() })
                    .ToList(),
            PriorityOptions = new List<SelectListItem> { /* ... populate ... */ },
            DepartmentOptions = allRequests.Select(r => r.Department)
                                         .Where(d => !string.IsNullOrEmpty(d))
                                         .Distinct()
                                         .OrderBy(d => d)
                                         .Select(d => new SelectListItem { Value = d, Text = d })
                                         .ToList()
        };
        
        filterModel.StatusOptions.Insert(0, new SelectListItem { Value = "", Text = "All Statuses" });
        filterModel.PriorityOptions.Insert(0, new SelectListItem { Value = "", Text = "All Priorities" });
        filterModel.DepartmentOptions.Insert(0, new SelectListItem { Value = "", Text = "All Departments" });

        ViewData["FilterModel"] = filterModel; // Pass it via ViewData or directly in the main model

        return View(allRequests); // Pass your main data model as usual
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await _purchaseOrderRequestService.GetRequestByIdAsync(id, enforcePermissions: true);
        if (request == null)
        {
            TempData["ErrorMessage"] = "Request not found or you don't have permission to view it.";
            return RedirectToAction(nameof(Index));
        }

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
                    var currentUser = _userContextService.GetCurrentUser();
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
        var request = await _purchaseOrderRequestService.GetRequestByIdAsync(id, enforcePermissions: true);
        if (request == null || !request.CanApprove)
        {
            TempData["ErrorMessage"] = "Cannot approve this request.";
            return RedirectToAction(nameof(Index));
        }

        if (_userContextService.GetCurrentUserRole() == UserRole.Approver)
        {
            var currentDepartment = _userContextService.GetCurrentUserDepartment();
            if (request.Department != currentDepartment)
            {
                TempData["ErrorMessage"] = "You can only approve requests from your department.";
                return RedirectToAction(nameof(Index));
            }
        }

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
            var currentUser = _userContextService.GetCurrentUser();
            var rejectionReason = "Rejected via web interface";

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
            var currentUser = _userContextService.GetCurrentUser();
            var cancellationReason = "Cancelled via web interface"; // Could be from form input

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