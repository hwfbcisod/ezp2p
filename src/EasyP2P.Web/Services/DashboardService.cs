using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Models;
using EasyP2P.Web.Extensions;

namespace EasyP2P.Web.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
}

public class DashboardService : IDashboardService
{
    private readonly IPurchaseOrderRequestRepository _porRepository;
    private readonly IPurchaseOrderRepository _poRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IPurchaseOrderRequestRepository porRepository,
        IPurchaseOrderRepository poRepository,
        ILogger<DashboardService> logger)
    {
        _porRepository = porRepository;
        _poRepository = poRepository;
        _logger = logger;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        try
        {
            var dashboard = new DashboardViewModel();

            // Get all data
            var allRequests = await _porRepository.GetAllAsync();
            var allOrders = await _poRepository.GetAllAsync();

            var requestViewModels = allRequests.ToViewModels().ToList();
            var orderViewModels = allOrders.ToViewModels().ToList();

            // Calculate metrics
            dashboard.Metrics = CalculateMetrics(requestViewModels, orderViewModels);

            // Generate alerts
            dashboard.Alerts = GenerateAlerts(requestViewModels, orderViewModels);

            // Get recent activity
            dashboard.RecentActivity = GetRecentActivity(requestViewModels, orderViewModels);

            // Get pending approvals
            dashboard.PendingApprovals = GetPendingApprovals(requestViewModels, orderViewModels);

            // Calculate financial summary
            dashboard.Financial = CalculateFinancialSummary(orderViewModels);

            // Get status breakdowns
            dashboard.RequestStatusBreakdown = GetStatusBreakdown(requestViewModels.Select(r => r.Status));
            dashboard.OrderStatusBreakdown = GetStatusBreakdown(orderViewModels.Select(o => o.Status));

            // Generate trend data (last 30 days)
            dashboard.RequestTrends = GenerateTrendData(requestViewModels);

            // Get department summaries
            dashboard.DepartmentSummaries = GetDepartmentSummaries(requestViewModels, orderViewModels);

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard data");
            return new DashboardViewModel(); // Return empty dashboard on error
        }
    }

    private DashboardMetrics CalculateMetrics(List<PurchaseOrderRequestViewModel> requests, List<PurchaseOrderViewModel> orders)
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var pendingRequests = requests.Count(r => r.Status == "PendingApproval" || r.Status == "Pending");
        var pendingOrders = orders.Count(o => o.Status == "PendingApproval");
        var urgentRequests = requests.Count(r => r.Priority == "Urgent" && (r.Status == "PendingApproval" || r.Status == "Pending"));

        var completedThisMonth = orders.Count(o =>
            o.Status == "ClosedCompleted" && o.OrderDate >= startOfMonth);

        var totalValueThisMonth = orders.Where(o => o.OrderDate >= startOfMonth)
            .Sum(o => o.TotalPrice);

        var totalRequests = requests.Count();
        var approvedRequests = requests.Count(r => r.Status == "Approved" || r.Status == "PurchaseOrderCreated");
        var approvalRate = totalRequests > 0 ? (double)approvedRequests / totalRequests * 100 : 0;

        // Calculate average processing time for completed requests
        var completedRequests = requests.Where(r => r.Status == "PurchaseOrderCreated").ToList();
        var avgProcessingDays = completedRequests.Any()
            ? completedRequests.Average(r => (r.LastUpdated - r.RequestDate).TotalDays)
            : 0;

        return new DashboardMetrics
        {
            TotalRequests = totalRequests,
            TotalOrders = orders.Count(),
            PendingApprovals = pendingRequests + pendingOrders,
            UrgentItems = urgentRequests,
            CompletedThisMonth = completedThisMonth,
            TotalValueThisMonth = totalValueThisMonth,
            ApprovalRate = Math.Round(approvalRate, 1),
            AverageProcessingDays = Math.Round(avgProcessingDays, 1)
        };
    }

    private List<DashboardAlert> GenerateAlerts(List<PurchaseOrderRequestViewModel> requests, List<PurchaseOrderViewModel> orders)
    {
        var alerts = new List<DashboardAlert>();

        // Overdue deliveries
        var overdueDeliveries = requests.Count(r =>
            r.ExpectedDeliveryDate.HasValue &&
            r.ExpectedDeliveryDate.Value < DateTime.Today &&
            r.Status != "PurchaseOrderCreated" &&
            r.Status != "Cancelled" &&
            r.Status != "Rejected");

        if (overdueDeliveries > 0)
        {
            alerts.Add(new DashboardAlert
            {
                Type = "warning",
                Title = "Overdue Deliveries",
                Message = $"{overdueDeliveries} request{(overdueDeliveries != 1 ? "s" : "")} past expected delivery date",
                ActionUrl = "/PurchaseOrderRequest?filter=overdue",
                ActionText = "View Details",
                CreatedAt = DateTime.Now
            });
        }

        // High value orders pending
        var highValuePending = orders.Count(o => o.TotalPrice > 10000 && o.Status == "PendingApproval");
        if (highValuePending > 0)
        {
            alerts.Add(new DashboardAlert
            {
                Type = "info",
                Title = "High Value Orders",
                Message = $"{highValuePending} order{(highValuePending != 1 ? "s" : "")} over $10,000 pending approval",
                ActionUrl = "/PurchaseOrder?filter=highvalue",
                ActionText = "Review",
                CreatedAt = DateTime.Now
            });
        }

        return alerts.Take(5).ToList(); // Limit to 5 alerts
    }

    private List<RecentActivityItem> GetRecentActivity(List<PurchaseOrderRequestViewModel> requests, List<PurchaseOrderViewModel> orders)
    {
        var activities = new List<RecentActivityItem>();

        // Recent requests
        foreach (var request in requests.OrderByDescending(r => r.LastUpdated).Take(5))
        {
            activities.Add(new RecentActivityItem
            {
                Id = request.Id,
                Type = "POR",
                Action = GetActionFromStatus(request.Status),
                ItemName = request.ItemName,
                User = request.UpdatedBy,
                Timestamp = request.LastUpdated,
                DetailUrl = $"/PurchaseOrderRequest/Details/{request.Id}"
            });
        }

        // Recent orders
        foreach (var order in orders.OrderByDescending(o => o.OrderDate).Take(5))
        {
            activities.Add(new RecentActivityItem
            {
                Id = order.Id,
                Type = "PO",
                Action = GetActionFromStatus(order.Status),
                ItemName = order.ItemName,
                User = order.CreatedBy,
                Timestamp = order.OrderDate,
                DetailUrl = $"/PurchaseOrder/Details/{order.Id}"
            });
        }

        return activities.OrderByDescending(a => a.Timestamp).Take(10).ToList();
    }

    private List<PendingApprovalItem> GetPendingApprovals(List<PurchaseOrderRequestViewModel> requests, List<PurchaseOrderViewModel> orders)
    {
        var pending = new List<PendingApprovalItem>();

        // Pending requests
        foreach (var request in requests.Where(r => r.Status == "PendingApproval" || r.Status == "Pending"))
        {
            pending.Add(new PendingApprovalItem
            {
                Id = request.Id,
                Type = "POR",
                ItemName = request.ItemName,
                Priority = request.Priority,
                RequestDate = request.RequestDate,
                DaysWaiting = (DateTime.Now - request.RequestDate).Days,
                DetailUrl = $"/PurchaseOrderRequest/Details/{request.Id}",
                ApproveUrl = $"/PurchaseOrderRequest/Approve/{request.Id}",
                RejectUrl = $"/PurchaseOrderRequest/Reject/{request.Id}"
            });
        }

        // Pending orders
        foreach (var order in orders.Where(o => o.Status == "PendingApproval"))
        {
            pending.Add(new PendingApprovalItem
            {
                Id = order.Id,
                Type = "PO",
                ItemName = order.ItemName,
                Value = order.TotalPrice,
                RequestDate = order.OrderDate,
                DaysWaiting = (DateTime.Now - order.OrderDate).Days,
                DetailUrl = $"/PurchaseOrder/Details/{order.Id}",
                ApproveUrl = $"/PurchaseOrder/Approve/{order.Id}",
                RejectUrl = $"/PurchaseOrder/Reject/{order.Id}"
            });
        }

        return pending.OrderByDescending(p => p.Priority == "Urgent")
                     .ThenByDescending(p => p.DaysWaiting)
                     .Take(10).ToList();
    }

    private FinancialSummary CalculateFinancialSummary(List<PurchaseOrderViewModel> orders)
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var ordersThisMonth = orders.Where(o => o.OrderDate >= startOfMonth).ToList();

        return new FinancialSummary
        {
            TotalOrderedThisMonth = ordersThisMonth.Sum(o => o.TotalPrice),
            TotalApprovedThisMonth = ordersThisMonth.Where(o => o.Status != "Rejected" && o.Status != "Cancelled")
                                                   .Sum(o => o.TotalPrice),
            TotalPaidThisMonth = ordersThisMonth.Where(o => o.Status == "PaymentMade" || o.Status == "ClosedCompleted")
                                               .Sum(o => o.TotalPrice),
            AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalPrice) : 0,
            LargestOrder = orders.Any() ? orders.Max(o => o.TotalPrice) : 0,
            OrdersAwaitingPayment = orders.Count(o => o.Status == "Invoiced" || o.Status == "ThreeWayMatch"),
            ValueAwaitingPayment = orders.Where(o => o.Status == "Invoiced" || o.Status == "ThreeWayMatch")
                                         .Sum(o => o.TotalPrice)
        };
    }

    private List<StatusBreakdown> GetStatusBreakdown(IEnumerable<string> statuses)
    {
        var statusList = statuses.ToList();
        var total = statusList.Count;

        if (total == 0) return new List<StatusBreakdown>();

        return statusList.GroupBy(s => s)
                        .Select(g => new StatusBreakdown
                        {
                            Status = g.Key,
                            Count = g.Count(),
                            Percentage = Math.Round((decimal)g.Count() / total * 100, 1),
                            BadgeClass = GetBadgeClassForStatus(g.Key)
                        })
                        .OrderByDescending(s => s.Count)
                        .ToList();
    }

    private List<TrendDataPoint> GenerateTrendData(List<PurchaseOrderRequestViewModel> requests)
    {
        var trends = new List<TrendDataPoint>();
        var startDate = DateTime.Now.AddDays(-30);

        for (int i = 0; i < 30; i++)
        {
            var date = startDate.AddDays(i);
            var dayRequests = requests.Where(r => r.RequestDate.Date == date.Date).ToList();

            trends.Add(new TrendDataPoint
            {
                Date = date.ToString("MM/dd"),
                Requests = dayRequests.Count(),
                Approvals = dayRequests.Count(r => r.Status == "Approved" || r.Status == "PurchaseOrderCreated")
            });
        }

        return trends;
    }

    private List<DepartmentSummary> GetDepartmentSummaries(List<PurchaseOrderRequestViewModel> requests, List<PurchaseOrderViewModel> orders)
    {
        return requests.Where(r => !string.IsNullOrEmpty(r.Department))
                      .GroupBy(r => r.Department)
                      .Select(g => new DepartmentSummary
                      {
                          Department = g.Key,
                          ActiveRequests = g.Count(r => r.Status != "Rejected" && r.Status != "Cancelled" && r.Status != "PurchaseOrderCreated"),
                          ActiveOrders = orders.Count(o => requests.Any(r => r.Id == o.PurchaseOrderRequestId && r.Department == g.Key)),
                          TotalValue = orders.Where(o => requests.Any(r => r.Id == o.PurchaseOrderRequestId && r.Department == g.Key))
                                           .Sum(o => o.TotalPrice),
                          TopItem = g.GroupBy(r => r.ItemName).OrderByDescending(ig => ig.Count()).FirstOrDefault()?.Key ?? "N/A"
                      })
                      .OrderByDescending(d => d.TotalValue)
                      .Take(5)
                      .ToList();
    }

    private string GetActionFromStatus(string status)
    {
        return status switch
        {
            "Created" => "Created",
            "PendingApproval" => "Pending Approval",
            "Approved" => "Approved",
            "Rejected" => "Rejected",
            "PurchaseOrderCreated" => "PO Created",
            "Cancelled" => "Cancelled",
            "Sent" => "Sent to Supplier",
            "Acknowledged" => "Acknowledged",
            "FullyReceived" => "Received",
            "Invoiced" => "Invoiced",
            "PaymentMade" => "Payment Made",
            "ClosedCompleted" => "Completed",
            _ => status
        };
    }

    private string GetBadgeClassForStatus(string status)
    {
        return status switch
        {
            "Created" => "bg-secondary",
            "PendingApproval" or "Pending" => "bg-warning",
            "Approved" => "bg-success",
            "Rejected" => "bg-danger",
            "PurchaseOrderCreated" => "bg-info",
            "Cancelled" => "bg-dark",
            "Sent" => "bg-info",
            "Acknowledged" => "bg-primary",
            "PartiallyReceived" => "bg-warning",
            "FullyReceived" => "bg-success",
            "Invoiced" => "bg-info",
            "ThreeWayMatch" => "bg-warning",
            "PaymentMade" => "bg-success",
            "ClosedCompleted" => "bg-dark",
            _ => "bg-secondary"
        };
    }
}