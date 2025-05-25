using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Models;
public class PurchaseOrderRequestViewModel
{
    public int Id { get; set; }

    [Display(Name = "Item Name")]
    public string ItemName { get; set; }

    [Display(Name = "Quantity")]
    public int Quantity { get; set; }

    [Display(Name = "Comment")]
    public string Comment { get; set; }

    [Display(Name = "Request Date")]
    public DateTime RequestDate { get; set; }

    [Display(Name = "Requested By")]
    public string RequestedBy { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; }

    // New enhanced properties with defaults for compatibility
    [Display(Name = "Last Updated")]
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    [Display(Name = "Updated By")]
    public string UpdatedBy { get; set; } = "";

    [Display(Name = "Justification")]
    public string Justification { get; set; } = "";

    [Display(Name = "Priority")]
    public string Priority { get; set; } = "Medium";

    [Display(Name = "Department")]
    public string Department { get; set; } = "";

    [Display(Name = "Budget Code")]
    public string BudgetCode { get; set; } = "";

    [Display(Name = "Expected Delivery Date")]
    public DateTime? ExpectedDeliveryDate { get; set; }

    // Status display properties
    public bool CanApprove => Status == "PendingApproval" || Status == "Pending";
    public bool CanReject => Status == "PendingApproval" || Status == "Pending";
    public bool CanCancel => Status != "Rejected" && Status != "Cancelled" && Status != "PurchaseOrderCreated";
    public bool CanCreatePO => Status == "Approved";

    public string StatusBadgeClass => Status switch
    {
        "Created" => "bg-secondary",
        "Pending" => "bg-warning",
        "PendingApproval" => "bg-warning",
        "Approved" => "bg-success",
        "Rejected" => "bg-danger",
        "PurchaseOrderCreated" => "bg-info",
        "Cancelled" => "bg-dark",
        _ => "bg-secondary"
    };

    public string StatusIcon => Status switch
    {
        "Created" => "bi-plus-circle",
        "Pending" => "bi-clock",
        "PendingApproval" => "bi-clock",
        "Approved" => "bi-check-circle",
        "Rejected" => "bi-x-circle",
        "PurchaseOrderCreated" => "bi-file-text",
        "Cancelled" => "bi-dash-circle",
        _ => "bi-question-circle"
    };
}