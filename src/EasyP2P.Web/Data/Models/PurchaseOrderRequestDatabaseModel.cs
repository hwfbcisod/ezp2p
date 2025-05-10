// Models/Database/PurchaseOrderRequestDatabaseModel.cs
namespace EasyP2P.Web.Models.Database;

/// <summary>
/// Database model representing a purchase order request in the database.
/// Maps directly to the PurchaseOrderRequests table.
/// </summary>
public class PurchaseOrderRequestDatabaseModel
{
    /// <summary>
    /// Primary key identifier for the purchase order request.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the item being requested.
    /// </summary>
    public string ItemName { get; set; }

    /// <summary>
    /// Quantity of the item being requested.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Optional comment or additional information about the request.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Date and time when the request was submitted.
    /// </summary>
    public DateTime RequestDate { get; set; }

    /// <summary>
    /// Name or identifier of the user who created the request.
    /// </summary>
    public string RequestedBy { get; set; }

    public string Status { get; set; }

}