namespace EasyP2P.Web.Models.Database;

public class PurchaseOrderRequestDatabaseModel
{
    public int Id { get; set; }
    public string ItemName { get; set; }
    public int Quantity { get; set; }
    public string Comment { get; set; }
    public DateTime RequestDate { get; set; }
    public string RequestedBy { get; set; }
    public string Status { get; set; }
    public DateTime LastUpdated { get; set; }
    public string UpdatedBy { get; set; }
    public string Justification { get; set; }
    public string Priority { get; set; }
    public string Department { get; set; }
    public string BudgetCode { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
}