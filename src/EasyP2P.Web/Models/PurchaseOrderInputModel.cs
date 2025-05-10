using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Models;

/// <summary>
/// Input model for creating a new purchase order.
/// </summary>
public class PurchaseOrderModel
{
    /// <summary>
    /// ID of the associated purchase order request.
    /// </summary>
    public int PurchaseOrderRequestId { get; set; }

    /// <summary>
    /// Name of the item being ordered.
    /// </summary>
    [Required(ErrorMessage = "Item name is required")]
    [Display(Name = "Item Name")]
    public string ItemName { get; set; }

    /// <summary>
    /// Quantity of the item being ordered.
    /// </summary>
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    [Display(Name = "Quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// Price per unit of the item.
    /// </summary>
    [Required(ErrorMessage = "Unit price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total price of the order (Quantity * UnitPrice).
    /// </summary>
    [Display(Name = "Total Price")]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Name of the supplier providing the items.
    /// </summary>
    [Required(ErrorMessage = "Supplier is required")]
    [Display(Name = "Supplier")]
    public string Supplier { get; set; }
}
