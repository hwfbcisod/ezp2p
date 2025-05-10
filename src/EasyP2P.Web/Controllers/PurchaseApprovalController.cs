using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyP2P.Web.Controllers;

public class PurchaseApprovalController : Controller
{
    private readonly ILogger<PurchaseApprovalController> _logger;
    private readonly IPurchaseOrderRequestRepository _purchaseOrderRequestRepository;

    public PurchaseApprovalController(ILogger<PurchaseApprovalController> logger, IPurchaseOrderRequestRepository purchaseOrderRequestRepository)
    {
        _logger = logger;
        _purchaseOrderRequestRepository = purchaseOrderRequestRepository;
    }

    // GET: PurchaseApproval/Index
    public async Task<IActionResult> Index()
    {
        var pendingRequests = await _purchaseOrderRequestRepository.GetByStatusAsync("Pending");
        var viewModels = pendingRequests.Select(x => new PurchaseOrderRequestViewModel
        {
            Id = x.Id,
            ItemName = x.ItemName,
            Quantity = x.Quantity,
            Comment = x.Comment,
            RequestDate = x.RequestDate,
            RequestedBy = x.RequestedBy,
            Status = x.Status
        }).ToList();

        return View(viewModels);
    }

    // GET: PurchaseApproval/All
    public async Task<IActionResult> All()
    {
        var allRequests = await _purchaseOrderRequestRepository.GetAllAsync();
        return View(allRequests);
    }

    // POST: PurchaseApproval/Approve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var pendingRequests = await _purchaseOrderRequestRepository.GetByStatusAsync("Pending");
        var request = pendingRequests.FirstOrDefault(r => r.Id == id);

        if (request != null)
        {
            await _purchaseOrderRequestRepository.UpdateStatusAsync(id, "Approved");

            _logger.LogInformation($"Approved POR with ID: {id}");
            TempData["SuccessMessage"] = $"Purchase Order Request #{id} has been approved.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Purchase Order Request #{id} not found.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: PurchaseApproval/Reject/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var pendingRequests = await _purchaseOrderRequestRepository.GetByStatusAsync("Pending");
        var request = pendingRequests.FirstOrDefault(r => r.Id == id);

        if (request != null)
        {
            await _purchaseOrderRequestRepository.UpdateStatusAsync(id, "Rejected");

            _logger.LogInformation($"Rejected POR with ID: {id}");
            TempData["SuccessMessage"] = $"Purchase Order Request #{id} has been rejected.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Purchase Order Request #{id} not found.";
        }

        return RedirectToAction(nameof(Index));
    }
}