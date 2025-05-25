using EasyP2P.Web.Enums;

namespace EasyP2P.Web.Extensions;

public static class PurchaseOrderRequestExtensions
{
    /// <summary>
    /// Gets the display name for a status enum value.
    /// </summary>
    public static string GetDisplayName(this PurchaseOrderRequestState status)
    {
        return status switch
        {
            PurchaseOrderRequestState.Created => "Created",
            PurchaseOrderRequestState.PendingApproval => "Pending Approval",
            PurchaseOrderRequestState.Approved => "Approved",
            PurchaseOrderRequestState.Rejected => "Rejected",
            PurchaseOrderRequestState.PurchaseOrderCreated => "Purchase Order Created",
            PurchaseOrderRequestState.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }

    /// <summary>
    /// Gets the CSS class for status badge styling.
    /// </summary>
    public static string GetBadgeClass(this PurchaseOrderRequestState status)
    {
        return status switch
        {
            PurchaseOrderRequestState.Created => "bg-secondary",
            PurchaseOrderRequestState.PendingApproval => "bg-warning",
            PurchaseOrderRequestState.Approved => "bg-success",
            PurchaseOrderRequestState.Rejected => "bg-danger",
            PurchaseOrderRequestState.PurchaseOrderCreated => "bg-info",
            PurchaseOrderRequestState.Cancelled => "bg-dark",
            _ => "bg-secondary"
        };
    }

    /// <summary>
    /// Gets the Bootstrap icon class for the status.
    /// </summary>
    public static string GetIconClass(this PurchaseOrderRequestState status)
    {
        return status switch
        {
            PurchaseOrderRequestState.Created => "bi-plus-circle",
            PurchaseOrderRequestState.PendingApproval => "bi-clock",
            PurchaseOrderRequestState.Approved => "bi-check-circle",
            PurchaseOrderRequestState.Rejected => "bi-x-circle",
            PurchaseOrderRequestState.PurchaseOrderCreated => "bi-file-text",
            PurchaseOrderRequestState.Cancelled => "bi-dash-circle",
            _ => "bi-question-circle"
        };
    }

    /// <summary>
    /// Determines which status transitions are valid from the current status.
    /// </summary>
    public static IEnumerable<PurchaseOrderRequestState> GetValidTransitions(this PurchaseOrderRequestState currentStatus)
    {
        return currentStatus switch
        {
            PurchaseOrderRequestState.Created => new[] { PurchaseOrderRequestState.PendingApproval, PurchaseOrderRequestState.Cancelled },
            PurchaseOrderRequestState.PendingApproval => new[] { PurchaseOrderRequestState.Approved, PurchaseOrderRequestState.Rejected, PurchaseOrderRequestState.Cancelled },
            PurchaseOrderRequestState.Approved => new[] { PurchaseOrderRequestState.PurchaseOrderCreated, PurchaseOrderRequestState.Cancelled },
            _ => Array.Empty<PurchaseOrderRequestState>()
        };
    }
}