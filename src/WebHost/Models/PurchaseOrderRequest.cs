
using System.ComponentModel.DataAnnotations;

namespace WebHost2.Models;

public class PurchaseOrderRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Item name is required")]
    [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters")]
    public string ItemName { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Comments are required")]
    [StringLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters")]
    public string Comments { get; set; }

    // User information (could be auto-populated from logged-in user)
    public string RequestorName { get; set; }

    // Status and timestamps
    public string Status { get; set; } = "Pending";
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}