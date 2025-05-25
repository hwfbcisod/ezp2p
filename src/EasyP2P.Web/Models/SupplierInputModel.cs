// Models/SupplierInputModel.cs
using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Models;

/// <summary>
/// Input model for creating or editing a supplier.
/// </summary>
public class SupplierInputModel
{
    [Required(ErrorMessage = "Supplier name is required")]
    [Display(Name = "Supplier Name")]
    [StringLength(200, ErrorMessage = "Supplier name cannot exceed 200 characters")]
    public string Name { get; set; } = "";

    [Display(Name = "Contact Person")]
    [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
    public string ContactPerson { get; set; } = "";

    [Display(Name = "Email Address")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = "";

    [Display(Name = "Phone Number")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string Phone { get; set; } = "";

    [Display(Name = "Address")]
    [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
    public string Address { get; set; } = "";

    [Display(Name = "City")]
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string City { get; set; } = "";

    [Display(Name = "State/Province")]
    [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
    public string State { get; set; } = "";

    [Display(Name = "Country")]
    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
    public string Country { get; set; } = "";

    [Display(Name = "Postal Code")]
    [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
    public string PostalCode { get; set; } = "";

    [Display(Name = "Tax ID")]
    [StringLength(50, ErrorMessage = "Tax ID cannot exceed 50 characters")]
    public string TaxId { get; set; } = "";

    [Display(Name = "Payment Terms")]
    [StringLength(100, ErrorMessage = "Payment terms cannot exceed 100 characters")]
    public string PaymentTerms { get; set; } = "";

    [Required(ErrorMessage = "Status is required")]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Active";

    [Display(Name = "Rating")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int? Rating { get; set; }

    [Display(Name = "Website")]
    [Url(ErrorMessage = "Please enter a valid website URL")]
    [StringLength(200, ErrorMessage = "Website URL cannot exceed 200 characters")]
    public string Website { get; set; } = "";

    [Display(Name = "Notes")]
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string Notes { get; set; } = "";
}