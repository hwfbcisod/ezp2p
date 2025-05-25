using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;

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
    //Task<IEnumerable<PurchaseOrderRequestViewModel>> GetDashboardDataAsync(string? userFilter = null);
}

public class PurchaseOrderRequestService : IPurchaseOrderRequestService
{
    private readonly IPurchaseOrderRequestRepository _repository;
    private readonly ILogger<PurchaseOrderRequestService> _logger;

    public PurchaseOrderRequestService(
        IPurchaseOrderRequestRepository repository,
        ILogger<PurchaseOrderRequestService> logger)
    {
        _repository = repository;
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

    //public async Task<IEnumerable<PurchaseOrderRequestViewModel>> GetDashboardDataAsync(string? userFilter = null)
    //{
    //    var requests = await _repository.GetRequestsRequiringAttentionAsync();

    //    if (!string.IsNullOrEmpty(userFilter))
    //    {
    //        requests = requests.Where(r => r.RequestedBy.Equals(userFilter, StringComparison.OrdinalIgnoreCase));
    //    }

    //    return requests.OrderByDescending(r => r.Priority == "Urgent")
    //                  .ThenByDescending(r => r.Priority == "High")
    //                  .ThenBy(r => r.RequestDate);
    //}
}

public class ValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();

    public bool IsValid => !Errors.Any();

    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
}
