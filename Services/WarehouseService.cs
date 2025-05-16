using APBD_4.DTOs;
using APBD_4.Models;
using APBD_4.Repositories;
using APBD_4.Data;
using Microsoft.Data.SqlClient;

namespace APBD_4.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _repository;
    private readonly IDbConnectionFactory _connectionFactory;
    
    public WarehouseService(IWarehouseRepository repository, IDbConnectionFactory connectionFactory)
    {
        _repository = repository;
        _connectionFactory = connectionFactory;
    }

    public async Task<ProductWarehouseResponseDto?> AddProductToWarehouseAsync(ProductWarehouseRequestDto requestDto)
    {
        using (SqlConnection connection = _connectionFactory.CreateConnection())
        {
            await connection.OpenAsync();

            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    if (requestDto.Amount < 0)
                        return null;

                    if (!await _repository.ProductExistsAsync(requestDto.IdProduct, connection, transaction))
                        throw new ArgumentException("Product not found.");

                    if (!await _repository.WarehouseExistsAsync(requestDto.IdWarehouse, connection, transaction))
                        throw new ArgumentException("Warehouse not found.");

                    var order = await _repository.GetMatchingOrderAsync(requestDto.IdProduct, requestDto.Amount, requestDto.CreatedAt, connection, transaction);

                    if (order is null)
                    {
                        var orderConflict = await _repository.CheckIfFulfilledOrderExistsAsync(
                            requestDto.IdProduct, requestDto.Amount, requestDto.CreatedAt, connection, transaction
                        );

                        if (orderConflict)
                            throw new InvalidOperationException("Order already fulfilled or duplicate attempt.");
    
                        throw new ArgumentException("Matching order not found.");
                    }


                    if (await _repository.IsOrderAlreadyProcessedAsync(order.Value.IdOrder, connection, transaction))
                    {
                        throw new InvalidOperationException($"Order {order.Value.IdOrder} already fulfilled.");
                    }

                    await _repository.UpdateOrderFulfillmentDateAsync(order.Value.IdOrder, connection, transaction);

                    var productWarehouse = new ProductWarehouse
                    {
                        IdProduct = requestDto.IdProduct,
                        IdWarehouse = requestDto.IdWarehouse,
                        IdOrder = order.Value.IdOrder,
                        Amount = requestDto.Amount,
                        Price = order.Value.ProductPrice * requestDto.Amount
                    };

                    int idProductWarehouse = await _repository.InsertProductToWarehouseAsync(productWarehouse, connection, transaction);
                    transaction.Commit();

                    return new ProductWarehouseResponseDto
                    {
                        IdProductWarehouse = idProductWarehouse,
                        Summary = $"Inserted product with id: {requestDto.IdProduct} into warehouse with id: {requestDto.IdWarehouse} with order with id: {order.Value.IdOrder}."
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
    
    public async Task<ProductWarehouseResponseDto?> AddProductToWarehouseUsingProcedureAsync(ProductWarehouseRequestDto requestDto)
    {
        try
        {
            var newId = await _repository.CallAddProductToWarehouseProcedureAsync(requestDto);

            if (newId == null)
                return null;

            return new ProductWarehouseResponseDto
            {
                IdProductWarehouse = newId.Value,
                Summary = $"(via procedure) Inserted product {requestDto.IdProduct} into warehouse {requestDto.IdWarehouse}."
            };
        }
        catch (SqlException ex) when (ex.Class >= 11 && ex.Class <= 16)
        {
            throw new ArgumentException($"Stored procedure error: {ex.Message}");
        }
    }
}
