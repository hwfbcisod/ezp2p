using EasyP2P.Web.Models;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Data.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;
using EasyP2P.Web.Extensions;

namespace EasyP2P.Web.Services;

/// <summary>
/// Service layer for Supplier business logic and validation.
/// </summary>
public interface ISupplierService
{
    Task<SupplierViewModel?> GetSupplierByIdAsync(int id);
    Task<IEnumerable<SupplierViewModel>> GetAllSuppliersAsync();
    Task<IEnumerable<SupplierViewModel>> GetSuppliersByStatusAsync(SupplierStatus status);
    Task<IEnumerable<SupplierViewModel>> SearchSuppliersAsync(string searchTerm);
    Task<IEnumerable<SupplierViewModel>> GetSuppliersByLocationAsync(string? city = null, string? state = null, string? country = null);
    Task<IEnumerable<SupplierViewModel>> GetTopRatedSuppliersAsync(int minRating = 4);
    Task<SupplierStatistics> GetSupplierStatisticsAsync();
    Task<ValidationResult> ValidateSupplierAsync(SupplierInputModel model, int? excludeId = null);
    Task<int> CreateSupplierAsync(SupplierInputModel model, string createdBy);
    Task<bool> UpdateSupplierAsync(int id, SupplierInputModel model, string updatedBy);
    Task<bool> UpdateSupplierStatusAsync(int id, SupplierStatus status, string updatedBy);
    Task<bool> DeleteSupplierAsync(int id, string deletedBy);
    Task<bool> CanDeleteSupplierAsync(int id);
    Task<IEnumerable<SupplierViewModel>> GetActiveSuppliersAsync();
}

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repository;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        ISupplierRepository repository,
        IUserContextService userContextService,
        ILogger<SupplierService> logger)
    {
        _repository = repository;
        _userContextService = userContextService;
        _logger = logger;
    }

    public async Task<SupplierViewModel?> GetSupplierByIdAsync(int id)
    {
        try
        {
            var dbModel = await _repository.GetByIdAsync(id);
            return dbModel?.ToViewModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<SupplierViewModel>> GetAllSuppliersAsync()
    {
        var allSuppliers = await _repository.GetAllAsync();
        var viewModels = allSuppliers.ToViewModels();

        return FilterSuppliersByUserRole(viewModels);
    }

    public async Task<IEnumerable<SupplierViewModel>> GetSuppliersByStatusAsync(SupplierStatus status)
    {
        try
        {
            var dbModels = await _repository.GetByStatusAsync(status);
            return dbModels.ToViewModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers with status {Status}", status);
            return Enumerable.Empty<SupplierViewModel>();
        }
    }

    public async Task<IEnumerable<SupplierViewModel>> SearchSuppliersAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllSuppliersAsync();

            var dbModels = await _repository.SearchByNameAsync(searchTerm);
            return dbModels.ToViewModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching suppliers with term {SearchTerm}", searchTerm);
            return Enumerable.Empty<SupplierViewModel>();
        }
    }

    public async Task<IEnumerable<SupplierViewModel>> GetSuppliersByLocationAsync(string? city = null, string? state = null, string? country = null)
    {
        try
        {
            var dbModels = await _repository.GetByLocationAsync(city, state, country);
            return dbModels.ToViewModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers by location");
            return Enumerable.Empty<SupplierViewModel>();
        }
    }

    public async Task<IEnumerable<SupplierViewModel>> GetTopRatedSuppliersAsync(int minRating = 4)
    {
        try
        {
            var dbModels = await _repository.GetTopRatedSuppliersAsync(minRating);
            return dbModels.ToViewModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top-rated suppliers");
            return Enumerable.Empty<SupplierViewModel>();
        }
    }

    public async Task<SupplierStatistics> GetSupplierStatisticsAsync()
    {
        try
        {
            return await _repository.GetStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier statistics");
            return new SupplierStatistics();
        }
    }

    public async Task<IEnumerable<SupplierViewModel>> GetActiveSuppliersAsync()
    {
        return await GetSuppliersByStatusAsync(SupplierStatus.Active);
    }

    public async Task<int> CreateSupplierAsync(SupplierInputModel model, string createdBy)
    {
        try
        {
            _logger.LogInformation("Creating supplier {SupplierName} by {User}", model.Name, createdBy);

            // Validate the supplier using business rules
            var validationResult = await ValidateSupplierAsync(model);
            if (!validationResult.IsValid)
            {
                var errorMessage = $"Validation failed: {string.Join(", ", validationResult.Errors)}";
                _logger.LogWarning("Validation failed for supplier creation: {Errors}", errorMessage);
                throw new ValidationException(errorMessage);
            }

            // Log warnings but don't block creation
            if (validationResult.Warnings.Any())
            {
                _logger.LogWarning("Supplier creation has warnings: {Warnings}",
                    string.Join(", ", validationResult.Warnings));
            }

            // Create the supplier through repository
            var id = await _repository.CreateAsync(model, createdBy);

            _logger.LogInformation("Supplier created successfully with ID {Id} by {User}", id, createdBy);

            return id;
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier {SupplierName} by {User}", model.Name, createdBy);
            throw new InvalidOperationException("Failed to create supplier", ex);
        }
    }

    public async Task<bool> UpdateSupplierAsync(int id, SupplierInputModel model, string updatedBy)
    {
        try
        {
            _logger.LogInformation("Updating supplier {Id} by {User}", id, updatedBy);

            // Validate the supplier using business rules
            var validationResult = await ValidateSupplierAsync(model, id);
            if (!validationResult.IsValid)
            {
                var errorMessage = $"Validation failed: {string.Join(", ", validationResult.Errors)}";
                _logger.LogWarning("Validation failed for supplier update: {Errors}", errorMessage);
                throw new ValidationException(errorMessage);
            }

            // Log warnings but don't block update
            if (validationResult.Warnings.Any())
            {
                _logger.LogWarning("Supplier update has warnings: {Warnings}",
                    string.Join(", ", validationResult.Warnings));
            }

            var result = await _repository.UpdateAsync(id, model, updatedBy);

            if (result)
            {
                _logger.LogInformation("Supplier {Id} updated successfully by {User}", id, updatedBy);
            }

            return result;
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier {Id}", id);
            return false;
        }
    }

    public async Task<bool> UpdateSupplierStatusAsync(int id, SupplierStatus status, string updatedBy)
    {
        try
        {
            var result = await _repository.UpdateStatusAsync(id, status, updatedBy);

            if (result)
            {
                _logger.LogInformation("Supplier {Id} status updated to {Status} by {User}", id, status, updatedBy);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier {Id} status", id);
            return false;
        }
    }

    public async Task<bool> DeleteSupplierAsync(int id, string deletedBy)
    {
        try
        {
            // Check if supplier can be deleted
            if (!await CanDeleteSupplierAsync(id))
            {
                _logger.LogWarning("Cannot delete supplier {Id} - business rules prevent deletion", id);
                return false;
            }

            var result = await _repository.DeleteAsync(id);

            if (result)
            {
                _logger.LogInformation("Supplier {Id} deleted by {User}", id, deletedBy);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier {Id}", id);
            return false;
        }
    }

    public async Task<bool> CanDeleteSupplierAsync(int id)
    {
        try
        {
            var supplier = await GetSupplierByIdAsync(id);
            if (supplier == null) return false;

            // Business rule: Only allow deletion of Inactive or Pending suppliers
            return supplier.Status == "Inactive" || supplier.Status == "Pending";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if supplier {Id} can be deleted", id);
            return false;
        }
    }

    public async Task<ValidationResult> ValidateSupplierAsync(SupplierInputModel model, int? excludeId = null)
    {
        var result = new ValidationResult();

        // Basic validation
        if (string.IsNullOrWhiteSpace(model.Name))
            result.AddError("Supplier name is required");

        if (string.IsNullOrWhiteSpace(model.Status))
            result.AddError("Status is required");

        // Business rules validation
        try
        {
            // Check for duplicate names
            if (await _repository.NameExistsAsync(model.Name, excludeId))
                result.AddError("A supplier with this name already exists");

            // Validate email format if provided
            if (!string.IsNullOrEmpty(model.Email) && !IsValidEmail(model.Email))
                result.AddError("Invalid email format");

            // Validate website URL if provided
            if (!string.IsNullOrEmpty(model.Website) && !IsValidUrl(model.Website))
                result.AddError("Invalid website URL format");

            // Business rules
            if (!IsValidStatus(model.Status))
                result.AddError("Invalid status value");

            if (model.Rating.HasValue && (model.Rating < 1 || model.Rating > 5))
                result.AddError("Rating must be between 1 and 5");

            // Warnings
            if (string.IsNullOrEmpty(model.ContactPerson))
                result.AddWarning("Contact person is recommended for better communication");

            if (string.IsNullOrEmpty(model.Email))
                result.AddWarning("Email address is recommended for electronic communication");

            if (string.IsNullOrEmpty(model.Phone))
                result.AddWarning("Phone number is recommended for urgent communications");

            if (model.Status == "Active" && !model.Rating.HasValue)
                result.AddWarning("Rating is recommended for active suppliers");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during supplier validation");
            result.AddError("Validation error occurred");
        }

        return result;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    private bool IsValidStatus(string status)
    {
        var validStatuses = new[] { "Active", "Inactive", "Pending", "Suspended" };
        return validStatuses.Contains(status);
    }

    private IEnumerable<SupplierViewModel> FilterSuppliersByUserRole(IEnumerable<SupplierViewModel> suppliers)
    {
        var role = _userContextService.GetCurrentUserRole();

        return role switch
        {
            UserRole.Administrator => suppliers, // Admin sees all suppliers
            UserRole.Purchaser => suppliers, // Purchaser sees all suppliers
            UserRole.Approver => suppliers.Where(s => s.Status == "Active"), // Only active suppliers
            UserRole.Requestor => suppliers.Where(s => s.Status == "Active"), // Only active suppliers
            _ => Enumerable.Empty<SupplierViewModel>()
        };
    }
}