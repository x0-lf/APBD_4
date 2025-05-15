using APBD_4.Models;
using Microsoft.Data.SqlClient;

namespace APBD_4.Repositories;

public interface IWarehouseRepository
{
    Task<bool> ProductExistsAsync
        (int productId, SqlConnection connection, SqlTransaction transaction);
   
    Task<bool> WarehouseExistsAsync
        (int warehouseId, SqlConnection connection, SqlTransaction transaction);
    
    Task<(int IdOrder, decimal ProductPrice)?> GetMatchingOrderAsync
        (int productId, int amount, DateTime requestCreatedAt, SqlConnection connection, SqlTransaction transaction);
    
    Task<bool> IsOrderAlreadyProcessedAsync
        (int orderId, SqlConnection connection, SqlTransaction transaction);
    
    Task UpdateOrderFulfillmentDateAsync
        (int orderId, SqlConnection connection, SqlTransaction transaction);
    
    Task<int> InsertProductToWarehouseAsync
        (ProductWarehouse productWarehouse, SqlConnection connection, SqlTransaction transaction);
}