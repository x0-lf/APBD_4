using System.Data;
using APBD_4.Models;
using APBD_4.Data;
using APBD_4.DTOs;
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
    
    public async Task<bool> CheckIfFulfilledOrderExistsAsync(int productId, int amount, DateTime createdAt, SqlConnection connection, SqlTransaction transaction)
    {
        const string query = @"
        SELECT IdOrder
        FROM [Order]
        WHERE IdProduct = @ProductId
          AND Amount = @Amount
          AND CreatedAt < @CreatedAt
          AND FulfilledAt IS NOT NULL";

        using (var cmd = new SqlCommand(query, connection, transaction))
        {
            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@CreatedAt", createdAt);

            var result = await cmd.ExecuteScalarAsync();
            return result != null;
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
    
    public async Task<int?> CallAddProductToWarehouseProcedureAsync(ProductWarehouseRequestDto requestDto)
    {
        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand("AddProductToWarehouse", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@IdProduct", requestDto.IdProduct);
                command.Parameters.AddWithValue("@IdWarehouse", requestDto.IdWarehouse);
                command.Parameters.AddWithValue("@Amount", requestDto.Amount);
                command.Parameters.AddWithValue("@CreatedAt", requestDto.CreatedAt);

                var result = await command.ExecuteScalarAsync();

                if (result == null || !int.TryParse(result.ToString(), out int newId))
                    return null;

                return newId;
            }
        }
    }

}