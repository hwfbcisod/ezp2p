using EasyP2P.Web.Models;

namespace EasyP2P.Web.Data.Repositories.Interfaces;

public interface IPurchaseOrderRepository
{
    // Get all purchase orders
    Task<IEnumerable<PurchaseOrderViewModel>> GetAllAsync();

    // Get purchase orders by status
    Task<IEnumerable<PurchaseOrderViewModel>> GetByStatusAsync(string status);

    // Get a single purchase order by ID
    Task<PurchaseOrderViewModel?> GetByIdAsync(int id);

    // Get purchase orders by purchase order request ID
    Task<IEnumerable<PurchaseOrderViewModel>> GetByRequestIdAsync(int requestId);

    // Create a new purchase order
    Task<int> CreateAsync(PurchaseOrderModel model, string createdBy);

    // Update the status of a purchase order
    Task<bool> UpdateStatusAsync(int id, string status);

    // Delete a purchase order
    Task<bool> DeleteAsync(int id);
}