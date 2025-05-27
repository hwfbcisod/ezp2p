using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;
using EasyP2P.Web.Extensions;
using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Services;

/// <summary>
/// Service layer for Purchase Order Request business logic and validation.
/// </summary>
public interface IPurchaseOrderRequestService
{
    Task<bool> ApproveRequestAsync(int id, string approvedBy);
    Task<bool> RejectRequestAsync(int id, string rejectedBy, string? rejectionReason = null);
    Task<bool> CancelRequestAsync(int id, string cancelledBy, string? cancellationReason = null);
    Task<bool> MarkPurchaseOrderCreatedAsync(int id, string updatedBy);
    Task<ValidationResult> ValidateRequestAsync(PurchaseOrderRequestInputModel model);
    Task<IEnumerable<PurchaseOrderRequestViewModel>> GetDashboardDataAsync(string? userFilter = null);
    Task<PurchaseOrderRequestViewModel?> GetRequestByIdAsync(int id);
    Task<IEnumerable<PurchaseOrderRequestViewModel>> GetAllRequestsAsync();
    Task<IEnumerable<PurchaseOrderRequestViewModel>> GetRequestsByStatusAsync(PurchaseOrderRequestState status);
    Task<int> CreateRequestAsync(PurchaseOrderRequestInputModel model, string requestedBy);
    Task<IEnumerable<PurchaseOrderRequestViewModel>> GetFilteredRequestsAsync();
    Task<IEnumerable<PurchaseOrderRequestViewModel>> GetRequestsForApprovalAsync();
    Task<PurchaseOrderRequestViewModel?> GetRequestByIdAsync(int id, bool enforcePermissions = true);
}

public class PurchaseOrderRequestService : IPurchaseOrderRequestService
{
    private readonly IPurchaseOrderRequestRepository _repository;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<PurchaseOrderRequestService> _logger;

    public PurchaseOrderRequestService(
        IPurchaseOrderRequestRepository repository,
        IUserContextService userContextService,
        ILogger<PurchaseOrderRequestService> logger)
    {
        _repository = repository;
        _userContextService = userContextService;
        _logger = logger;
    }

    public async Task<bool> ApproveRequestAsync(int id, string approvedBy)
    {
        try
        {
            // Validate transition
            if (!await _repository.CanTransitionToStatus(id, PurchaseOrderRequestState.Approved))
            {
                _logger.LogWarning("Cannot approve request {Id} - invalid state transition", id);
                return false;
            }

            // Update status
            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderRequestState.Approved, approvedBy);

            if (result)
            {
                _logger.LogInformation("Purchase Order Request {Id} approved by {User}", id, approvedBy);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving request {Id}", id);
            return false;
        }
    }

    public async Task<bool> RejectRequestAsync(int id, string rejectedBy, string? rejectionReason = null)
    {
        try
        {
            if (!await _repository.CanTransitionToStatus(id, PurchaseOrderRequestState.Rejected))
            {
                _logger.LogWarning("Cannot reject request {Id} - invalid state transition", id);
                return false;
            }

            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderRequestState.Rejected, rejectedBy);

            if (result)
            {
                _logger.LogInformation("Purchase Order Request {Id} rejected by {User}. Reason: {Reason}",
                    id, rejectedBy, rejectionReason ?? "Not specified");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting request {Id}", id);
            return false;
        }
    }

    public async Task<bool> CancelRequestAsync(int id, string cancelledBy, string? cancellationReason = null)
    {
        try
        {
            if (!await _repository.CanTransitionToStatus(id, PurchaseOrderRequestState.Cancelled))
            {
                _logger.LogWarning("Cannot cancel request {Id} - invalid state transition", id);
                return false;
            }

            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderRequestState.Cancelled, cancelledBy);

            if (result)
            {
                _logger.LogInformation("Purchase Order Request {Id} cancelled by {User}. Reason: {Reason}",
                    id, cancelledBy, cancellationReason ?? "Not specified");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling request {Id}", id);
            return false;
        }
    }

