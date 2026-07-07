using Elattaba.API.Helper;
using Elattba.Application.Common;
using Elattba.Application.Orders;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _orderService.GetAllAsync();
        return ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _orderService.GetByIdAsync(id);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto createOrderDto)
    {
        var result = await _orderService.CreateAsync(createOrderDto);
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.OrderId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return ToActionResult(result);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
    {
        var result = await _orderService.UpdateStatusAsync(id, updateOrderStatusDto);
        return ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _orderService.DeleteAsync(id);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        var response = new ResponseAPI(result.StatusCode, result.Message, result.Data);

        return result.StatusCode switch
        {
            200 => Ok(response),
            404 => NotFound(response),
            >= 500 => Problem(statusCode: result.StatusCode, title: "Internal Server Error", detail: result.Message),
            _ => StatusCode(result.StatusCode, response)
        };
    }

    private IActionResult ToActionResult(ServiceResult result)
    {
        var response = new ResponseAPI(result.StatusCode, result.Message);

        return result.StatusCode switch
        {
            200 => Ok(response),
            404 => NotFound(response),
            >= 500 => Problem(statusCode: result.StatusCode, title: "Internal Server Error", detail: result.Message),
            _ => StatusCode(result.StatusCode, response)
        };
    }
}
