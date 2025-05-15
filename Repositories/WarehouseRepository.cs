using APBD_4.Models;
using APBD_4.Data;
using Microsoft.Data.SqlClient;

namespace APBD_4.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public WarehouseRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> ProductExistsAsync(int productId, SqlConnection connection, SqlTransaction transaction)
    {
        var query = "SELECT IdProduct FROM Product WHERE IdProduct = @IdProduct";

        using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
        {
            cmd.Parameters.AddWithValue("@IdProduct", productId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }
    }

    public async Task<bool> WarehouseExistsAsync(int warehouseId, SqlConnection connection, SqlTransaction transaction)
    {
        var query = "SELECT IdWarehouse FROM Warehouse WHERE IdWarehouse = @IdWarehouse";

        using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
        {
            cmd.Parameters.AddWithValue("@IdWarehouse", warehouseId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }
    }

    public async Task<(int IdOrder, decimal ProductPrice)?> GetMatchingOrderAsync(int productId, int amount,
        DateTime requestCreatedAt, SqlConnection connection, SqlTransaction transaction)
    {
        var query = @"
            SELECT TOP 1 o.IdOrder, p.Price
            FROM [Order] o
            JOIN Product p ON o.IdProduct = p.IdProduct
            WHERE o.IdProduct = @ProductId
              AND o.Amount = @Amount
              AND o.CreatedAt < @RequestCreatedAt
              AND o.FulfilledAt IS NULL";
        
        using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
        {
            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@RequestCreatedAt", requestCreatedAt);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return (reader.GetInt32(0), reader.GetDecimal(1));
                }
            }
        }

        return null;
    }

    public async Task<bool> IsOrderAlreadyProcessedAsync(int orderId, SqlConnection connection,
        SqlTransaction transaction)
    {
        var query = "SELECT IdOrder FROM Product_Warehouse WHERE IdOrder = @OrderId";

        using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
        {
            cmd.Parameters.AddWithValue("@OrderId", orderId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }
    }

    public async Task UpdateOrderFulfillmentDateAsync(int orderId, SqlConnection connection, SqlTransaction transaction)
    {
        var query = "UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @OrderId";

        using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
        {
            cmd.Parameters.AddWithValue("@OrderId", orderId);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task<int> InsertProductToWarehouseAsync(ProductWarehouse productWarehouse, SqlConnection connection,
        SqlTransaction transaction)
    {
        var query = @"
            INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, GETDATE());
            SELECT SCOPE_IDENTITY();";

        using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
        {
            cmd.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
            cmd.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
            cmd.Parameters.AddWithValue("@IdOrder", productWarehouse.IdOrder);
            cmd.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
            cmd.Parameters.AddWithValue("@Price", productWarehouse.Price);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }
}