using APBD_4.DTOs;
using APBD_4.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_4.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] ProductWarehouseRequestDto request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        try
        {
            var result = await _warehouseService.AddProductToWarehouseAsync(request);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    
    [HttpPost("proc")]
    public async Task<IActionResult> AddProductToWarehouseViaProcedure([FromBody] ProductWarehouseRequestDto request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than 0");

        try
        {
            var result = await _warehouseService.AddProductToWarehouseUsingProcedureAsync(request);

            if (result == null)
                return NotFound("Invalid Product ID, Warehouse ID, or no matching order.");

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            var message = ex.Message.ToLower();

            if (message.Contains("no order to fullfill") || message.Contains("no order to fulfill"))
                return Conflict("Order already fulfilled or invalid.");

            if (message.Contains("idproduct") || message.Contains("idwarehouse"))
                return NotFound(ex.Message);

            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error: {ex.Message}");
        }
    }


}