using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Extensions;
using EasyP2P.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Services;

/// <summary>
/// Service layer for Purchase Order business logic and validation.
/// </summary>
public interface IPurchaseOrderService
{
    Task<PurchaseOrderViewModel?> GetOrderByIdAsync(int id);
    Task<IEnumerable<PurchaseOrderViewModel>> GetAllOrdersAsync();
    Task<IEnumerable<PurchaseOrderViewModel>> GetOrdersByStatusAsync(PurchaseOrderState status);
    Task<IEnumerable<PurchaseOrderViewModel>> GetOrdersByRequestIdAsync(int requestId);
    Task<bool> ApproveOrderAsync(int id, string approvedBy);
    Task<bool> RejectOrderAsync(int id, string rejectedBy);
    Task<bool> CancelOrderAsync(int id, string cancelledBy);
    Task<ValidationResult> ValidateOrderAsync(PurchaseOrderModel model);
    Task<int> CreateOrderAsync(PurchaseOrderModel model, string createdBy);
    Task<bool> SendOrderAsync(int id);
    Task<bool> AcknowledgeOrderAsync(int id);
    Task<bool> AttachDeliveryNoteAsync(int id, string deliveryNoteFilePath);
    Task AttachInvoiceAsync(int id, string invoiceFilePath);
    Task<bool> PayOrderAsync(int id);
}

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<PurchaseOrderService> _logger;

    public PurchaseOrderService(
        IPurchaseOrderRepository repository,
        IUserContextService userContextService,
        ILogger<PurchaseOrderService> logger)
    {
        _repository = repository;
        _userContextService = userContextService;
        _logger = logger;
    }

    public async Task<PurchaseOrderViewModel?> GetOrderByIdAsync(int id)
    {
        var dbModel = await _repository.GetByIdAsync(id);
        if (dbModel == null) return null;

        var viewModel = dbModel.ToViewModel();

        if (!_userContextService.CanViewEntity("PO", viewModel.CreatedBy))
        {
            _logger.LogWarning("User {User} attempted to access PO {Id} without permission",
                _userContextService.GetCurrentUser(), id);
            return null;
        }

        return viewModel;
    }

    public async Task<IEnumerable<PurchaseOrderViewModel>> GetAllOrdersAsync()
    {
        var allOrders = await _repository.GetAllAsync();
        var viewModels = allOrders.ToViewModels();

        return FilterOrdersByUserRole(viewModels);
    }

    public async Task<IEnumerable<PurchaseOrderViewModel>> GetOrdersByStatusAsync(PurchaseOrderState status)
    {
        try
        {
            var dbModels = await _repository.GetByStatusAsync(status);
            return dbModels.ToViewModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders with status {Status}", status);
            return Enumerable.Empty<PurchaseOrderViewModel>();
        }
    }

    public async Task<IEnumerable<PurchaseOrderViewModel>> GetOrdersByRequestIdAsync(int requestId)
    {
        try
        {
            var dbModels = await _repository.GetByRequestIdAsync(requestId);
            return dbModels.ToViewModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for request {RequestId}", requestId);
            return Enumerable.Empty<PurchaseOrderViewModel>();
        }
    }

    public async Task<int> CreateOrderAsync(PurchaseOrderModel model, string createdBy)
    {
        try
        {
            _logger.LogInformation("Creating purchase order for item {ItemName} by {User}",
                model.ItemName, createdBy);

            // Calculate total price (business logic)
            model.TotalPrice = model.Quantity * model.UnitPrice;

            // Validate the order using business rules
            var validationResult = await ValidateOrderAsync(model);
            if (!validationResult.IsValid)
            {
                var errorMessage = $"Validation failed: {string.Join(", ", validationResult.Errors)}";
                _logger.LogWarning("Validation failed for purchase order: {Errors}", errorMessage);
                throw new ValidationException(errorMessage);
            }

            // Log warnings but don't block creation
            if (validationResult.Warnings.Any())
            {
                _logger.LogWarning("Purchase order has warnings: {Warnings}",
                    string.Join(", ", validationResult.Warnings));
            }

            // Create the order through repository
            var orderId = await _repository.CreateAsync(model, createdBy);

            _logger.LogInformation("Purchase Order created successfully with ID {Id} by {User}",
                orderId, createdBy);

            return orderId;
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase order for item {ItemName} by {User}",
                model.ItemName, createdBy);
            throw new InvalidOperationException("Failed to create purchase order", ex);
        }
    }

    public async Task<bool> ApproveOrderAsync(int id, string approvedBy)
    {
        try
        {
            var order = await _repository.GetByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Cannot approve order {Id} - order not found", id);
                return false;
            }

            if (order.Status != "PendingApproval")
            {
                _logger.LogWarning("Cannot approve order {Id} - invalid state {Status}", id, order.Status);
                return false;
            }

            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderState.Approved);

            if (result)
            {
                _logger.LogInformation("Purchase Order {Id} approved by {User}", id, approvedBy);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving order {Id}", id);
            return false;
        }
    }

    public async Task<bool> RejectOrderAsync(int id, string rejectedBy)
    {
        try
        {
            var order = await _repository.GetByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Cannot reject order {Id} - order not found", id);
                return false;
            }

            if (order.Status != "PendingApproval")
            {
                _logger.LogWarning("Cannot reject order {Id} - invalid state {Status}", id, order.Status);
                return false;
            }

            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderState.Rejected);

            if (result)
            {
                _logger.LogInformation("Purchase Order {Id} rejected by {User}", id, rejectedBy);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting order {Id}", id);
            return false;
        }
    }

    public async Task<bool> CancelOrderAsync(int id, string cancelledBy)
    {
        try
        {
            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderState.Cancelled);

            if (result)
            {
                _logger.LogInformation("Purchase Order {Id} cancelled by {User}", id, cancelledBy);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {Id}", id);
            return false;
        }
    }

    public async Task<bool> SendOrderAsync(int id)
    {
        // TODO: Make this method really send the purchase order to a supplier, not just update the status
        try
        {
            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderState.Sent);

            if (result)
            {
                _logger.LogInformation("Purchase Order {Id} sent to supplier", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while trying to send order {Id} to supplier!", id);
            return false;
        }
    }

    public async Task<bool> AcknowledgeOrderAsync(int id)
    {
        //TODO: Make this method really acknowledge the purchase order, not just update the status
        try
        {
            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderState.Acknowledged);
            if (result)
            {
                _logger.LogInformation("Purchase Order {Id} acknowledged by supplier", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging order {Id}", id);
            return false;
        }
    }

    public async Task<bool> AttachDeliveryNoteAsync(int id, string deliveryNoteFilePath)
    {
        try
        {
            // TODO: Scrape the data from the delivery note and compare it to data in the purchase order.
            // Depending on the result, update with either FullyReceived or PartiallyReceived status.
            // Use an LLM to perform these tasks.
            using var fileStream = new FileStream(deliveryNoteFilePath, FileMode.Open, FileAccess.Read);
            
            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderState.FullyReceived);
            if (result)
            {
                _logger.LogInformation("Items for purchase order {Id} have been fully delivered by the supplier", id);
            }
            return result;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attaching delivery note for order {Id}", id);
            throw;
        }
    }

    public async Task AttachInvoiceAsync(int id, string invoiceFilePath)
    {
        try
        {
            using var fileStream = new FileStream(invoiceFilePath, FileMode.Open, FileAccess.Read);
            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderState.Invoiced);
            // TODO: Scrape the data from the invoice and perform a three way match with the corresponding purchase order and delivery note
            // Use an LLM to perform these tasks.
            result = await _repository.UpdateStatusAsync(id, PurchaseOrderState.ThreeWayMatch);
            result = await _repository.UpdateStatusAsync(id, PurchaseOrderState.PendingPayment);
            if (result)
            {
                _logger.LogInformation("Invoice for purchase order {Id} has been attached and matched", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attaching invoice for order {Id}", id);
            throw;
        }
    }

    public async Task<bool> PayOrderAsync(int id)
    {
        try
        {
            // TODO: Implement actual payment processing logic here.
            var result = await _repository.UpdateStatusAsync(id, PurchaseOrderState.Completed);
            // TODO: Attach proof of payment document to the purchase order and make it availabe for Download in the Details view.
            if (result)
            {
                _logger.LogInformation("Purchase Order {Id} has been paid", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error paying for order {Id}", id);
            return false;
        }
    }

    public async Task<ValidationResult> ValidateOrderAsync(PurchaseOrderModel model)
    {
        var result = new ValidationResult();

        // Basic validation
        if (string.IsNullOrWhiteSpace(model.ItemName))
            result.AddError("Item name is required");

        if (model.Quantity <= 0)
            result.AddError("Quantity must be greater than 0");

        if (model.UnitPrice <= 0)
            result.AddError("Unit price must be greater than 0");

        if (string.IsNullOrWhiteSpace(model.Supplier))
            result.AddError("Supplier is required");

        // Business rules validation
        if (model.TotalPrice != model.Quantity * model.UnitPrice)
            result.AddError("Total price calculation is incorrect");

        // Large order validation
        if (model.TotalPrice > 10000)
            result.AddWarning("Large orders over $10,000 may require additional approval");

        return result;
    }

    private IEnumerable<PurchaseOrderViewModel> FilterOrdersByUserRole(IEnumerable<PurchaseOrderViewModel> orders)
    {
        var role = _userContextService.GetCurrentUserRole();
        var currentUser = _userContextService.GetCurrentUser();

        return role switch
        {
            UserRole.Administrator => orders, // Admin sees everything
            UserRole.Purchaser => orders, // Purchaser sees everything
            UserRole.Approver => orders, // Approver can see all orders for approval purposes
            UserRole.Requestor => orders.Where(o => o.CreatedBy == currentUser), // Only own orders
            _ => Enumerable.Empty<PurchaseOrderViewModel>()
        };
    }
}