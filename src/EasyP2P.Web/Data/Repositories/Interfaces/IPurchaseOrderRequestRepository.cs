// Repositories/Interfaces/IPurchaseOrderRequestRepository.cs
using EasyP2P.Web.Models;

namespace EasyP2P.Web.Data.Repositories.Interfaces;

public interface IPurchaseOrderRequestRepository
{
    // Get all purchase order requests
    Task<IEnumerable<PurchaseOrderRequestViewModel>> GetAllAsync();

    // Get a single purchase order request by ID
    Task<PurchaseOrderRequestViewModel?> GetByIdAsync(int id);

    // Create a new purchase order request
    Task CreateAsync(PurchaseOrderRequestInputModel model, string requestedBy);

    // Delete a purchase order request
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<PurchaseOrderRequestViewModel>> GetByStatusAsync(string status);

    Task UpdateStatusAsync(int id, string status);
}