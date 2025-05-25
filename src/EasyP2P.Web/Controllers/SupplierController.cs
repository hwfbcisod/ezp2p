using EasyP2P.Web.Models;
using EasyP2P.Web.Services;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Controllers;

public class SupplierController : Controller
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SupplierController> _logger;

    public SupplierController(
        ISupplierService supplierService,
        ILogger<SupplierController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    // GET: Supplier/Index
    public async Task<IActionResult> Index(string? status = null, string? search = null, string? location = null)
    {
        try
        {
            IEnumerable<SupplierViewModel> suppliers;

            // Apply filters based on query parameters
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<SupplierStatus>(status, out var supplierStatus))
            {
                suppliers = await _supplierService.GetSuppliersByStatusAsync(supplierStatus);
            }
            else if (!string.IsNullOrEmpty(search))
            {
                suppliers = await _supplierService.SearchSuppliersAsync(search);
            }
            else if (!string.IsNullOrEmpty(location))
            {
                // Parse location filter (could be city, state, or country)
                suppliers = await _supplierService.GetSuppliersByLocationAsync(location, location, location);
            }
            else
            {
                suppliers = await _supplierService.GetAllSuppliersAsync();
            }

            // Pass filter values to view for maintaining state
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentLocation = location;

            // Get statistics for dashboard cards
            ViewBag.Statistics = await _supplierService.GetSupplierStatisticsAsync();

            return View(suppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading suppliers index page");
            TempData["ErrorMessage"] = "An error occurred while loading suppliers.";
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

    // GET: Supplier/Create
    public IActionResult Create()
    {
        ViewBag.Statuses = GetStatusSelectList();
        ViewBag.PaymentTerms = GetPaymentTermsSelectList();
        ViewBag.Countries = GetCountrySelectList();

        var model = new SupplierInputModel
        {
            Status = "Active",
            Country = "USA"
        };

        return View(model);
    }

    // POST: Supplier/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierInputModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                string currentUser = "CurrentUser"; // Replace with actual user

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

        // If we got this far, something failed, redisplay form
        ViewBag.Statuses = GetStatusSelectList();
        ViewBag.PaymentTerms = GetPaymentTermsSelectList();
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
        ViewBag.PaymentTerms = GetPaymentTermsSelectList();
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
                string currentUser = "CurrentUser"; // Replace with actual user

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

        // If we got this far, something failed, redisplay form
        ViewBag.Statuses = GetStatusSelectList();
        ViewBag.PaymentTerms = GetPaymentTermsSelectList();
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
            string currentUser = "CurrentUser"; // Replace with actual user

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
            string currentUser = "CurrentUser"; // Replace with actual user

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
            string currentUser = "CurrentUser"; // Replace with actual user

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
            string currentUser = "CurrentUser"; // Replace with actual user

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

    // GET: Supplier/TopRated
    public async Task<IActionResult> TopRated(int minRating = 4)
    {
        try
        {
            var suppliers = await _supplierService.GetTopRatedSuppliersAsync(minRating);
            ViewBag.MinRating = minRating;
            return View("Index", suppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading top-rated suppliers");
            TempData["ErrorMessage"] = "An error occurred while loading top-rated suppliers.";
            return RedirectToAction(nameof(Index));
        }
    }

    // AJAX endpoint for supplier search
    [HttpGet]
    public async Task<IActionResult> SearchSuppliers(string term)
    {
        try
        {
            var suppliers = await _supplierService.SearchSuppliersAsync(term);
            var results = suppliers.Select(s => new
            {
                id = s.Id,
                name = s.Name,
                contactPerson = s.ContactPerson,
                email = s.Email,
                status = s.Status,
                rating = s.Rating
            });

            return Json(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during supplier search");
            return Json(new { error = "Search failed" });
        }
    }

    // Helper action to get active suppliers for dropdowns in other parts of the system
    [HttpGet]
    public async Task<IActionResult> GetActiveSuppliers()
    {
        try
        {
            var suppliers = await _supplierService.GetActiveSuppliersAsync();
            var results = suppliers.Select(s => new
            {
                id = s.Id,
                name = s.Name,
                paymentTerms = s.PaymentTerms
            });

            return Json(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active suppliers");
            return Json(new { error = "Failed to retrieve suppliers" });
        }
    }

    // Helper methods for dropdown lists
    private List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetStatusSelectList()
    {
        return new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new() { Value = "Active", Text = "Active" },
            new() { Value = "Inactive", Text = "Inactive" },
            new() { Value = "Pending", Text = "Pending" },
            new() { Value = "Suspended", Text = "Suspended" }
        };
    }

    private List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetPaymentTermsSelectList()
    {
        return new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new() { Value = "", Text = "Select Payment Terms" },
            new() { Value = "Net 15", Text = "Net 15" },
            new() { Value = "Net 30", Text = "Net 30" },
            new() { Value = "Net 45", Text = "Net 45" },
            new() { Value = "Net 60", Text = "Net 60" },
            new() { Value = "Net 90", Text = "Net 90" },
            new() { Value = "Due on Receipt", Text = "Due on Receipt" },
            new() { Value = "2/10 Net 30", Text = "2/10 Net 30" },
            new() { Value = "1/15 Net 30", Text = "1/15 Net 30" }
        };
    }

    private List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetCountrySelectList()
    {
        return new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
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
            new() { Value = "Other", Text = "Other" }
        };
    }
}