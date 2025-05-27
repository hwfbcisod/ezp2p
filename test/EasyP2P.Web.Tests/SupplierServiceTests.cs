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

public class SupplierServiceTests
{
    private readonly Mock<ISupplierRepository> _mockRepository;
    private readonly Mock<IUserContextService> _mockUserContextService;
    private readonly Mock<ILogger<SupplierService>> _mockLogger;
    private readonly SupplierService _service;

    public SupplierServiceTests()
    {
        _mockRepository = new Mock<ISupplierRepository>();
        _mockUserContextService = new Mock<IUserContextService>();
        _mockLogger = new Mock<ILogger<SupplierService>>();

        _service = new SupplierService(
            _mockRepository.Object,
            _mockUserContextService.Object,
            _mockLogger.Object);
    }

    #region GetSupplierByIdAsync Tests

    [Fact]
    public async Task GetSupplierByIdAsync_ExistingSupplier_ReturnsViewModel()
    {
        var supplierId = 1;
        var dbModel = CreateValidSupplierDatabaseModel();

        _mockRepository.Setup(r => r.GetByIdAsync(supplierId))
            .ReturnsAsync(dbModel);

        var result = await _service.GetSupplierByIdAsync(supplierId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(dbModel.Id);
        result.Name.Should().Be(dbModel.Name);
        result.Status.Should().Be(dbModel.Status);
    }

    [Fact]
    public async Task GetSupplierByIdAsync_NonExistingSupplier_ReturnsNull()
    {
        var supplierId = 999;

        _mockRepository.Setup(r => r.GetByIdAsync(supplierId))
            .ReturnsAsync((SupplierDatabaseModel?)null);

        var result = await _service.GetSupplierByIdAsync(supplierId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSupplierByIdAsync_RepositoryThrowsException_ReturnsNull()
    {
        var supplierId = 1;

        _mockRepository.Setup(r => r.GetByIdAsync(supplierId))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _service.GetSupplierByIdAsync(supplierId);

        result.Should().BeNull();
    }

    #endregion

    #region CreateSupplierAsync Tests

    [Fact]
    public async Task CreateSupplierAsync_ValidModel_ReturnsId()
    {
        var model = CreateValidSupplierInputModel();
        var createdBy = "user@test.com";
        var expectedId = 123;

        SetupValidationMocks(model);
        _mockRepository.Setup(r => r.CreateAsync(model, createdBy))
            .ReturnsAsync(expectedId);

        var result = await _service.CreateSupplierAsync(model, createdBy);

        result.Should().Be(expectedId);
        _mockRepository.Verify(r => r.CreateAsync(model, createdBy), Times.Once);
    }

    [Fact]
    public async Task CreateSupplierAsync_DuplicateName_ThrowsValidationException()
    {
        var model = CreateValidSupplierInputModel();
        var createdBy = "user@test.com";

        _mockRepository.Setup(r => r.NameExistsAsync(model.Name, null))
            .ReturnsAsync(true);

        await _service.Invoking(s => s.CreateSupplierAsync(model, createdBy))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Validation failed: A supplier with this name already exists");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateSupplierAsync_EmptyName_ThrowsValidationException(string? name)
    {
        var model = CreateValidSupplierInputModel();
        model.Name = name!;
        var createdBy = "user@test.com";

        await _service.Invoking(s => s.CreateSupplierAsync(model, createdBy))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Validation failed: Supplier name is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    public async Task CreateSupplierAsync_InvalidEmail_ThrowsValidationException(string invalidEmail)
    {
        var model = CreateValidSupplierInputModel();
        model.Email = invalidEmail;
        var createdBy = "user@test.com";

        SetupValidationMocks(model, nameExists: false);

        await _service.Invoking(s => s.CreateSupplierAsync(model, createdBy))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Validation failed: Invalid email format");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://invalid.com")]
    [InlineData("invalid://url")]
    public async Task CreateSupplierAsync_InvalidWebsite_ThrowsValidationException(string invalidWebsite)
    {
        var model = CreateValidSupplierInputModel();
        model.Website = invalidWebsite;
        var createdBy = "user@test.com";

        SetupValidationMocks(model, nameExists: false);

        await _service.Invoking(s => s.CreateSupplierAsync(model, createdBy))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Validation failed: Invalid website URL format");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task CreateSupplierAsync_InvalidRating_ThrowsValidationException(int invalidRating)
    {
        var model = CreateValidSupplierInputModel();
        model.Rating = invalidRating;
        var createdBy = "user@test.com";

        SetupValidationMocks(model, nameExists: false);

        await _service.Invoking(s => s.CreateSupplierAsync(model, createdBy))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Validation failed: Rating must be between 1 and 5");
    }

    #endregion

    #region UpdateSupplierStatusAsync Tests

    [Fact]
    public async Task UpdateSupplierStatusAsync_ValidStatusChange_ReturnsTrue()
    {
        var supplierId = 1;
        var newStatus = SupplierStatus.Active;
        var updatedBy = "admin@test.com";

        _mockRepository.Setup(r => r.UpdateStatusAsync(supplierId, newStatus, updatedBy))
            .ReturnsAsync(true);

        var result = await _service.UpdateSupplierStatusAsync(supplierId, newStatus, updatedBy);

        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateStatusAsync(supplierId, newStatus, updatedBy), Times.Once);
    }

    [Fact]
    public async Task UpdateSupplierStatusAsync_RepositoryFails_ReturnsFalse()
    {
        var supplierId = 1;
        var newStatus = SupplierStatus.Active;
        var updatedBy = "admin@test.com";

        _mockRepository.Setup(r => r.UpdateStatusAsync(supplierId, newStatus, updatedBy))
            .ReturnsAsync(false);

        var result = await _service.UpdateSupplierStatusAsync(supplierId, newStatus, updatedBy);

        result.Should().BeFalse();
    }

    #endregion

    #region GetAllSuppliersAsync Tests

    [Fact]
    public async Task GetAllSuppliersAsync_AdministratorRole_ReturnsAllSuppliers()
    {
        var allSuppliers = new List<SupplierDatabaseModel>
        {
            CreateValidSupplierDatabaseModel(id: 1, status: "Active"),
            CreateValidSupplierDatabaseModel(id: 2, status: "Inactive"),
            CreateValidSupplierDatabaseModel(id: 3, status: "Suspended")
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allSuppliers);
        _mockUserContextService.Setup(u => u.GetCurrentUserRole())
            .Returns(UserRole.Administrator);

        var result = await _service.GetAllSuppliersAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllSuppliersAsync_RequestorRole_ReturnsOnlyActiveSuppliers()
    {
        var allSuppliers = new List<SupplierDatabaseModel>
        {
            CreateValidSupplierDatabaseModel(id: 1, status: "Active"),
            CreateValidSupplierDatabaseModel(id: 2, status: "Inactive"),
            CreateValidSupplierDatabaseModel(id: 3, status: "Active")
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allSuppliers);
        _mockUserContextService.Setup(u => u.GetCurrentUserRole())
            .Returns(UserRole.Requestor);

        var result = await _service.GetAllSuppliersAsync();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(s => s.Status == "Active");
    }

    #endregion

    #region CanDeleteSupplierAsync Tests

    [Theory]
    [InlineData("Inactive", true)]
    [InlineData("Pending", true)]
    [InlineData("Active", false)]
    [InlineData("Suspended", false)]
    public async Task CanDeleteSupplierAsync_VariousStatuses_ReturnsExpectedResult(string status, bool expectedResult)
    {
        var supplierId = 1;
        var supplier = CreateValidSupplierDatabaseModel(status: status);

        _mockRepository.Setup(r => r.GetByIdAsync(supplierId))
            .ReturnsAsync(supplier);

        var result = await _service.CanDeleteSupplierAsync(supplierId);

        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task CanDeleteSupplierAsync_SupplierNotFound_ReturnsFalse()
    {
        var supplierId = 999;

        _mockRepository.Setup(r => r.GetByIdAsync(supplierId))
            .ReturnsAsync((SupplierDatabaseModel?)null);

        var result = await _service.CanDeleteSupplierAsync(supplierId);

        result.Should().BeFalse();
    }

    #endregion

    #region SearchSuppliersAsync Tests

    [Fact]
    public async Task SearchSuppliersAsync_ValidSearchTerm_ReturnsMatchingSuppliers()
    {
        var searchTerm = "Acme";
        var matchingSuppliers = new List<SupplierDatabaseModel>
        {
            CreateValidSupplierDatabaseModel(id: 1, name: "Acme Corporation"),
            CreateValidSupplierDatabaseModel(id: 2, name: "Acme Industries")
        };

        _mockRepository.Setup(r => r.SearchByNameAsync(searchTerm))
            .ReturnsAsync(matchingSuppliers);

        var result = await _service.SearchSuppliersAsync(searchTerm);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(s => s.Name.Contains("Acme"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchSuppliersAsync_EmptySearchTerm_ReturnsAllSuppliers(string? searchTerm)
    {
        var allSuppliers = new List<SupplierDatabaseModel>
        {
            CreateValidSupplierDatabaseModel(id: 1),
            CreateValidSupplierDatabaseModel(id: 2)
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allSuppliers);
        _mockUserContextService.Setup(u => u.GetCurrentUserRole())
            .Returns(UserRole.Administrator);

        var result = await _service.SearchSuppliersAsync(searchTerm!);

        result.Should().HaveCount(2);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        _mockRepository.Verify(r => r.SearchByNameAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private SupplierInputModel CreateValidSupplierInputModel()
    {
        return new SupplierInputModel
        {
            Name = "Test Supplier Corp",
            ContactPerson = "John Doe",
            Email = "john@testsupplier.com",
            Phone = "+1-555-0123",
            Address = "123 Business St",
            City = "Business City",
            State = "BC",
            Country = "USA",
            PostalCode = "12345",
            TaxId = "TAX123456",
            PaymentTerms = "Net 30",
            Status = "Active",
            Rating = 4,
            Website = "https://testsupplier.com",
            Notes = "Reliable supplier"
        };
    }

    private SupplierDatabaseModel CreateValidSupplierDatabaseModel(
        int id = 1,
        string name = "Test Supplier Corp",
        string status = "Active")
    {
        return new SupplierDatabaseModel
        {
            Id = id,
            Name = name,
            ContactPerson = "John Doe",
            Email = "john@testsupplier.com",
            Phone = "+1-555-0123",
            Address = "123 Business St",
            City = "Business City",
            State = "BC",
            Country = "USA",
            PostalCode = "12345",
            TaxId = "TAX123456",
            PaymentTerms = "Net 30",
            Status = status,
            Rating = 4,
            Website = "https://testsupplier.com",
            Notes = "Reliable supplier",
            CreatedDate = DateTime.UtcNow.AddDays(-30),
            CreatedBy = "admin@test.com",
            LastUpdated = DateTime.UtcNow.AddDays(-1),
            UpdatedBy = "admin@test.com"
        };
    }

    private void SetupValidationMocks(SupplierInputModel model, bool nameExists = false)
    {
        _mockRepository.Setup(r => r.NameExistsAsync(model.Name, null))
            .ReturnsAsync(nameExists);
    }

    #endregion
}