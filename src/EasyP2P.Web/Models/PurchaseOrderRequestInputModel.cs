using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Models;

public class PurchaseOrderRequestInputModel
{
    [Required(ErrorMessage = "Item name is required")]
    [Display(Name = "Item Name")]
    [StringLength(200, ErrorMessage = "Item name cannot exceed 200 characters")]
    public string ItemName { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    [Display(Name = "Quantity")]
    public int Quantity { get; set; }

    [Display(Name = "Comment")]
    [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
    public string Comment { get; set; }

    // New enhanced properties - optional for backward compatibility
    [Display(Name = "Business Justification")]
    [StringLength(1000, ErrorMessage = "Justification cannot exceed 1000 characters")]
    public string Justification { get; set; } = "";

    [Display(Name = "Priority")]
    public string Priority { get; set; } = "Medium";

    [Display(Name = "Department")]
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string Department { get; set; } = "";

    [Display(Name = "Budget Code")]
    [StringLength(50, ErrorMessage = "Budget code cannot exceed 50 characters")]
    public string BudgetCode { get; set; } = "";

    [Display(Name = "Expected Delivery Date")]
    [DataType(DataType.Date)]
    public DateTime? ExpectedDeliveryDate { get; set; }
}