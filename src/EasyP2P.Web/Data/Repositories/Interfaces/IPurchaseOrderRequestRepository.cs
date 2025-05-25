using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;

namespace EasyP2P.Web.Data.Repositories.Interfaces;

public interface IPurchaseOrderRequestRepository
{
    Task<IEnumerable<PurchaseOrderRequestViewModel>> GetAllAsync();
    Task<PurchaseOrderRequestViewModel?> GetByIdAsync(int id);
    Task<int> CreateAsync(PurchaseOrderRequestInputModel model, string requestedBy);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<PurchaseOrderRequestViewModel>> GetByStatusAsync(string status);
    Task<bool> UpdateStatusAsync(int id, PurchaseOrderRequestState status, string updatedBy);
    Task<bool> CanTransitionToStatus(int id, PurchaseOrderRequestState newStatus);
}