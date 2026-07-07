using Elattaba.API.Helper;
using Elattba.Application.Common;
using Elattba.Application.Orders;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderItemController : ControllerBase
{
    private readonly IOrderItemService _orderItemService;

    public OrderItemController(IOrderItemService orderItemService)
    {
        _orderItemService = orderItemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _orderItemService.GetAllAsync();
        return ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _orderItemService.GetByIdAsync(id);
        return ToActionResult(result);
    }

    [HttpPost("{orderId}")]
    public async Task<IActionResult> Create(int orderId, [FromBody] CreateOrderItemDto createOrderItemDto)
    {
        var result = await _orderItemService.CreateAsync(orderId, createOrderItemDto);
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.OrderItemId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderItemDto updateOrderItemDto)
    {
        var result = await _orderItemService.UpdateAsync(id, updateOrderItemDto);
        return ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _orderItemService.DeleteAsync(id);
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
