using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;
using EasyP2P.Web.Models.Database;
using EasyP2P.Web.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace EasyP2P.Web.Tests.Services;

public class PurchaseOrderRequestServiceTests
{
    private readonly Mock<IPurchaseOrderRequestRepository> _mockRepository;
    private readonly Mock<IUserContextService> _mockUserContextService;
    private readonly Mock<ILogger<PurchaseOrderRequestService>> _mockLogger;
    private readonly PurchaseOrderRequestService _service;

    public PurchaseOrderRequestServiceTests()
    {
        _mockRepository = new Mock<IPurchaseOrderRequestRepository>();
        _mockUserContextService = new Mock<IUserContextService>();
        _mockLogger = new Mock<ILogger<PurchaseOrderRequestService>>();

        _service = new PurchaseOrderRequestService(
            _mockRepository.Object,
            _mockUserContextService.Object,
            _mockLogger.Object);
    }

    #region ApproveRequestAsync Tests

    [Fact]
    public async Task ApproveRequestAsync_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var requestId = 1;
        var approvedBy = "approver@test.com";

        _mockRepository.Setup(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.Approved))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.UpdateStatusAsync(requestId, PurchaseOrderRequestState.Approved, approvedBy))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ApproveRequestAsync(requestId, approvedBy);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.Approved), Times.Once);
        _mockRepository.Verify(r => r.UpdateStatusAsync(requestId, PurchaseOrderRequestState.Approved, approvedBy), Times.Once);
    }

    [Fact]
    public async Task ApproveRequestAsync_InvalidTransition_ReturnsFalse()
    {
        // Arrange
        var requestId = 1;
        var approvedBy = "approver@test.com";

        _mockRepository.Setup(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.Approved))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ApproveRequestAsync(requestId, approvedBy);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.Approved), Times.Once);
        _mockRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<PurchaseOrderRequestState>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ApproveRequestAsync_RepositoryThrowsException_ReturnsFalse()
    {
        // Arrange
        var requestId = 1;
        var approvedBy = "approver@test.com";

        _mockRepository.Setup(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.Approved))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.ApproveRequestAsync(requestId, approvedBy);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RejectRequestAsync Tests

    [Fact]
    public async Task RejectRequestAsync_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var requestId = 1;
        var rejectedBy = "approver@test.com";
        var rejectionReason = "Budget constraints";

        _mockRepository.Setup(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.Rejected))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.UpdateStatusAsync(requestId, PurchaseOrderRequestState.Rejected, rejectedBy))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RejectRequestAsync(requestId, rejectedBy, rejectionReason);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.Rejected), Times.Once);
        _mockRepository.Verify(r => r.UpdateStatusAsync(requestId, PurchaseOrderRequestState.Rejected, rejectedBy), Times.Once);
    }

    [Fact]
    public async Task RejectRequestAsync_InvalidTransition_ReturnsFalse()
    {
        // Arrange
        var requestId = 1;
        var rejectedBy = "approver@test.com";

        _mockRepository.Setup(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.Rejected))
            .ReturnsAsync(false);

        // Act
        var result = await _service.RejectRequestAsync(requestId, rejectedBy);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<PurchaseOrderRequestState>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region CreateRequestAsync Tests

    [Fact]
    public async Task CreateRequestAsync_ValidModel_ReturnsId()
    {
        // Arrange
        var model = CreateValidPurchaseOrderRequestInputModel();
        var requestedBy = "user@test.com";
        var expectedId = 123;

        _mockRepository.Setup(r => r.CreateAsync(model, requestedBy))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _service.CreateRequestAsync(model, requestedBy);

        // Assert
        result.Should().Be(expectedId);
        _mockRepository.Verify(r => r.CreateAsync(model, requestedBy), Times.Once);
    }

    [Fact]
    public async Task CreateRequestAsync_InvalidModel_ThrowsValidationException()
    {
        // Arrange
        var model = new PurchaseOrderRequestInputModel(); // Empty model - invalid
        var requestedBy = "user@test.com";

        // Act & Assert
        await _service.Invoking(s => s.CreateRequestAsync(model, requestedBy))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Validation failed:*");
    }

    [Fact]
    public async Task CreateRequestAsync_RepositoryThrowsException_ThrowsInvalidOperationException()
    {
        // Arrange
        var model = CreateValidPurchaseOrderRequestInputModel();
        var requestedBy = "user@test.com";

        _mockRepository.Setup(r => r.CreateAsync(model, requestedBy))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await _service.Invoking(s => s.CreateRequestAsync(model, requestedBy))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to create purchase order request");
    }

    #endregion

    #region ValidateRequestAsync Tests

    [Fact]
    public async Task ValidateRequestAsync_ValidModel_ReturnsValidResult()
    {
        // Arrange
        var model = CreateValidPurchaseOrderRequestInputModel();

        // Act
        var result = await _service.ValidateRequestAsync(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateRequestAsync_EmptyItemName_ReturnsInvalidResult()
    {
        // Arrange
        var model = CreateValidPurchaseOrderRequestInputModel();
        model.ItemName = "";

        // Act
        var result = await _service.ValidateRequestAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Item name is required");
    }

    [Fact]
    public async Task ValidateRequestAsync_ZeroQuantity_ReturnsInvalidResult()
    {
        // Arrange
        var model = CreateValidPurchaseOrderRequestInputModel();
        model.Quantity = 0;

        // Act
        var result = await _service.ValidateRequestAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Quantity must be greater than 0");
    }

    [Fact]
    public async Task ValidateRequestAsync_NegativeQuantity_ReturnsInvalidResult()
    {
        // Arrange
        var model = CreateValidPurchaseOrderRequestInputModel();
        model.Quantity = -5;

        // Act
        var result = await _service.ValidateRequestAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Quantity must be greater than 0");
    }

    [Fact]
    public async Task ValidateRequestAsync_EmptyJustification_ReturnsInvalidResult()
    {
        // Arrange
        var model = CreateValidPurchaseOrderRequestInputModel();
        model.Justification = "";

        // Act
        var result = await _service.ValidateRequestAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Business justification is required");
    }

    [Fact]
    public async Task ValidateRequestAsync_EmptyPriority_ReturnsInvalidResult()
    {
        // Arrange
        var model = CreateValidPurchaseOrderRequestInputModel();
        model.Priority = "";

        // Act
        var result = await _service.ValidateRequestAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Priority is required");
    }

    [Fact]
    public async Task ValidateRequestAsync_EmptyDepartment_ReturnsInvalidResult()
    {
        // Arrange
        var model = CreateValidPurchaseOrderRequestInputModel();
        model.Department = "";

        // Act
        var result = await _service.ValidateRequestAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Department is required");
    }

    [Fact]
    public async Task ValidateRequestAsync_PastDeliveryDate_ReturnsInvalidResult()
    {
        // Arrange
        var model = CreateValidPurchaseOrderRequestInputModel();
        model.ExpectedDeliveryDate = DateTime.Today.AddDays(-1);

        // Act
        var result = await _service.ValidateRequestAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Expected delivery date must be at least tomorrow");
    }

    [Fact]
    public async Task ValidateRequestAsync_UrgentPriorityWithDistantDeliveryDate_ReturnsWarning()
    {
        // Arrange
        var model = CreateValidPurchaseOrderRequestInputModel();
        model.Priority = "Urgent";
        model.ExpectedDeliveryDate = DateTime.Today.AddDays(10);

        // Act
        var result = await _service.ValidateRequestAsync(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain("Urgent requests typically have delivery dates within 3 days");
    }

    #endregion

    #region GetRequestByIdAsync Tests

    [Fact]
    public async Task GetRequestByIdAsync_ExistingRequest_ReturnsViewModel()
    {
        // Arrange
        var requestId = 1;
        var dbModel = CreateValidDatabaseModel();

        _mockRepository.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(dbModel);

        // Act
        var result = await _service.GetRequestByIdAsync(requestId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(dbModel.Id);
        result.ItemName.Should().Be(dbModel.ItemName);
        result.Quantity.Should().Be(dbModel.Quantity);
    }

    [Fact]
    public async Task GetRequestByIdAsync_NonExistingRequest_ReturnsNull()
    {
        // Arrange
        var requestId = 999;

        _mockRepository.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((PurchaseOrderRequestDatabaseModel?)null);

        // Act
        var result = await _service.GetRequestByIdAsync(requestId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRequestByIdAsync_WithPermissionEnforcement_RequestorCanOnlyViewOwnRequests()
    {
        // Arrange
        var requestId = 1;
        var dbModel = CreateValidDatabaseModel();
        dbModel.RequestedBy = "other@test.com";

        _mockRepository.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(dbModel);
        _mockUserContextService.Setup(u => u.CanViewEntity("POR", "other@test.com", dbModel.Department))
            .Returns(false);

        // Act
        var result = await _service.GetRequestByIdAsync(requestId, enforcePermissions: true);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetFilteredRequestsAsync Tests

    [Fact]
    public async Task GetFilteredRequestsAsync_RequestorRole_ReturnsOnlyOwnRequests()
    {
        // Arrange
        var currentUser = "user@test.com";
        var allRequests = new List<PurchaseOrderRequestDatabaseModel>
        {
            CreateValidDatabaseModel(id: 1, requestedBy: currentUser),
            CreateValidDatabaseModel(id: 2, requestedBy: "other@test.com"),
            CreateValidDatabaseModel(id: 3, requestedBy: currentUser)
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allRequests);
        _mockUserContextService.Setup(u => u.GetCurrentUserRole())
            .Returns(UserRole.Requestor);
        _mockUserContextService.Setup(u => u.GetCurrentUser())
            .Returns(currentUser);

        // Act
        var result = await _service.GetFilteredRequestsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.RequestedBy == currentUser);
    }

    [Fact]
    public async Task GetFilteredRequestsAsync_AdministratorRole_ReturnsAllRequests()
    {
        // Arrange
        var allRequests = new List<PurchaseOrderRequestDatabaseModel>
        {
            CreateValidDatabaseModel(id: 1, requestedBy: "user1@test.com"),
            CreateValidDatabaseModel(id: 2, requestedBy: "user2@test.com"),
            CreateValidDatabaseModel(id: 3, requestedBy: "user3@test.com")
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allRequests);
        _mockUserContextService.Setup(u => u.GetCurrentUserRole())
            .Returns(UserRole.Administrator);

        // Act
        var result = await _service.GetFilteredRequestsAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetFilteredRequestsAsync_ApproverRole_ReturnsOnlyDepartmentRequests()
    {
        // Arrange
        var currentDepartment = "IT";
        var allRequests = new List<PurchaseOrderRequestDatabaseModel>
        {
            CreateValidDatabaseModel(id: 1, department: currentDepartment),
            CreateValidDatabaseModel(id: 2, department: "HR"),
            CreateValidDatabaseModel(id: 3, department: currentDepartment)
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allRequests);
        _mockUserContextService.Setup(u => u.GetCurrentUserRole())
            .Returns(UserRole.Approver);
        _mockUserContextService.Setup(u => u.GetAccessibleDepartments())
            .Returns(new List<string> { currentDepartment });

        // Act
        var result = await _service.GetFilteredRequestsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Department == currentDepartment);
    }

    #endregion

    #region MarkPurchaseOrderCreatedAsync Tests

    [Fact]
    public async Task MarkPurchaseOrderCreatedAsync_ValidTransition_ReturnsTrue()
    {
        // Arrange
        var requestId = 1;
        var updatedBy = "purchaser@test.com";

        _mockRepository.Setup(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.PurchaseOrderCreated))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.UpdateStatusAsync(requestId, PurchaseOrderRequestState.PurchaseOrderCreated, updatedBy))
            .ReturnsAsync(true);

        // Act
        var result = await _service.MarkPurchaseOrderCreatedAsync(requestId, updatedBy);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateStatusAsync(requestId, PurchaseOrderRequestState.PurchaseOrderCreated, updatedBy), Times.Once);
    }

    [Fact]
    public async Task MarkPurchaseOrderCreatedAsync_InvalidTransition_ReturnsFalse()
    {
        // Arrange
        var requestId = 1;
        var updatedBy = "purchaser@test.com";

        _mockRepository.Setup(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.PurchaseOrderCreated))
            .ReturnsAsync(false);

        // Act
        var result = await _service.MarkPurchaseOrderCreatedAsync(requestId, updatedBy);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<PurchaseOrderRequestState>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region CancelRequestAsync Tests

    [Fact]
    public async Task CancelRequestAsync_ValidCancellation_ReturnsTrue()
    {
        // Arrange
        var requestId = 1;
        var cancelledBy = "user@test.com";
        var cancellationReason = "No longer needed";

        _mockRepository.Setup(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.Cancelled))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.UpdateStatusAsync(requestId, PurchaseOrderRequestState.Cancelled, cancelledBy))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CancelRequestAsync(requestId, cancelledBy, cancellationReason);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateStatusAsync(requestId, PurchaseOrderRequestState.Cancelled, cancelledBy), Times.Once);
    }

    [Fact]
    public async Task CancelRequestAsync_InvalidTransition_ReturnsFalse()
    {
        // Arrange
        var requestId = 1;
        var cancelledBy = "user@test.com";

        _mockRepository.Setup(r => r.CanTransitionToStatus(requestId, PurchaseOrderRequestState.Cancelled))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CancelRequestAsync(requestId, cancelledBy);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<PurchaseOrderRequestState>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private PurchaseOrderRequestInputModel CreateValidPurchaseOrderRequestInputModel()
    {
        return new PurchaseOrderRequestInputModel
        {
            ItemName = "Test Item",
            Quantity = 5,
            Comment = "Test comment",
            Justification = "Business need for test purposes",
            Priority = "Medium",
            Department = "IT",
            BudgetCode = "IT-2025-001",
            ExpectedDeliveryDate = DateTime.Today.AddDays(7)
        };
    }

    private PurchaseOrderRequestDatabaseModel CreateValidDatabaseModel(
        int id = 1,
        string requestedBy = "user@test.com",
        string department = "IT",
        string status = "PendingApproval")
    {
        return new PurchaseOrderRequestDatabaseModel
        {
            Id = id,
            ItemName = "Test Item",
            Quantity = 5,
            Comment = "Test comment",
            RequestDate = DateTime.UtcNow.AddDays(-1),
            RequestedBy = requestedBy,
            Status = status,
            LastUpdated = DateTime.UtcNow,
            UpdatedBy = requestedBy,
            Justification = "Business need for test purposes",
            Priority = "Medium",
            Department = department,
            BudgetCode = "IT-2025-001",
            ExpectedDeliveryDate = DateTime.Today.AddDays(7)
        };
    }

    #endregion
}