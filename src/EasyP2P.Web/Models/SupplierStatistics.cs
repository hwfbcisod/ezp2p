
namespace EasyP2P.Web.Models;

public class SupplierStatistics
{
    public int TotalSuppliers { get; set; }
    public int ActiveSuppliers { get; set; }
    public int InactiveSuppliers { get; set; }
    public int PendingSuppliers { get; set; }
    public int SuspendedSuppliers { get; set; }
    public double AverageRating { get; set; }
    public int HighRatedSuppliers { get; set; }
}