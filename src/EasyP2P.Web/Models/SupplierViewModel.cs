// Models/SupplierViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Models;

/// <summary>
/// View model for displaying supplier information.
/// </summary>
public class SupplierViewModel
{
    public int Id { get; set; }

    [Display(Name = "Supplier Name")]
    public string Name { get; set; } = "";

    [Display(Name = "Contact Person")]
    public string ContactPerson { get; set; } = "";

    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Display(Name = "Phone")]
    public string Phone { get; set; } = "";

    [Display(Name = "Address")]
    public string Address { get; set; } = "";

    [Display(Name = "City")]
    public string City { get; set; } = "";

    [Display(Name = "State")]
    public string State { get; set; } = "";

    [Display(Name = "Country")]
    public string Country { get; set; } = "";

    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = "";

    [Display(Name = "Tax ID")]
    public string TaxId { get; set; } = "";

    [Display(Name = "Payment Terms")]
    public string PaymentTerms { get; set; } = "";

    [Display(Name = "Status")]
    public string Status { get; set; } = "";

    [Display(Name = "Rating")]
    public int? Rating { get; set; }

    [Display(Name = "Website")]
    public string Website { get; set; } = "";

    [Display(Name = "Notes")]
    public string Notes { get; set; } = "";

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; }

    [Display(Name = "Created By")]
    public string CreatedBy { get; set; } = "";

    [Display(Name = "Last Updated")]
    public DateTime LastUpdated { get; set; }

    [Display(Name = "Updated By")]
    public string UpdatedBy { get; set; } = "";

    // Computed properties for UI
    public string StatusBadgeClass => Status switch
    {
        "Active" => "bg-success",
        "Inactive" => "bg-secondary",
        "Pending" => "bg-warning",
        "Suspended" => "bg-danger",
        _ => "bg-secondary"
    };

    public string StatusIcon => Status switch
    {
        "Active" => "bi-check-circle",
        "Inactive" => "bi-pause-circle",
        "Pending" => "bi-clock",
        "Suspended" => "bi-x-circle",
        _ => "bi-question-circle"
    };

    public string RatingStars
    {
        get
        {
            if (!Rating.HasValue) return "Not Rated";

            var stars = "";
            for (int i = 1; i <= 5; i++)
            {
                stars += i <= Rating.Value ? "★" : "☆";
            }
            return stars;
        }
    }

    public string RatingClass => Rating switch
    {
        5 => "text-success",
        4 => "text-info",
        3 => "text-warning",
        2 => "text-danger",
        1 => "text-danger",
        _ => "text-muted"
    };

    public string FullAddress
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Address)) parts.Add(Address);
            if (!string.IsNullOrEmpty(City)) parts.Add(City);
            if (!string.IsNullOrEmpty(State)) parts.Add(State);
            if (!string.IsNullOrEmpty(PostalCode)) parts.Add(PostalCode);
            if (!string.IsNullOrEmpty(Country)) parts.Add(Country);

            return string.Join(", ", parts);
        }
    }

    public bool CanEdit => Status != "Suspended";
    public bool CanDelete => Status == "Inactive" || Status == "Pending";
    public bool CanActivate => Status == "Inactive" || Status == "Pending";
    public bool CanDeactivate => Status == "Active";
}