    public async Task<int> CreateRequestAsync(PurchaseOrderRequestInputModel model, string requestedBy)
    {
        try
        {
            _logger.LogInformation("Creating purchase order request for item {ItemName} by {User}",
                model.ItemName, requestedBy);

            // Validate the request using business rules
            var validationResult = await ValidateRequestAsync(model);
            if (!validationResult.IsValid)
            {
                var errorMessage = $"Validation failed: {string.Join(", ", validationResult.Errors)}";
                _logger.LogWarning("Validation failed for purchase order request: {Errors}", errorMessage);
                throw new ValidationException(errorMessage);
            }

            // Log warnings but don't block creation
            if (validationResult.Warnings.Any())
            {
                _logger.LogWarning("Purchase order request has warnings: {Warnings}",
                    string.Join(", ", validationResult.Warnings));
            }

            // Create the request through repository
            var id = await _repository.CreateAsync(model, requestedBy);

            _logger.LogInformation("Purchase Order Request created successfully with ID {Id} by {User}",
                id, requestedBy);

            return id;
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase order request for item {ItemName} by {User}",
                model.ItemName, requestedBy);
            throw new InvalidOperationException("Failed to create purchase order request", ex);
        }
    }

    public async Task<bool> MarkPurchaseOrderCreatedAsync(int id, string updatedBy)
    {
        try
        {
            if (!await _repository.CanTransitionToStatus(id, PurchaseOrderRequestState.PurchaseOrderCreated))
            {
                _logger.LogWarning("Cannot mark request {Id} as PO created - invalid state transition", id);
                return false;
            }

            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderRequestState.PurchaseOrderCreated, updatedBy);

            if (result)
            {
                _logger.LogInformation("Purchase Order Request {Id} marked as PO created by {User}", id, updatedBy);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking request {Id} as PO created", id);
            return false;
        }
    }

    public async Task<ValidationResult> ValidateRequestAsync(PurchaseOrderRequestInputModel model)
    {
        var result = new ValidationResult();

        // Basic validation
        if (string.IsNullOrWhiteSpace(model.ItemName))
            result.AddError("Item name is required");

        if (model.Quantity <= 0)
            result.AddError("Quantity must be greater than 0");

        if (string.IsNullOrWhiteSpace(model.Justification))
            result.AddError("Business justification is required");

        if (string.IsNullOrWhiteSpace(model.Priority))
            result.AddError("Priority is required");

        if (string.IsNullOrWhiteSpace(model.Department))
            result.AddError("Department is required");

        // Business rules validation
        if (model.ExpectedDeliveryDate.HasValue && model.ExpectedDeliveryDate.Value < DateTime.Today.AddDays(1))
            result.AddError("Expected delivery date must be at least tomorrow");

        if (model.Priority == "Urgent" && model.ExpectedDeliveryDate.HasValue &&
            model.ExpectedDeliveryDate.Value > DateTime.Today.AddDays(3))
            result.AddWarning("Urgent requests typically have delivery dates within 3 days");

        return result;
    }

    public async Task<IEnumerable<PurchaseOrderRequestViewModel>> GetDashboardDataAsync(string? userFilter = null)
    {
        try
        {
            var allRequests = await _repository.GetAllAsync();
            var viewModels = allRequests.ToViewModels();

            // Filter requests that require attention
            var requestsRequiringAttention = viewModels.Where(r =>
                r.Status == "PendingApproval" ||
                r.Priority == "Urgent" ||
                (r.ExpectedDeliveryDate.HasValue && r.ExpectedDeliveryDate.Value <= DateTime.Today.AddDays(3))
            );

            if (!string.IsNullOrEmpty(userFilter))
            {
                requestsRequiringAttention = requestsRequiringAttention.Where(r =>
                    r.RequestedBy.Equals(userFilter, StringComparison.OrdinalIgnoreCase));
            }

            return requestsRequiringAttention
                .OrderByDescending(r => r.Priority == "Urgent")
                .ThenByDescending(r => r.Priority == "High")
                .ThenBy(r => r.RequestDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data");
            return Enumerable.Empty<PurchaseOrderRequestViewModel>();
        }
    }

    public async Task<PurchaseOrderRequestViewModel?> GetRequestByIdAsync(int id)
    {
        try
        {
            var dbModel = await _repository.GetByIdAsync(id);
            return dbModel?.ToViewModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving request {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<PurchaseOrderRequestViewModel>> GetAllRequestsAsync()
    {
        try
        {
            var dbModels = await _repository.GetAllAsync();
            return dbModels.ToViewModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all requests");
            return Enumerable.Empty<PurchaseOrderRequestViewModel>();
        }
    }

    public async Task<IEnumerable<PurchaseOrderRequestViewModel>> GetRequestsByStatusAsync(PurchaseOrderRequestState status)
    {
        try
        {
            var dbModels = await _repository.GetByStatusAsync(status);
            return dbModels.ToViewModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving requests with status {Status}", status);
            return Enumerable.Empty<PurchaseOrderRequestViewModel>();
        }
    }

    public async Task<IEnumerable<PurchaseOrderRequestViewModel>> GetFilteredRequestsAsync()
    {
        var allRequests = await _repository.GetAllAsync();
        var viewModels = allRequests.ToViewModels();

        return FilterRequestsByUserRole(viewModels);
    }

    public async Task<IEnumerable<PurchaseOrderRequestViewModel>> GetRequestsForApprovalAsync()
    {
        var pendingRequests = await _repository.GetByStatusAsync(PurchaseOrderRequestState.PendingApproval);
        var viewModels = pendingRequests.ToViewModels();

        return FilterRequestsByUserRole(viewModels)
            .Where(r => r.Status == "PendingApproval");
    }

    public async Task<PurchaseOrderRequestViewModel?> GetRequestByIdAsync(int id, bool enforcePermissions = true)
    {
        var dbModel = await _repository.GetByIdAsync(id);
        if (dbModel == null) return null;

        var viewModel = dbModel.ToViewModel();

        // Apply permission filtering if enforced
        if (enforcePermissions && !_userContextService.CanViewEntity("POR", viewModel.RequestedBy, viewModel.Department))
        {
            _logger.LogWarning("User {User} attempted to access POR {Id} without permission",
                _userContextService.GetCurrentUser(), id);
            return null;
        }

        return viewModel;
    }

    private IEnumerable<PurchaseOrderRequestViewModel> FilterRequestsByUserRole(IEnumerable<PurchaseOrderRequestViewModel> requests)
    {
        var role = _userContextService.GetCurrentUserRole();
        var currentUser = _userContextService.GetCurrentUser();
        var accessibleDepartments = _userContextService.GetAccessibleDepartments();

        return role switch
        {
            UserRole.Administrator => requests, // Admin sees everything
            UserRole.Purchaser => requests, // Purchaser sees everything
            UserRole.Approver => requests.Where(r =>
                string.IsNullOrEmpty(r.Department) || accessibleDepartments.Contains(r.Department)),
            UserRole.Requestor => requests.Where(r => r.RequestedBy == currentUser),
            _ => Enumerable.Empty<PurchaseOrderRequestViewModel>()
        };
    }
}

public class ValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();

    public bool IsValid => !Errors.Any();

    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
}