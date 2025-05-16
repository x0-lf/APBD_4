using APBD_4.DTOs;
using Microsoft.Data.SqlClient;

namespace APBD_4.Services;

public interface IWarehouseService
{
    Task<ProductWarehouseResponseDto?> AddProductToWarehouseAsync(ProductWarehouseRequestDto requestDto);
    
    Task<ProductWarehouseResponseDto?> AddProductToWarehouseUsingProcedureAsync(ProductWarehouseRequestDto requestDto);


}

