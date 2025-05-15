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
        try
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            var result = await _warehouseService.AddProductToWarehouseAsync(request);

            if (result == null)
                return NotFound("Invalid Product ID, Warehouse ID, or no matching order.");

            if (result.IdProductWarehouse == -1)
                return Conflict(result.Summary);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}