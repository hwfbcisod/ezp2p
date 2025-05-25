using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;
using EasyP2P.Web.Models.Database;

namespace EasyP2P.Web.Data.Repositories.Interfaces;

public interface ISupplierRepository
{
    // Get all suppliers
    Task<IEnumerable<SupplierDatabaseModel>> GetAllAsync();

    // Get suppliers by status
    Task<IEnumerable<SupplierDatabaseModel>> GetByStatusAsync(SupplierStatus status);

    // Get a single supplier by ID
    Task<SupplierDatabaseModel?> GetByIdAsync(int id);

    // Search suppliers by name
    Task<IEnumerable<SupplierDatabaseModel>> SearchByNameAsync(string searchTerm);

    // Get suppliers by location (city, state, country)
    Task<IEnumerable<SupplierDatabaseModel>> GetByLocationAsync(string? city = null, string? state = null, string? country = null);

    // Create a new supplier
    Task<int> CreateAsync(SupplierInputModel model, string createdBy);

    // Update an existing supplier
    Task<bool> UpdateAsync(int id, SupplierInputModel model, string updatedBy);

    // Update supplier status
    Task<bool> UpdateStatusAsync(int id, SupplierStatus status, string updatedBy);

    // Delete a supplier
    Task<bool> DeleteAsync(int id);

    // Check if supplier name exists
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    // Get suppliers with ratings above threshold
    Task<IEnumerable<SupplierDatabaseModel>> GetTopRatedSuppliersAsync(int minRating = 4);

    // Get supplier statistics
    Task<SupplierStatistics> GetStatisticsAsync();
}