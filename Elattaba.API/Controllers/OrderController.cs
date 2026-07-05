using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : BaseController
{
    public OrderController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var orders = await _unitOfWork.Orders.GetAllAsync(
                order => order.Buyer!,
                order => order.Store!,
                order => order.Carrier!,
                order => order.OrderItems);
            var data = orders.Select(order => order.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Orders retrieved successfully", data));
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
            var orders = await _unitOfWork.Orders.GetAllAsync(
                order => order.Buyer!,
                order => order.Store!,
                order => order.Carrier!,
                order => order.OrderItems);
            var order = orders.FirstOrDefault(item => item.OrderId == id);
            if (order == null)
            {
                return NotFound(new ResponseAPI(404, "Order not found."));
            }

            return Ok(new ResponseAPI(200, "Order retrieved successfully", order.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto createOrderDto)
    {
        try
        {
            var buyer = await _unitOfWork.Users.GetByIdAsync(createOrderDto.BuyerId);
            if (buyer == null)
            {
                return NotFound(new ResponseAPI(404, "Buyer not found."));
            }

            var store = await _unitOfWork.Stores.GetByIdAsync(createOrderDto.StoreId);
            if (store == null)
            {
                return NotFound(new ResponseAPI(404, "Store not found."));
            }

            Carrier? carrier = null;
            if (createOrderDto.CarrierId.HasValue)
            {
                carrier = await _unitOfWork.Carriers.GetByIdAsync(createOrderDto.CarrierId.Value);
                if (carrier == null)
                {
                    return NotFound(new ResponseAPI(404, "Carrier not found."));
                }
            }

            var order = new Order
            {
                BuyerId = createOrderDto.BuyerId,
                StoreId = createOrderDto.StoreId,
                CarrierId = createOrderDto.CarrierId,
                ShippingAddressSnapshot = createOrderDto.ShippingAddressSnapshot,
                PaymentMethod = createOrderDto.PaymentMethod
            };

            foreach (var itemDto in createOrderDto.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    return NotFound(new ResponseAPI(404, $"Product {itemDto.ProductId} not found."));
                }

                var subtotal = product.BasePrice * itemDto.Quantity;
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.BasePrice,
                    Subtotal = subtotal
                });
                order.TotalAmount += subtotal;
            }

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            order.Buyer = buyer;
            order.Store = store;
            order.Carrier = carrier;
            return CreatedAtAction(
                nameof(GetById),
                new { id = order.OrderId },
                new ResponseAPI(201, "Order created successfully", order.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(new ResponseAPI(404, "Order not found."));
            }

            if (updateOrderStatusDto.CarrierId.HasValue)
            {
                var carrier = await _unitOfWork.Carriers.GetByIdAsync(updateOrderStatusDto.CarrierId.Value);
                if (carrier == null)
                {
                    return NotFound(new ResponseAPI(404, "Carrier not found."));
                }
            }

            order.CarrierId = updateOrderStatusDto.CarrierId;
            order.ShippingCost = updateOrderStatusDto.ShippingCost;
            order.TrackingNumber = updateOrderStatusDto.TrackingNumber;
            order.PaymentStatus = updateOrderStatusDto.PaymentStatus;
            order.Status = updateOrderStatusDto.Status;

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Order status updated successfully", order.ToDto()));
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
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(new ResponseAPI(404, "Order not found."));
            }

            await _unitOfWork.Orders.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Order deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }
}
