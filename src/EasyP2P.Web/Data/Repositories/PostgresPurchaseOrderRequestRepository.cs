
// Repositories/SqlPurchaseOrderRequestRepository.cs
using EasyP2P.Web.Models;
using EasyP2P.Web.Data.Repositories.Interfaces;
using Npgsql;

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

    public async Task<IEnumerable<PurchaseOrderRequestViewModel>> GetAllAsync()
    {
        var requests = new List<PurchaseOrderRequestViewModel>();

        try
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand(
                    "SELECT id, item_name, quantity, comment, request_date, requested_by, status FROM purchase_order_requests",
                    connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            requests.Add(MapToViewModel(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all purchase order requests");
            throw;
        }

        return requests;
    }

    public async Task<IEnumerable<PurchaseOrderRequestViewModel>> GetByStatusAsync(string status)
    {
        var requests = new List<PurchaseOrderRequestViewModel>();

        try
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand(
                    "SELECT id, item_name, quantity, comment, request_date, requested_by, status " +
                    "FROM purchase_order_requests WHERE status = @status",
                    connection))
                {
                    command.Parameters.AddWithValue("status", status);

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            requests.Add(MapToViewModel(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving purchase order requests with status {Status}", status);
            throw;
        }

        return requests;
    }

    public async Task<PurchaseOrderRequestViewModel?> GetByIdAsync(int id)
    {
        try
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand(
                    "SELECT id, item_name, quantity, comment, request_date, requested_by, status " +
                    "FROM purchase_order_requests WHERE id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("id", id);

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapToViewModel(reader);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving purchase order request with ID {Id}", id);
            throw;
        }

        return null;
    }

    public async Task CreateAsync(PurchaseOrderRequestInputModel model, string requestedBy)
    {
        try
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand(
                    "INSERT INTO purchase_order_requests (item_name, quantity, comment, request_date, requested_by, status) " +
                    "VALUES (@item_name, @quantity, @comment, @request_date, @requested_by, @status) " +
                    "RETURNING id",
                    connection))
                {
                    command.Parameters.AddWithValue("item_name", model.ItemName);
                    command.Parameters.AddWithValue("quantity", model.Quantity);
                    command.Parameters.AddWithValue("comment", model.Comment ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("request_date", DateTime.Now);
                    command.Parameters.AddWithValue("requested_by", requestedBy);
                    command.Parameters.AddWithValue("status", "Pending");

                    // Execute the command and get the newly created ID
                    var result = await command.ExecuteNonQueryAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating purchase order request");
            throw;
        }
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        await using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            await using (var command = new NpgsqlCommand(
                "UPDATE purchase_order_requests SET status = @status WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("id", id);
                command.Parameters.AddWithValue("status", status);

                int rowsAffected = await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand(
                    "DELETE FROM purchase_order_requests WHERE id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("id", id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting purchase order request with ID {Id}", id);
            throw;
        }
    }

    // Helper method to map data reader to view model
    private PurchaseOrderRequestViewModel MapToViewModel(NpgsqlDataReader reader)
    {
        return new PurchaseOrderRequestViewModel
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            ItemName = reader.GetString(reader.GetOrdinal("item_name")),
            Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
            Comment = reader.IsDBNull(reader.GetOrdinal("comment")) ? string.Empty : reader.GetString(reader.GetOrdinal("comment")),
            RequestDate = reader.GetDateTime(reader.GetOrdinal("request_date")),
            RequestedBy = reader.GetString(reader.GetOrdinal("requested_by")),
            Status = reader.GetString(reader.GetOrdinal("status"))
        };
    }
}