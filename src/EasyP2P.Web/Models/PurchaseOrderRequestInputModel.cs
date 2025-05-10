using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Models;

public class PurchaseOrderRequestInputModel
{
    [Required(ErrorMessage = "Item name is required")]
    [Display(Name = "Item Name")]
    public string ItemName { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    [Display(Name = "Quantity")]
    public int Quantity { get; set; }

    [Display(Name = "Comment")]
    public string Comment { get; set; }
}