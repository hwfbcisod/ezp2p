using Microsoft.AspNetCore.Mvc.Rendering;

namespace EasyP2P.Web.Models;
public class FilterViewModel
{
    // Common Filters
    public string? Status { get; set; }
    public string? SearchTerm { get; set; } // Generic search term for item, name, etc.
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
    public string? EntityId { get; set; } // For PO ID, POR ID, Supplier ID

    // Purchase Order Request Specific
    public string? Priority { get; set; }
    public string? Department { get; set; }
    public string? BudgetCode { get; set; }
    public string? RequestedBy { get; set; }
    public string? DeliveryDateFilter { get; set; } // e.g., "overdue", "thisweek"

    // Purchase Order Specific
    public string? Supplier { get; set; } // Supplier name for PO
    public int? QuantityMin { get; set; }
    public int? QuantityMax { get; set; }
    public decimal? UnitPriceMin { get; set; }
    public decimal? UnitPriceMax { get; set; }
    public decimal? TotalPriceMin { get; set; }
    public decimal? TotalPriceMax { get; set; }
    public int? RelatedRequestId { get; set; } // POR ID for PO

    // Supplier Specific
    public string? Location { get; set; } // City, State, Country for Supplier
    public int? MinRating { get; set; }

    // Configuration to control which filters are displayed
    public FilterType CurrentFilterType { get; set; }

    // Dropdown options - to be populated by the controller
    public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> PriorityOptions { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> DepartmentOptions { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> RatingOptions { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> DeliveryDateOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "All Dates" },
            new SelectListItem { Value = "overdue", Text = "Overdue" },
            new SelectListItem { Value = "thisweek", Text = "This Week" },
            new SelectListItem { Value = "nextweek", Text = "Next Week" },
            new SelectListItem { Value = "thismonth", Text = "This Month" }
        };

    public FilterViewModel()
    {
        // Initialize default options if needed, or better, from controller
        StatusOptions.Add(new SelectListItem { Value = "", Text = "All Statuses" });
        PriorityOptions.Add(new SelectListItem { Value = "", Text = "All Priorities" });
        DepartmentOptions.Add(new SelectListItem { Value = "", Text = "All Departments" });
        RatingOptions.Add(new SelectListItem { Value = "", Text = "All Ratings" });
    }
}

public enum FilterType
{
    PurchaseOrderRequest,
    PurchaseOrder,
    Supplier
}
