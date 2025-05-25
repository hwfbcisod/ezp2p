using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Enums;
using EasyP2P.Web.Models;
using Npgsql;

namespace EasyP2P.Web.Repositories;

public class PostgresPurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresPurchaseOrderRepository> _logger;

    public PostgresPurchaseOrderRepository(IConfiguration configuration, ILogger<PostgresPurchaseOrderRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("PostgresConnectionString")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'PostgresConnectionString' not found.");
        _logger = logger;
    }

    public async Task<IEnumerable<PurchaseOrderViewModel>> GetAllAsync()
    {
        var orders = new List<PurchaseOrderViewModel>();

        try
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand(
                    "SELECT id, purchase_order_request_id, item_name, quantity, unit_price, " +
                    "total_price, supplier, order_date, created_by, status FROM purchase_orders",
                    connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(MapToViewModel(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all purchase orders");
            throw;
        }

        return orders;
    }

    public async Task<IEnumerable<PurchaseOrderViewModel>> GetByStatusAsync(PurchaseOrderState status)
    {
        var orders = new List<PurchaseOrderViewModel>();

        try
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand(
                    "SELECT id, purchase_order_request_id, item_name, quantity, unit_price, " +
                    "total_price, supplier, order_date, created_by, status " +
                    "FROM purchase_orders WHERE status = @status",
                    connection))
                {
                    command.Parameters.AddWithValue("status", status);

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(MapToViewModel(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving purchase orders with status {Status}", status);
            throw;
        }

        return orders;
    }

    public async Task<PurchaseOrderViewModel?> GetByIdAsync(int id)
    {
        try
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand(
                    "SELECT id, purchase_order_request_id, item_name, quantity, unit_price, " +
                    "total_price, supplier, order_date, created_by, status " +
                    "FROM purchase_orders WHERE id = @id",
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
            _logger.LogError(ex, "Error occurred while retrieving purchase order with ID {Id}", id);
            throw;
        }

        return null;
    }

    public async Task<IEnumerable<PurchaseOrderViewModel>> GetByRequestIdAsync(int requestId)
    {
        var orders = new List<PurchaseOrderViewModel>();

        try
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand(
                    "SELECT id, purchase_order_request_id, item_name, quantity, unit_price, " +
                    "total_price, supplier, order_date, created_by, status " +
                    "FROM purchase_orders WHERE purchase_order_request_id = @request_id",
                    connection))
                {
                    command.Parameters.AddWithValue("request_id", requestId);

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(MapToViewModel(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving purchase orders for request ID {RequestId}", requestId);
            throw;
        }

        return orders;
    }

    public async Task<int> CreateAsync(PurchaseOrderModel model, string createdBy)
    {
        try
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand(
                    "INSERT INTO purchase_orders (purchase_order_request_id, item_name, quantity, " +
                    "unit_price, total_price, supplier, order_date, created_by, status) " +
                    "VALUES (@request_id, @item_name, @quantity, @unit_price, @total_price, " +
                    "@supplier, @order_date, @created_by, @status) " +
                    "RETURNING id",
                    connection))
                {
                    command.Parameters.AddWithValue("request_id", model.PurchaseOrderRequestId);
                    command.Parameters.AddWithValue("item_name", model.ItemName);
                    command.Parameters.AddWithValue("quantity", model.Quantity);
                    command.Parameters.AddWithValue("unit_price", model.UnitPrice);
                    command.Parameters.AddWithValue("total_price", model.TotalPrice);
                    command.Parameters.AddWithValue("supplier", model.Supplier);
                    command.Parameters.AddWithValue("order_date", DateTime.Now);
                    command.Parameters.AddWithValue("created_by", createdBy);
                    command.Parameters.AddWithValue("status", "Created");

                    // Execute the command and get the newly created ID
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating purchase order");
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, PurchaseOrderState status)
    {
        try
        {
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var command = new NpgsqlCommand(
                    "UPDATE purchase_orders SET status = @status WHERE id = @id",
                    connection))
                {
                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("status", Enum.GetName(status)!);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating status for purchase order with ID {Id}", id);
            throw;
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
                    "DELETE FROM purchase_orders WHERE id = @id",
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
            _logger.LogError(ex, "Error occurred while deleting purchase order with ID {Id}", id);
            throw;
        }
    }

    // Helper method to map data reader to view model
    private PurchaseOrderViewModel MapToViewModel(NpgsqlDataReader reader)
    {
        return new PurchaseOrderViewModel
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            PurchaseOrderRequestId = reader.GetInt32(reader.GetOrdinal("purchase_order_request_id")),
            ItemName = reader.GetString(reader.GetOrdinal("item_name")),
            Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
            UnitPrice = reader.GetDecimal(reader.GetOrdinal("unit_price")),
            TotalPrice = reader.GetDecimal(reader.GetOrdinal("total_price")),
            Supplier = reader.GetString(reader.GetOrdinal("supplier")),
            OrderDate = reader.GetDateTime(reader.GetOrdinal("order_date")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            Status = reader.GetString(reader.GetOrdinal("status"))
        };
    }
}