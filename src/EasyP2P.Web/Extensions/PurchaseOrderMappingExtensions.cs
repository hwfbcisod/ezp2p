using EasyP2P.Web.Models;
using EasyP2P.Web.Models.Database;

namespace EasyP2P.Web.Extensions;

public static class PurchaseOrderMappingExtensions
{
    public static PurchaseOrderViewModel ToViewModel(this PurchaseOrderDatabaseModel dbModel)
    {
        return new PurchaseOrderViewModel
        {
            Id = dbModel.Id,
            PurchaseOrderRequestId = dbModel.PurchaseOrderRequestId,
            ItemName = dbModel.ItemName,
            Quantity = dbModel.Quantity,
            UnitPrice = dbModel.UnitPrice,
            TotalPrice = dbModel.TotalPrice,
            Supplier = dbModel.Supplier,
            OrderDate = dbModel.OrderDate,
            CreatedBy = dbModel.CreatedBy,
            Status = dbModel.Status
        };
    }

    public static IEnumerable<PurchaseOrderViewModel> ToViewModels(this IEnumerable<PurchaseOrderDatabaseModel> dbModels)
    {
        return dbModels.Select(ToViewModel);
    }
}