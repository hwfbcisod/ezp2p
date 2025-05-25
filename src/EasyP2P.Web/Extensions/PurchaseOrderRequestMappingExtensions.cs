using EasyP2P.Web.Models;
using EasyP2P.Web.Models.Database;

namespace EasyP2P.Web.Extensions;

public static class PurchaseOrderRequestMappingExtensions
{
    public static PurchaseOrderRequestViewModel ToViewModel(this PurchaseOrderRequestDatabaseModel dbModel)
    {
        return new PurchaseOrderRequestViewModel
        {
            Id = dbModel.Id,
            ItemName = dbModel.ItemName,
            Quantity = dbModel.Quantity,
            Comment = dbModel.Comment,
            RequestDate = dbModel.RequestDate,
            RequestedBy = dbModel.RequestedBy,
            Status = dbModel.Status,
            LastUpdated = dbModel.LastUpdated,
            UpdatedBy = dbModel.UpdatedBy,
            Justification = dbModel.Justification,
            Priority = dbModel.Priority,
            Department = dbModel.Department,
            BudgetCode = dbModel.BudgetCode,
            ExpectedDeliveryDate = dbModel.ExpectedDeliveryDate
        };
    }

    public static IEnumerable<PurchaseOrderRequestViewModel> ToViewModels(this IEnumerable<PurchaseOrderRequestDatabaseModel> dbModels)
    {
        return dbModels.Select(ToViewModel);
    }
}