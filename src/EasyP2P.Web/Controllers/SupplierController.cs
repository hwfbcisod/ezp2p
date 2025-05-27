using EasyP2P.Web.Models;
using EasyP2P.Web.Services;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using EasyP2P.Web.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyP2P.Web.Controllers;

[Authorize]
public class SupplierController : Controller
{
    private readonly ISupplierService _supplierService;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<SupplierController> _logger;

    public SupplierController(
        ISupplierService supplierService,
        IUserContextService userContextService,
        ILogger<SupplierController> logger)
    {
        _supplierService = supplierService;
        _userContextService = userContextService;
        _logger = logger;
    }

    [RequiresPermission("ViewAllSuppliers")]
    public async Task<IActionResult> Index(string? status = null, string? search = null, string? location = null)
    {
        try
        {
            IEnumerable<SupplierViewModel> suppliers;

            // Apply filters based on query parameters (this part remains for initial load if needed)
            // However, the primary filtering will now be client-side after initial load.
            // For a fully server-side filtered approach with this new model, you'd pass the FilterViewModel here.
            // For now, let's assume initial load gets all relevant suppliers based on broad filters.
            suppliers = await _supplierService.GetAllSuppliersAsync();

            // Pass filter values to view for maintaining state if needed for other purposes
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentLocation = location;

            // Get statistics for dashboard cards
            ViewBag.Statistics = await _supplierService.GetSupplierStatisticsAsync();

            // Prepare the FilterViewModel
            var filterModel = new FilterViewModel
            {
                CurrentFilterType = FilterType.Supplier,
                StatusOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "All Statuses" },
                    new SelectListItem { Value = "Active", Text = "Active" },
                    new SelectListItem { Value = "Inactive", Text = "Inactive" },
                    new SelectListItem { Value = "Suspended", Text = "Suspended" }
                },
                RatingOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "All Ratings" },
                    new SelectListItem { Value = "5", Text = "5 Stars" },
                    new SelectListItem { Value = "4", Text = "4+ Stars" },
                    new SelectListItem { Value = "3", Text = "3+ Stars" },
                    new SelectListItem { Value = "2", Text = "2+ Stars" },
                    new SelectListItem { Value = "1", Text = "1+ Star" }
                },
                Status = status,
                SearchTerm = search, // Assuming 'search' from query maps to generic SearchTerm for suppliers
                Location = location,
                MinRating = 0
            };

            ViewData["FilterModel"] = filterModel; // Pass the filter model to the view

            return View(suppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading suppliers index page");
            TempData["ErrorMessage"] = "An error occurred while loading suppliers.";
            // Still pass an empty filter model in case of error to prevent view from breaking
            ViewData["FilterModel"] = new FilterViewModel { CurrentFilterType = FilterType.Supplier };
            return View(Enumerable.Empty<SupplierViewModel>());
        }
    }

    // GET: Supplier/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(id);
        if (supplier == null)
        {
            TempData["ErrorMessage"] = "Supplier not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(supplier);
    }

    [RequiresPermission("CreateSupplier")]
    public IActionResult Create()
    {
        ViewBag.Statuses = GetStatusSelectList();
        ViewBag.Countries = GetCountrySelectList();

        var model = new SupplierInputModel
        {
            Status = "Active",
            Country = "USA"
        };

        return View(model);
    }

    [RequiresPermission("CreateSupplier")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierInputModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var currentUser = _userContextService.GetCurrentUser();

                var supplierId = await _supplierService.CreateSupplierAsync(model, currentUser);

                _logger.LogInformation("New supplier created with ID {Id}: {SupplierName}",
                    supplierId, model.Name);

                TempData["SuccessMessage"] = $"Supplier '{model.Name}' created successfully!";
                return RedirectToAction(nameof(Details), new { id = supplierId });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier {SupplierName}", model.Name);
                ModelState.AddModelError("", "An error occurred while creating the supplier.");
            }
        }

        ViewBag.Statuses = GetStatusSelectList();
        ViewBag.Countries = GetCountrySelectList();
        return View(model);
    }

    // GET: Supplier/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(id);
        if (supplier == null)
        {
            TempData["ErrorMessage"] = "Supplier not found.";
            return RedirectToAction(nameof(Index));
        }

        if (!supplier.CanEdit)
        {
            TempData["ErrorMessage"] = "This supplier cannot be edited due to its current status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewBag.Statuses = GetStatusSelectList();
        ViewBag.Countries = GetCountrySelectList();

        var inputModel = supplier.ToInputModel();
        return View(inputModel);
    }

    // POST: Supplier/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SupplierInputModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var currentUser = _userContextService.GetCurrentUser();
                var success = await _supplierService.UpdateSupplierAsync(id, model, currentUser);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Supplier '{model.Name}' updated successfully!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update the supplier. Please try again.";
                }
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier {Id}", id);
                ModelState.AddModelError("", "An error occurred while updating the supplier.");
            }
        }

        ViewBag.Statuses = GetStatusSelectList();
        ViewBag.Countries = GetCountrySelectList();
        return View(model);
    }

    // POST: Supplier/Activate/5
    [HttpPost, ActionName("Activate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateConfirmed(int id)
    {
        try
        {
            string currentUser = _userContextService.GetCurrentUser();

            var success = await _supplierService.UpdateSupplierStatusAsync(id, SupplierStatus.Active, currentUser);

            if (success)
            {
                TempData["SuccessMessage"] = "Supplier activated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to activate the supplier.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating supplier {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while activating the supplier.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Supplier/Deactivate/5
    [HttpPost, ActionName("Deactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateConfirmed(int id)
    {
        try
        {
            string currentUser = _userContextService.GetCurrentUser();

            var success = await _supplierService.UpdateSupplierStatusAsync(id, SupplierStatus.Inactive, currentUser);

            if (success)
            {
                TempData["SuccessMessage"] = "Supplier deactivated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to deactivate the supplier.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating supplier {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while deactivating the supplier.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Supplier/Suspend/5
    [HttpPost, ActionName("Suspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendConfirmed(int id)
    {
        try
        {
            string currentUser = _userContextService.GetCurrentUser(); // Replace with actual user

            var success = await _supplierService.UpdateSupplierStatusAsync(id, SupplierStatus.Suspended, currentUser);

            if (success)
            {
                TempData["SuccessMessage"] = "Supplier suspended successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to suspend the supplier.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending supplier {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while suspending the supplier.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Supplier/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(id);
        if (supplier == null)
        {
            TempData["ErrorMessage"] = "Supplier not found.";
            return RedirectToAction(nameof(Index));
        }

        if (!supplier.CanDelete)
        {
            TempData["ErrorMessage"] = "This supplier cannot be deleted due to its current status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(supplier);
    }

    // POST: Supplier/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            string currentUser = _userContextService.GetCurrentUser();

            var success = await _supplierService.DeleteSupplierAsync(id, currentUser);

            if (success)
            {
                TempData["SuccessMessage"] = "Supplier deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete the supplier. It may be referenced by existing purchase orders.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the supplier.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private List<SelectListItem> GetStatusSelectList()
    {
        return new List<SelectListItem>
        {
            new() { Value = "Active", Text = "Active" },
            new() { Value = "Inactive", Text = "Inactive" },
            new() { Value = "Pending", Text = "Pending" },
            new() { Value = "Suspended", Text = "Suspended" }
        };
    }

    private List<SelectListItem> GetCountrySelectList()
    {
        return new List<SelectListItem>
        {
            new() { Value = "", Text = "Select Country" },
            new() { Value = "USA", Text = "United States" },
            new() { Value = "Canada", Text = "Canada" },
            new() { Value = "Mexico", Text = "Mexico" },
            new() { Value = "United Kingdom", Text = "United Kingdom" },
            new() { Value = "Germany", Text = "Germany" },
            new() { Value = "France", Text = "France" },
            new() { Value = "Italy", Text = "Italy" },
            new() { Value = "Spain", Text = "Spain" },
            new() { Value = "Netherlands", Text = "Netherlands" },      
            new() { Value = "Japan", Text = "Japan" },
            new() { Value = "China", Text = "China" },
            new() { Value = "India", Text = "India" },
            new() { Value = "Australia", Text = "Australia" },          
            new() { Value = "Bulgaria", Text = "Bulgaria"},
            new() { Value = "Other", Text = "Other" }
        };
    }
}