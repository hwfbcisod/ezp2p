namespace EasyP2P.Web.Models;

public class PurchaseOrderRequestViewModel
{
    public int Id { get; set; }
    public string ItemName { get; set; }
    public int Quantity { get; set; }
    public string Comment { get; set; }
    public DateTime RequestDate { get; set; }
    public string RequestedBy { get; set; }
    public string Status { get; set; }
}