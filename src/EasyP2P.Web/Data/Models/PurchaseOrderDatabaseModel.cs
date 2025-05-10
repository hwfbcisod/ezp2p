namespace EasyP2P.Web.Models.Database;

/// <summary>
/// Database model representing a purchase order in the database.
/// Maps directly to the purchase_orders table.
/// </summary>
public class PurchaseOrderDatabaseModel
{
    /// <summary>
    /// Primary key identifier for the purchase order.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the associated purchase order request.
    /// </summary>
    public int PurchaseOrderRequestId { get; set; }

    /// <summary>
    /// Name of the item being ordered.
    /// </summary>
    public string ItemName { get; set; }

    /// <summary>
    /// Quantity of the item being ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Price per unit of the item.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total price of the order (Quantity * UnitPrice).
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Name of the supplier providing the items.
    /// </summary>
    public string Supplier { get; set; }

    /// <summary>
    /// Date when the purchase order was created.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// User who created the purchase order.
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// Current status of the purchase order.
    /// </summary>
    public string Status { get; set; }
}