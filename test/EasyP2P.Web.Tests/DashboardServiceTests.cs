using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Models.Database;
using EasyP2P.Web.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyP2P.Web.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IPurchaseOrderRequestRepository> _mockPorRepository;
    private readonly Mock<IPurchaseOrderRepository> _mockPoRepository;
    private readonly Mock<ISupplierRepository> _mockSupplierRepository;
    private readonly Mock<ILogger<DashboardService>> _mockLogger;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _mockPorRepository = new Mock<IPurchaseOrderRequestRepository>();
        _mockPoRepository = new Mock<IPurchaseOrderRepository>();
        _mockSupplierRepository = new Mock<ISupplierRepository>();
        _mockLogger = new Mock<ILogger<DashboardService>>();

        _service = new DashboardService(
            _mockPorRepository.Object,
            _mockPoRepository.Object,
            _mockSupplierRepository.Object,
            _mockLogger.Object);
    }

    #region GetDashboardDataAsync Tests

    [Fact]
    public async Task GetDashboardDataAsync_WithValidData_ReturnsCompleteViewModel()
    {
        var requests = CreateSampleRequests();
        var orders = CreateSampleOrders();
        var suppliers = CreateSampleSuppliers();

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(requests);
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(suppliers);

        var result = await _service.GetDashboardDataAsync();

        result.Should().NotBeNull();
        result.Metrics.Should().NotBeNull();
        result.Alerts.Should().NotBeNull();
        result.RecentActivity.Should().NotBeNull();
        result.PendingApprovals.Should().NotBeNull();
        result.Financial.Should().NotBeNull();
        result.RequestStatusBreakdown.Should().NotBeNull();
        result.RequestTrends.Should().NotBeNull();
        result.DepartmentSummaries.Should().NotBeNull();
        result.SupplierSummaries.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDashboardDataAsync_CalculatesMetricsCorrectly()
    {
        var requests = new List<PurchaseOrderRequestDatabaseModel>
        {
            CreateRequest(id: 1, status: "PendingApproval", priority: "Urgent"),
            CreateRequest(id: 2, status: "Approved"),
            CreateRequest(id: 3, status: "PurchaseOrderCreated"),
            CreateRequest(id: 4, status: "PendingApproval")
        };

        var orders = new List<PurchaseOrderDatabaseModel>
        {
            CreateOrder(id: 1, status: "PendingApproval"),
            CreateOrder(id: 2, status: "Approved"),
            CreateOrder(id: 3, status: "ClosedCompleted", orderDate: DateTime.Now.AddDays(-5))
        };

        var suppliers = new List<SupplierDatabaseModel>
        {
            CreateSupplier(id: 1, status: "Active", rating: 5),
            CreateSupplier(id: 2, status: "Active", rating: 4),
            CreateSupplier(id: 3, status: "Inactive")
        };

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(requests);
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(suppliers);

        var result = await _service.GetDashboardDataAsync();

        result.Metrics.TotalRequests.Should().Be(4);
        result.Metrics.TotalOrders.Should().Be(3);
        result.Metrics.PendingApprovals.Should().Be(3);
        result.Metrics.UrgentItems.Should().Be(1);
        result.Metrics.CompletedThisMonth.Should().Be(1);
        result.Metrics.TotalSuppliers.Should().Be(3);
        result.Metrics.ActiveSuppliers.Should().Be(2);
        result.Metrics.TopRatedSuppliers.Should().Be(2);
    }

    [Fact]
    public async Task GetDashboardDataAsync_GeneratesAlertsForOverdueDeliveries()
    {
        var requests = new List<PurchaseOrderRequestDatabaseModel>
        {
            CreateRequest(id: 1, status: "PendingApproval", expectedDeliveryDate: DateTime.Today.AddDays(-1)),
            CreateRequest(id: 2, status: "Approved", expectedDeliveryDate: DateTime.Today.AddDays(-2)),
            CreateRequest(id: 3, status: "PurchaseOrderCreated") // Shouldn't count
        };

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(requests);
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PurchaseOrderDatabaseModel>());
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SupplierDatabaseModel>());

        var result = await _service.GetDashboardDataAsync();

        var overdueAlert = result.Alerts.FirstOrDefault(a => a.Type == "warning" && a.Title == "Overdue Deliveries");
        overdueAlert.Should().NotBeNull();
        overdueAlert!.Message.Should().Contain("2 requests past expected delivery date");
    }

    [Fact]
    public async Task GetDashboardDataAsync_GeneratesAlertsForHighValueOrders()
    {
        var orders = new List<PurchaseOrderDatabaseModel>
        {
            CreateOrder(id: 1, status: "PendingApproval", totalPrice: 15000m),
            CreateOrder(id: 2, status: "PendingApproval", totalPrice: 12000m),
            CreateOrder(id: 3, status: "Approved", totalPrice: 15000m)
        };

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PurchaseOrderRequestDatabaseModel>());
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SupplierDatabaseModel>());

        var result = await _service.GetDashboardDataAsync();

        var highValueAlert = result.Alerts.FirstOrDefault(a => a.Type == "info" && a.Title == "High Value Orders");
        highValueAlert.Should().NotBeNull();
        highValueAlert!.Message.Should().Contain("2 orders over $10,000 pending approval");
    }

    [Fact]
    public async Task GetDashboardDataAsync_GeneratesSupplierAlerts()
    {
        var suppliers = new List<SupplierDatabaseModel>
        {
            CreateSupplier(id: 1, status: "Suspended"),
            CreateSupplier(id: 2, status: "Suspended"),
            CreateSupplier(id: 3, status: "Active", rating: 2),
            CreateSupplier(id: 4, status: "Active", rating: 1)
        };

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PurchaseOrderRequestDatabaseModel>());
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PurchaseOrderDatabaseModel>());
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(suppliers);

        var result = await _service.GetDashboardDataAsync();

        var suspendedAlert = result.Alerts.FirstOrDefault(a => a.Title == "Suspended Suppliers");
        suspendedAlert.Should().NotBeNull();
        suspendedAlert!.Message.Should().Contain("2 suppliers currently suspended");

        var lowRatedAlert = result.Alerts.FirstOrDefault(a => a.Title == "Low Rated Suppliers");
        lowRatedAlert.Should().NotBeNull();
        lowRatedAlert!.Message.Should().Contain("2 active suppliers with poor ratings");
    }

    [Fact]
    public async Task GetDashboardDataAsync_CalculatesFinancialSummaryCorrectly()
    {
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var orders = new List<PurchaseOrderDatabaseModel>
        {
            CreateOrder(id: 1, totalPrice: 1000m, orderDate: currentMonth.AddDays(5), status: "Approved"),
            CreateOrder(id: 2, totalPrice: 2000m, orderDate: currentMonth.AddDays(10), status: "PaymentMade"),
            CreateOrder(id: 3, totalPrice: 5000m, orderDate: currentMonth.AddMonths(-1), status: "Approved"), // Previous month
            CreateOrder(id: 4, totalPrice: 500m, orderDate: currentMonth.AddDays(15), status: "Invoiced")
        };

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PurchaseOrderRequestDatabaseModel>());
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SupplierDatabaseModel>());

        
        var result = await _service.GetDashboardDataAsync();

        
        result.Financial.TotalOrderedThisMonth.Should().Be(3500m);
        result.Financial.TotalApprovedThisMonth.Should().Be(3500m);
        result.Financial.TotalPaidThisMonth.Should().Be(2000m);
        result.Financial.AverageOrderValue.Should().Be(2125m);
        result.Financial.LargestOrder.Should().Be(5000m);
        result.Financial.OrdersAwaitingPayment.Should().Be(1);
        result.Financial.ValueAwaitingPayment.Should().Be(500m);
    }

    [Fact]
    public async Task GetDashboardDataAsync_GeneratesRecentActivity()
    {
        var requests = new List<PurchaseOrderRequestDatabaseModel>
        {
            CreateRequest(id: 1, status: "Approved", lastUpdated: DateTime.Now.AddHours(-1)),
            CreateRequest(id: 2, status: "PendingApproval", lastUpdated: DateTime.Now.AddHours(-2))
        };

        var orders = new List<PurchaseOrderDatabaseModel>
        {
            CreateOrder(id: 1, status: "Sent", orderDate: DateTime.Now.AddHours(-30)),
            CreateOrder(id: 2, status: "Created", orderDate: DateTime.Now.AddHours(-4))
        };

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(requests);
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SupplierDatabaseModel>());

        var result = await _service.GetDashboardDataAsync();

        result.RecentActivity.Should().NotBeEmpty();
        result.RecentActivity.Should().HaveCountLessOrEqualTo(10);
        result.RecentActivity.Should().BeInDescendingOrder(a => a.Timestamp);
    }

    [Fact]
    public async Task GetDashboardDataAsync_GeneratesPendingApprovals()
    {
        var requests = new List<PurchaseOrderRequestDatabaseModel>
        {
            CreateRequest(id: 1, status: "PendingApproval", priority: "Urgent", requestDate: DateTime.Now.AddDays(-2)),
            CreateRequest(id: 2, status: "PendingApproval", priority: "High", requestDate: DateTime.Now.AddDays(-1))
        };

        var orders = new List<PurchaseOrderDatabaseModel>
        {
            CreateOrder(id: 1, status: "PendingApproval", orderDate: DateTime.Now.AddDays(-3), totalPrice: 5000m)
        };

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(requests);
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SupplierDatabaseModel>());

        var result = await _service.GetDashboardDataAsync();

        result.PendingApprovals.Should().HaveCount(3);
        result.PendingApprovals.Should().Contain(p => p.Type == "POR" && p.Id == 1);
        result.PendingApprovals.Should().Contain(p => p.Type == "POR" && p.Id == 2);
        result.PendingApprovals.Should().Contain(p => p.Type == "PO" && p.Id == 1);

        var firstItem = result.PendingApprovals.First();
        firstItem.Priority.Should().Be("Urgent");
    }

    [Fact]
    public async Task GetDashboardDataAsync_GeneratesStatusBreakdown()
    {
        var requests = new List<PurchaseOrderRequestDatabaseModel>
        {
            CreateRequest(id: 1, status: "PendingApproval"),
            CreateRequest(id: 2, status: "PendingApproval"),
            CreateRequest(id: 3, status: "Approved"),
            CreateRequest(id: 4, status: "Rejected")
        };

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(requests);
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PurchaseOrderDatabaseModel>());
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SupplierDatabaseModel>());

        var result = await _service.GetDashboardDataAsync();

        result.RequestStatusBreakdown.Should().HaveCount(3);

        var pendingBreakdown = result.RequestStatusBreakdown.FirstOrDefault(s => s.Status == "PendingApproval");
        pendingBreakdown.Should().NotBeNull();
        pendingBreakdown!.Count.Should().Be(2);
        pendingBreakdown.Percentage.Should().Be(50.0m);
    }

    [Fact]
    public async Task GetDashboardDataAsync_GeneratesTrendData()
    {
        var requests = new List<PurchaseOrderRequestDatabaseModel>();

        for (int i = 0; i < 30; i++)
        {
            var date = DateTime.Now.AddDays(-i);
            requests.Add(CreateRequest(id: i + 1, requestDate: date, status: i % 3 == 0 ? "Approved" : "PendingApproval"));
        }

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(requests);
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PurchaseOrderDatabaseModel>());
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SupplierDatabaseModel>());

        var result = await _service.GetDashboardDataAsync();

        result.RequestTrends.Should().HaveCount(30);
        result.RequestTrends.Should().OnlyContain(t => t.Requests >= 0);
        result.RequestTrends.Should().OnlyContain(t => t.Approvals >= 0);
    }

    [Fact]
    public async Task GetDashboardDataAsync_GeneratesDepartmentSummaries()
    {
        var requests = new List<PurchaseOrderRequestDatabaseModel>
        {
            CreateRequest(id: 1, department: "IT", itemName: "Laptop"),
            CreateRequest(id: 2, department: "IT", itemName: "Monitor"),
            CreateRequest(id: 3, department: "HR", itemName: "Desk"),
            CreateRequest(id: 4, department: "IT", itemName: "Laptop")
        };

        var orders = new List<PurchaseOrderDatabaseModel>
        {
            CreateOrder(id: 1, purchaseOrderRequestId: 1, totalPrice: 1000m),
            CreateOrder(id: 2, purchaseOrderRequestId: 3, totalPrice: 500m)
        };

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(requests);
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SupplierDatabaseModel>());

        var result = await _service.GetDashboardDataAsync();

        result.DepartmentSummaries.Should().HaveCount(2);

        var itSummary = result.DepartmentSummaries.FirstOrDefault(d => d.Department == "IT");
        itSummary.Should().NotBeNull();
        itSummary!.TopItem.Should().Be("Laptop");
        itSummary.TotalValue.Should().Be(1000m);
    }

    [Fact]
    public async Task GetDashboardDataAsync_GeneratesSupplierSummaries()
    {
        var suppliers = new List<SupplierDatabaseModel>
        {
            CreateSupplier(id: 1, name: "Supplier A", status: "Active", rating: 5, paymentTerms: "Net 30"),
            CreateSupplier(id: 2, name: "Supplier B", status: "Active", rating: 4, paymentTerms: "Net 15"),
            CreateSupplier(id: 3, name: "Supplier C", status: "Inactive", rating: 3) // Should be excluded
        };

        var orders = new List<PurchaseOrderDatabaseModel>
        {
            CreateOrder(id: 1, supplier: "Supplier A", totalPrice: 2000m, orderDate: DateTime.Now.AddDays(-5)),
            CreateOrder(id: 2, supplier: "Supplier A", totalPrice: 1000m, orderDate: DateTime.Now.AddDays(-10)),
            CreateOrder(id: 3, supplier: "Supplier B", totalPrice: 500m, orderDate: DateTime.Now.AddDays(-2))
        };

        _mockPorRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PurchaseOrderRequestDatabaseModel>());
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(suppliers);

        var result = await _service.GetDashboardDataAsync();

        result.SupplierSummaries.Should().HaveCount(2);
        result.SupplierSummaries.Should().BeInDescendingOrder(s => s.TotalValue);

        var topSupplier = result.SupplierSummaries.First();
        topSupplier.SupplierName.Should().Be("Supplier A");
        topSupplier.TotalValue.Should().Be(3000m);
        topSupplier.OrderCount.Should().Be(2);
        topSupplier.RatingStars.Should().Be("★★★★★");
    }

    [Fact]
    public async Task GetDashboardDataAsync_WithRepositoryException_ReturnsEmptyDashboard()
    {
        _mockPorRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));
        _mockPoRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PurchaseOrderDatabaseModel>());
        _mockSupplierRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SupplierDatabaseModel>());

        var result = await _service.GetDashboardDataAsync();

        result.Should().NotBeNull();
        result.Metrics.Should().NotBeNull();
        result.Alerts.Should().BeEmpty();
        result.RecentActivity.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private List<PurchaseOrderRequestDatabaseModel> CreateSampleRequests()
    {
        return new List<PurchaseOrderRequestDatabaseModel>
        {
            CreateRequest(id: 1, status: "PendingApproval", priority: "High"),
            CreateRequest(id: 2, status: "Approved", priority: "Medium"),
            CreateRequest(id: 3, status: "PurchaseOrderCreated", priority: "Low")
        };
    }

    private List<PurchaseOrderDatabaseModel> CreateSampleOrders()
    {
        return new List<PurchaseOrderDatabaseModel>
        {
            CreateOrder(id: 1, status: "PendingApproval", totalPrice: 1000m),
            CreateOrder(id: 2, status: "Approved", totalPrice: 2000m),
            CreateOrder(id: 3, status: "Sent", totalPrice: 1500m)
        };
    }

    private List<SupplierDatabaseModel> CreateSampleSuppliers()
    {
        return new List<SupplierDatabaseModel>
        {
            CreateSupplier(id: 1, status: "Active", rating: 5),
            CreateSupplier(id: 2, status: "Active", rating: 4),
            CreateSupplier(id: 3, status: "Inactive", rating: 3)
        };
    }

    private PurchaseOrderRequestDatabaseModel CreateRequest(
        int id = 1,
        string status = "PendingApproval",
        string priority = "Medium",
        string department = "IT",
        string itemName = "Test Item",
        DateTime? requestDate = null,
        DateTime? lastUpdated = null,
        DateTime? expectedDeliveryDate = null)
    {
        return new PurchaseOrderRequestDatabaseModel
        {
            Id = id,
            ItemName = itemName,
            Quantity = 1,
            Comment = "Test comment",
            RequestDate = requestDate ?? DateTime.UtcNow.AddDays(-1),
            RequestedBy = "user@test.com",
            Status = status,
            LastUpdated = lastUpdated ?? DateTime.UtcNow,
            UpdatedBy = "user@test.com",
            Justification = "Business need",
            Priority = priority,
            Department = department,
            BudgetCode = "TEST-001",
            ExpectedDeliveryDate = expectedDeliveryDate
        };
    }

    private PurchaseOrderDatabaseModel CreateOrder(
        int id = 1,
        string status = "PendingApproval",
        decimal totalPrice = 1000m,
        DateTime? orderDate = null,
        string supplier = "Test Supplier",
        int purchaseOrderRequestId = 1)
    {
        return new PurchaseOrderDatabaseModel
        {
            Id = id,
            PurchaseOrderRequestId = purchaseOrderRequestId,
            ItemName = "Test Item",
            Quantity = 1,
            UnitPrice = totalPrice,
            TotalPrice = totalPrice,
            Supplier = supplier,
            OrderDate = orderDate ?? DateTime.UtcNow,
            DeliveryDate = DateTime.UtcNow.AddDays(7),
            CreatedBy = "user@test.com",
            Status = status
        };
    }

    private SupplierDatabaseModel CreateSupplier(
        int id = 1,
        string name = "Test Supplier",
        string status = "Active",
        int? rating = null,
        string paymentTerms = "Net 30")
    {
        return new SupplierDatabaseModel
        {
            Id = id,
            Name = name,
            ContactPerson = "John Doe",
            Email = "contact@supplier.com",
            Phone = "+1-555-0123",
            Address = "123 Business St",
            City = "Business City",
            State = "BC",
            Country = "USA",
            PostalCode = "12345",
            TaxId = "TAX123",
            PaymentTerms = paymentTerms,
            Status = status,
            Rating = rating,
            Website = "https://supplier.com",
            Notes = "Test supplier",
            CreatedDate = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "admin@test.com",
            LastUpdated = DateTime.UtcNow,
            UpdatedBy = "admin@test.com"
        };
    }

    #endregion
}