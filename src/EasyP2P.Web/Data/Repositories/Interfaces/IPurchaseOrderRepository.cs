using EasyP2P.Web.Models;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Models.Database;

namespace EasyP2P.Web.Data.Repositories.Interfaces;

public interface IPurchaseOrderRepository
{
    // Get all purchase orders
    Task<IEnumerable<PurchaseOrderDatabaseModel>> GetAllAsync();

    // Get purchase orders by status
    Task<IEnumerable<PurchaseOrderDatabaseModel>> GetByStatusAsync(PurchaseOrderState status);

    // Get a single purchase order by ID
    Task<PurchaseOrderDatabaseModel?> GetByIdAsync(int id);

    // Get purchase orders by purchase order request ID
    Task<IEnumerable<PurchaseOrderDatabaseModel>> GetByRequestIdAsync(int requestId);

    // Create a new purchase order
    Task<int> CreateAsync(PurchaseOrderModel model, string createdBy);

    // Update the status of a purchase order
    Task<bool> UpdateStatusAsync(int id, PurchaseOrderState status);

    // Delete a purchase order
    Task<bool> DeleteAsync(int id);
}