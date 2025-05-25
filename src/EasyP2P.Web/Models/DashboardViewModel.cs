using System.ComponentModel.DataAnnotations;

namespace EasyP2P.Web.Models;

public class DashboardViewModel
{
    public DashboardMetrics Metrics { get; set; } = new();
    public List<DashboardAlert> Alerts { get; set; } = new();
    public List<RecentActivityItem> RecentActivity { get; set; } = new();
    public List<PendingApprovalItem> PendingApprovals { get; set; } = new();
    public FinancialSummary Financial { get; set; } = new();
    public List<StatusBreakdown> RequestStatusBreakdown { get; set; } = new();
    public List<StatusBreakdown> OrderStatusBreakdown { get; set; } = new();
    public List<TrendDataPoint> RequestTrends { get; set; } = new();
    public List<DepartmentSummary> DepartmentSummaries { get; set; } = new();
    public List<SupplierSummary> SupplierSummaries { get; set; } = new();
}

public class DashboardMetrics
{
    public int TotalRequests { get; set; }
    public int TotalOrders { get; set; }
    public int PendingApprovals { get; set; }
    public int UrgentItems { get; set; }
    public int CompletedThisMonth { get; set; }
    public decimal TotalValueThisMonth { get; set; }
    public double ApprovalRate { get; set; }
    public double AverageProcessingDays { get; set; }
    public int TotalSuppliers { get; set; }
    public int ActiveSuppliers { get; set; }
    public int TopRatedSuppliers { get; set; }
}

public class DashboardAlert
{
    public string Type { get; set; } = ""; // "urgent", "warning", "info"
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string ActionUrl { get; set; } = "";
    public string ActionText { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class RecentActivityItem
{
    public int Id { get; set; }
    public string Type { get; set; } = ""; // "POR", "PO"
    public string Action { get; set; } = ""; // "Created", "Approved", etc.
    public string ItemName { get; set; } = "";
    public string User { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string DetailUrl { get; set; } = "";
}

public class PendingApprovalItem
{
    public int Id { get; set; }
    public string Type { get; set; } = ""; // "POR", "PO"
    public string ItemName { get; set; } = "";
    public string Priority { get; set; } = "";
    public decimal? Value { get; set; }
    public DateTime RequestDate { get; set; }
    public int DaysWaiting { get; set; }
    public string DetailUrl { get; set; } = "";
    public string ApproveUrl { get; set; } = "";
    public string RejectUrl { get; set; } = "";
}

public class FinancialSummary
{
    public decimal TotalRequestedThisMonth { get; set; }
    public decimal TotalApprovedThisMonth { get; set; }
    public decimal TotalOrderedThisMonth { get; set; }
    public decimal TotalPaidThisMonth { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal LargestOrder { get; set; }
    public int OrdersAwaitingPayment { get; set; }
    public decimal ValueAwaitingPayment { get; set; }
}

public class StatusBreakdown
{
    public string Status { get; set; } = "";
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public string BadgeClass { get; set; } = "";
}

public class TrendDataPoint
{
    public string Date { get; set; } = "";
    public int Requests { get; set; }
    public int Approvals { get; set; }
    public decimal Value { get; set; }
}

public class DepartmentSummary
{
    public string Department { get; set; } = "";
    public int ActiveRequests { get; set; }
    public int ActiveOrders { get; set; }
    public decimal TotalValue { get; set; }
    public string TopItem { get; set; } = "";
}

public class SupplierSummary
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = "";
    public string Status { get; set; } = "";
    public int? Rating { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public string PaymentTerms { get; set; } = "";
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
}