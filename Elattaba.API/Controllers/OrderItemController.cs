using Elattaba.API.Helper;
using Elattba.Application.Auth;
using Elattba.Application.Common;
using Elattba.Application.Orders;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await _orderItemService.GetAllAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _orderItemService.GetByIdAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPost("{orderId}")]
    [Authorize(Policy = AuthConstants.BuyerOnlyPolicy)]
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

        return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AuthConstants.BuyerOnlyPolicy)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderItemDto updateOrderItemDto)
    {
        var result = await _orderItemService.UpdateAsync(id, updateOrderItemDto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthConstants.BuyerOnlyPolicy)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _orderItemService.DeleteAsync(id);
        return this.ToActionResult(result);
    }

}
