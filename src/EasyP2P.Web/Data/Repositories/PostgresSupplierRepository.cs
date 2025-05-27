using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;
using EasyP2P.Web.Models.Database;
using Npgsql;
using System.Data;

namespace EasyP2P.Web.Data.Repositories;

public class PostgresSupplierRepository : ISupplierRepository
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresSupplierRepository> _logger;

    public PostgresSupplierRepository(IConfiguration configuration, ILogger<PostgresSupplierRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("PostgresConnectionString")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'PostgresConnectionString' not found.");
        _logger = logger;
    }

    public async Task<IEnumerable<SupplierDatabaseModel>> GetAllAsync()
    {
        var suppliers = new List<SupplierDatabaseModel>();

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT id, name, contact_person, email, phone, address, city, state, country, 
                       postal_code, tax_id, payment_terms, status, rating, website, notes,
                       created_date, created_by, last_updated, updated_by
                FROM suppliers 
                ORDER BY name";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                suppliers.Add(MapToDatabaseModel(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all suppliers");
            throw;
        }

        return suppliers;
    }

    public async Task<IEnumerable<SupplierDatabaseModel>> GetByStatusAsync(SupplierStatus status)
    {
        var suppliers = new List<SupplierDatabaseModel>();

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT id, name, contact_person, email, phone, address, city, state, country, 
                       postal_code, tax_id, payment_terms, status, rating, website, notes,
                       created_date, created_by, last_updated, updated_by
                FROM suppliers 
                WHERE status = @status
                ORDER BY name";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("status", Enum.GetName(status));

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                suppliers.Add(MapToDatabaseModel(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving suppliers with status {Status}", status);
            throw;
        }

        return suppliers;
    }

    public async Task<SupplierDatabaseModel?> GetByIdAsync(int id)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT id, name, contact_person, email, phone, address, city, state, country, 
                       postal_code, tax_id, payment_terms, status, rating, website, notes,
                       created_date, created_by, last_updated, updated_by
                FROM suppliers 
                WHERE id = @id";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapToDatabaseModel(reader);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving supplier with ID {Id}", id);
            throw;
        }

        return null;
    }

    public async Task<IEnumerable<SupplierDatabaseModel>> SearchByNameAsync(string searchTerm)
    {
        var suppliers = new List<SupplierDatabaseModel>();

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT id, name, contact_person, email, phone, address, city, state, country, 
                       postal_code, tax_id, payment_terms, status, rating, website, notes,
                       created_date, created_by, last_updated, updated_by
                FROM suppliers 
                WHERE name ILIKE @searchTerm OR contact_person ILIKE @searchTerm
                ORDER BY name";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("searchTerm", $"%{searchTerm}%");

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                suppliers.Add(MapToDatabaseModel(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching suppliers with term {SearchTerm}", searchTerm);
            throw;
        }

        return suppliers;
    }

    public async Task<IEnumerable<SupplierDatabaseModel>> GetByLocationAsync(string? city = null, string? state = null, string? country = null)
    {
        var suppliers = new List<SupplierDatabaseModel>();

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var whereClauses = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrEmpty(city))
            {
                whereClauses.Add("city ILIKE @city");
                parameters.Add(new NpgsqlParameter("city", $"%{city}%"));
            }

            if (!string.IsNullOrEmpty(state))
            {
                whereClauses.Add("state ILIKE @state");
                parameters.Add(new NpgsqlParameter("state", $"%{state}%"));
            }

            if (!string.IsNullOrEmpty(country))
            {
                whereClauses.Add("country ILIKE @country");
                parameters.Add(new NpgsqlParameter("country", $"%{country}%"));
            }

            var whereClause = whereClauses.Any() ? $"WHERE {string.Join(" AND ", whereClauses)}" : "";

            var query = $@"
                SELECT id, name, contact_person, email, phone, address, city, state, country, 
                       postal_code, tax_id, payment_terms, status, rating, website, notes,
                       created_date, created_by, last_updated, updated_by
                FROM suppliers 
                {whereClause}
                ORDER BY name";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddRange(parameters.ToArray());

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                suppliers.Add(MapToDatabaseModel(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving suppliers by location");
            throw;
        }

        return suppliers;
    }

    public async Task<int> CreateAsync(SupplierInputModel model, string createdBy)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                INSERT INTO suppliers (name, contact_person, email, phone, address, city, state, 
                                     country, postal_code, tax_id, payment_terms, status, rating, 
                                     website, notes, created_date, created_by, last_updated, updated_by)
                VALUES (@name, @contact_person, @email, @phone, @address, @city, @state, 
                        @country, @postal_code, @tax_id, @payment_terms, @status, @rating, 
                        @website, @notes, @created_date, @created_by, @last_updated, @updated_by)
                RETURNING id";

            await using var command = new NpgsqlCommand(query, connection);
            var now = DateTime.UtcNow;

            AddSupplierParameters(command, model);
            command.Parameters.AddWithValue("created_date", now);
            command.Parameters.AddWithValue("created_by", createdBy);
            command.Parameters.AddWithValue("last_updated", now);
            command.Parameters.AddWithValue("updated_by", createdBy);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating supplier");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, SupplierInputModel model, string updatedBy)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE suppliers 
                SET name = @name, contact_person = @contact_person, email = @email, phone = @phone,
                    address = @address, city = @city, state = @state, country = @country,
                    postal_code = @postal_code, tax_id = @tax_id, payment_terms = @payment_terms,
                    status = @status, rating = @rating, website = @website, notes = @notes,
                    last_updated = @last_updated, updated_by = @updated_by
                WHERE id = @id";

            await using var command = new NpgsqlCommand(query, connection);

            AddSupplierParameters(command, model);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("last_updated", DateTime.UtcNow);
            command.Parameters.AddWithValue("updated_by", updatedBy);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating supplier with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, SupplierStatus status, string updatedBy)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE suppliers 
                SET status = @status, last_updated = @last_updated, updated_by = @updated_by
                WHERE id = @id";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("status", Enum.GetName(status));
            command.Parameters.AddWithValue("last_updated", DateTime.UtcNow);
            command.Parameters.AddWithValue("updated_by", updatedBy);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating supplier status for ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM suppliers WHERE id = @id";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting supplier with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(*) FROM suppliers WHERE name = @name";

            if (excludeId.HasValue)
            {
                query += " AND id != @excludeId";
            }

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("name", name);

            if (excludeId.HasValue)
            {
                command.Parameters.AddWithValue("excludeId", excludeId.Value);
            }

            var count = await command.ExecuteScalarAsync();
            return Convert.ToInt32(count) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if supplier name exists");
            throw;
        }
    }

    public async Task<IEnumerable<SupplierDatabaseModel>> GetTopRatedSuppliersAsync(int minRating = 4)
    {
        var suppliers = new List<SupplierDatabaseModel>();

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT id, name, contact_person, email, phone, address, city, state, country, 
                       postal_code, tax_id, payment_terms, status, rating, website, notes,
                       created_date, created_by, last_updated, updated_by
                FROM suppliers 
                WHERE rating >= @minRating AND status = 'Active'
                ORDER BY rating DESC, name";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("minRating", minRating);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                suppliers.Add(MapToDatabaseModel(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving top-rated suppliers");
            throw;
        }

        return suppliers;
    }

    public async Task<SupplierStatistics> GetStatisticsAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    COUNT(*) as total_suppliers,
                    COUNT(CASE WHEN status = 'Active' THEN 1 END) as active_suppliers,
                    COUNT(CASE WHEN status = 'Inactive' THEN 1 END) as inactive_suppliers,
                    COUNT(CASE WHEN status = 'Pending' THEN 1 END) as pending_suppliers,
                    COUNT(CASE WHEN status = 'Suspended' THEN 1 END) as suspended_suppliers,
                    AVG(rating) as average_rating,
                    COUNT(CASE WHEN rating >= 4 THEN 1 END) as high_rated_suppliers
                FROM suppliers";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new SupplierStatistics
                {
                    TotalSuppliers = reader.GetInt32("total_suppliers"),
                    ActiveSuppliers = reader.GetInt32("active_suppliers"),
                    InactiveSuppliers = reader.GetInt32("inactive_suppliers"),
                    PendingSuppliers = reader.GetInt32("pending_suppliers"),
                    SuspendedSuppliers = reader.GetInt32("suspended_suppliers"),
                    AverageRating = reader.IsDBNull("average_rating") ? 0 : (double)reader.GetDecimal("average_rating"),
                    HighRatedSuppliers = reader.GetInt32("high_rated_suppliers")
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving supplier statistics");
            throw;
        }

        return new SupplierStatistics();
    }

    private void AddSupplierParameters(NpgsqlCommand command, SupplierInputModel model)
    {
        command.Parameters.AddWithValue("name", model.Name);
        command.Parameters.AddWithValue("contact_person", model.ContactPerson ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("email", model.Email ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("phone", model.Phone ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("address", model.Address ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("city", model.City ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("state", model.State ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("country", model.Country ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("postal_code", model.PostalCode ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("tax_id", model.TaxId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("payment_terms", model.PaymentTerms ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("status", model.Status);
        command.Parameters.AddWithValue("rating", model.Rating ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("website", model.Website ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("notes", model.Notes ?? (object)DBNull.Value);
    }

    private SupplierDatabaseModel MapToDatabaseModel(NpgsqlDataReader reader)
    {
        return new SupplierDatabaseModel
        {
            Id = reader.GetInt32("id"),
            Name = reader.GetString("name"),
            ContactPerson = reader.IsDBNull("contact_person") ? "" : reader.GetString("contact_person"),
            Email = reader.IsDBNull("email") ? "" : reader.GetString("email"),
            Phone = reader.IsDBNull("phone") ? "" : reader.GetString("phone"),
            Address = reader.IsDBNull("address") ? "" : reader.GetString("address"),
            City = reader.IsDBNull("city") ? "" : reader.GetString("city"),
            State = reader.IsDBNull("state") ? "" : reader.GetString("state"),
            Country = reader.IsDBNull("country") ? "" : reader.GetString("country"),
            PostalCode = reader.IsDBNull("postal_code") ? "" : reader.GetString("postal_code"),
            TaxId = reader.IsDBNull("tax_id") ? "" : reader.GetString("tax_id"),
            PaymentTerms = reader.IsDBNull("payment_terms") ? "" : reader.GetString("payment_terms"),
            Status = reader.GetString("status"),
            Rating = reader.IsDBNull("rating") ? null : reader.GetInt32("rating"),
            Website = reader.IsDBNull("website") ? "" : reader.GetString("website"),
            Notes = reader.IsDBNull("notes") ? "" : reader.GetString("notes"),
            CreatedDate = reader.GetDateTime("created_date"),
            CreatedBy = reader.GetString("created_by"),
            LastUpdated = reader.GetDateTime("last_updated"),
            UpdatedBy = reader.GetString("updated_by")
        };
    }
}
