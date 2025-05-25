using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;
using EasyP2P.Web.Models.Database;
using Npgsql;
using System.Data;

namespace EasyP2P.Web.Data.Repositories;

public class PostgresPurchaseOrderRequestRepository : IPurchaseOrderRequestRepository
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresPurchaseOrderRequestRepository> _logger;

    public PostgresPurchaseOrderRequestRepository(IConfiguration configuration, ILogger<PostgresPurchaseOrderRequestRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("PostgresConnectionString")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'PostgresConnectionString' not found.");
        _logger = logger;
    }

    public async Task<IEnumerable<PurchaseOrderRequestDatabaseModel>> GetAllAsync()
    {
        var requests = new List<PurchaseOrderRequestDatabaseModel>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
                SELECT id, item_name, quantity, comment, request_date, requested_by, status,
                       COALESCE(last_updated, request_date) as last_updated,
                       COALESCE(updated_by, requested_by) as updated_by,
                       COALESCE(justification, '') as justification,
                       COALESCE(priority, 'Medium') as priority,
                       COALESCE(department, '') as department,
                       COALESCE(budget_code, '') as budget_code,
                       expected_delivery_date
                FROM purchase_order_requests 
                ORDER BY request_date DESC";

        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            requests.Add(MapToDatabaseModel(reader));
        }

        return requests;
    }

    public async Task<IEnumerable<PurchaseOrderRequestDatabaseModel>> GetByStatusAsync(PurchaseOrderRequestState status)
    {
        var requests = new List<PurchaseOrderRequestDatabaseModel>();

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT id, item_name, quantity, comment, request_date, requested_by, status,
                       COALESCE(last_updated, request_date) as last_updated,
                       COALESCE(updated_by, requested_by) as updated_by,
                       COALESCE(justification, '') as justification,
                       COALESCE(priority, 'Medium') as priority,
                       COALESCE(department, '') as department,
                       COALESCE(budget_code, '') as budget_code,
                       expected_delivery_date
                FROM purchase_order_requests 
                WHERE status = @status 
                ORDER BY request_date DESC";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("status", Enum.GetName(status));

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                requests.Add(MapToDatabaseModel(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving purchase order requests with status {Status}", status);
            throw;
        }

        return requests;
    }

    public async Task<PurchaseOrderRequestDatabaseModel?> GetByIdAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
                SELECT id, item_name, quantity, comment, request_date, requested_by, status,
                       COALESCE(last_updated, request_date) as last_updated,
                       COALESCE(updated_by, requested_by) as updated_by,
                       COALESCE(justification, '') as justification,
                       COALESCE(priority, 'Medium') as priority,
                       COALESCE(department, '') as department,
                       COALESCE(budget_code, '') as budget_code,
                       expected_delivery_date
                FROM purchase_order_requests 
                WHERE id = @id";

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToDatabaseModel(reader);
        }

        return null;
    }

    public async Task<int> CreateAsync(PurchaseOrderRequestInputModel model, string requestedBy)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var enhancedQuery = @"
                    INSERT INTO purchase_order_requests 
                    (item_name, quantity, comment, request_date, requested_by, status, 
                     last_updated, updated_by, justification, priority, department, 
                     budget_code, expected_delivery_date) 
                    VALUES (@item_name, @quantity, @comment, @request_date, @requested_by, 
                            @status, @last_updated, @updated_by, @justification, @priority, 
                            @department, @budget_code, @expected_delivery_date) 
                    RETURNING id";

        await using var command = new NpgsqlCommand(enhancedQuery, connection);

        var now = DateTime.UtcNow;
        command.Parameters.AddWithValue("item_name", model.ItemName);
        command.Parameters.AddWithValue("quantity", model.Quantity);
        command.Parameters.AddWithValue("comment", model.Comment ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("request_date", now);
        command.Parameters.AddWithValue("requested_by", requestedBy);
        command.Parameters.AddWithValue("status", "PendingApproval");
        command.Parameters.AddWithValue("last_updated", now);
        command.Parameters.AddWithValue("updated_by", requestedBy);
        command.Parameters.AddWithValue("justification", model.Justification ?? "");
        command.Parameters.AddWithValue("priority", model.Priority ?? "Medium");
        command.Parameters.AddWithValue("department", model.Department ?? "");
        command.Parameters.AddWithValue("budget_code", model.BudgetCode ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("expected_delivery_date", model.ExpectedDeliveryDate ?? (object)DBNull.Value);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateStatusAsync(int id, PurchaseOrderRequestState status, string updatedBy)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Try enhanced update first
            try
            {
                await using var command = new NpgsqlCommand(@"
                    UPDATE purchase_order_requests 
                    SET status = @status, last_updated = @last_updated, updated_by = @updated_by 
                    WHERE id = @id", connection);

                command.Parameters.AddWithValue("id", id);
                command.Parameters.AddWithValue("status", Enum.GetName(status));
                command.Parameters.AddWithValue("last_updated", DateTime.UtcNow);
                command.Parameters.AddWithValue("updated_by", updatedBy);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch
            {
                // Fall back to basic update
                await using var command = new NpgsqlCommand(
                    "UPDATE purchase_order_requests SET status = @status WHERE id = @id", connection);

                command.Parameters.AddWithValue("id", id);
                command.Parameters.AddWithValue("status", status);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating status for purchase order request with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> CanTransitionToStatus(int id, PurchaseOrderRequestState newStatus)
    {
        var request = await GetByIdAsync(id);
        if (request == null) return false;

        return newStatus switch
        {
            PurchaseOrderRequestState.PendingApproval => request.Status == nameof(PurchaseOrderRequestState.Created),
            PurchaseOrderRequestState.Approved => request.Status == nameof(PurchaseOrderRequestState.PendingApproval) || request.Status == "Pending",
            PurchaseOrderRequestState.Rejected => request.Status == nameof(PurchaseOrderRequestState.PendingApproval) || request.Status == "Pending",
            PurchaseOrderRequestState.PurchaseOrderCreated => request.Status == nameof(PurchaseOrderRequestState.Approved),
            PurchaseOrderRequestState.Cancelled => request.Status != nameof(PurchaseOrderRequestState.Rejected)
                                                    && request.Status != nameof(PurchaseOrderRequestState.Cancelled)
                                                    && request.Status != nameof(PurchaseOrderRequestState.PurchaseOrderCreated),
            _ => false
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(
                "DELETE FROM purchase_order_requests WHERE id = @id", connection);

            command.Parameters.AddWithValue("id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting purchase order request with ID {Id}", id);
            throw;
        }
    }

    private PurchaseOrderRequestDatabaseModel MapToDatabaseModel(NpgsqlDataReader reader)
    {
        return new PurchaseOrderRequestDatabaseModel
        {
            Id = reader.GetInt32("id"),
            ItemName = reader.GetString("item_name"),
            Quantity = reader.GetInt32("quantity"),
            Comment = reader.IsDBNull("comment") ? string.Empty : reader.GetString("comment"),
            RequestDate = reader.GetDateTime("request_date"),
            RequestedBy = reader.GetString("requested_by"),
            Status = reader.GetString("status"),
            LastUpdated = reader.GetDateTime("last_updated"),
            UpdatedBy = reader.GetString("updated_by"),
            Justification = reader.GetString("justification"),
            Priority = reader.GetString("priority"),
            Department = reader.GetString("department"),
            BudgetCode = reader.IsDBNull("budget_code") ? string.Empty : reader.GetString("budget_code"),
            ExpectedDeliveryDate = reader.IsDBNull("expected_delivery_date") ? null : reader.GetDateTime("expected_delivery_date")
        };
    }
}