using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderItemController : BaseController
{
    public OrderItemController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var items = await _unitOfWork.OrderItems.GetAllAsync(item => item.Product!);
            var data = items.Select(item => item.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Order items retrieved successfully", data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var items = await _unitOfWork.OrderItems.GetAllAsync(item => item.Product!);
            var item = items.FirstOrDefault(orderItem => orderItem.OrderItemId == id);
            if (item == null)
            {
                return NotFound(new ResponseAPI(404, "Order item not found."));
            }

            return Ok(new ResponseAPI(200, "Order item retrieved successfully", item.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost("{orderId}")]
    public async Task<IActionResult> Create(int orderId, [FromBody] CreateOrderItemDto createOrderItemDto)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                return NotFound(new ResponseAPI(404, "Order not found."));
            }

            var product = await _unitOfWork.Products.GetByIdAsync(createOrderItemDto.ProductId);
            if (product == null)
            {
                return NotFound(new ResponseAPI(404, "Product not found."));
            }

            var item = new OrderItem
            {
                OrderId = orderId,
                ProductId = createOrderItemDto.ProductId,
                Quantity = createOrderItemDto.Quantity,
                UnitPrice = product.BasePrice,
                Subtotal = product.BasePrice * createOrderItemDto.Quantity
            };

            order.TotalAmount += item.Subtotal;
            await _unitOfWork.OrderItems.AddAsync(item);
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CompleteAsync();

            item.Product = product;
            return CreatedAtAction(
                nameof(GetById),
                new { id = item.OrderItemId },
                new ResponseAPI(201, "Order item created successfully", item.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderItemDto updateOrderItemDto)
    {
        try
        {
            var item = await _unitOfWork.OrderItems.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(new ResponseAPI(404, "Order item not found."));
            }

            var order = await _unitOfWork.Orders.GetByIdAsync(item.OrderId);
            var oldSubtotal = item.Subtotal;

            item.Quantity = updateOrderItemDto.Quantity;
            item.Subtotal = item.UnitPrice * updateOrderItemDto.Quantity;

            if (order != null)
            {
                order.TotalAmount = order.TotalAmount - oldSubtotal + item.Subtotal;
                await _unitOfWork.Orders.UpdateAsync(order);
            }

            await _unitOfWork.OrderItems.UpdateAsync(item);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Order item updated successfully", item.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var item = await _unitOfWork.OrderItems.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(new ResponseAPI(404, "Order item not found."));
            }

            var order = await _unitOfWork.Orders.GetByIdAsync(item.OrderId);
            if (order != null)
            {
                order.TotalAmount -= item.Subtotal;
                await _unitOfWork.Orders.UpdateAsync(order);
            }

            await _unitOfWork.OrderItems.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Order item deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }
}
