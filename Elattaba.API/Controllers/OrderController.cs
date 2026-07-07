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
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await _orderService.GetAllAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _orderService.GetByIdAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthConstants.BuyerOnlyPolicy)]
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

        return this.ToActionResult(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = AuthConstants.SellerOnlyPolicy)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
    {
        var result = await _orderService.UpdateStatusAsync(id, updateOrderStatusDto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _orderService.DeleteAsync(id);
        return this.ToActionResult(result);
    }

}
