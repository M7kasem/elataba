using ElAtaba.Domain.Entities;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.Orders;

public sealed class OrderItemService : IOrderItemService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderItemService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<IReadOnlyList<OrderItemDto>>> GetAllAsync()
    {
        try
        {
            var items = await _unitOfWork.OrderItems.GetAllAsync(item => item.Product!);
            var data = items.Select(item => item.ToOrderItemDto()).ToList();

            return new ServiceResult<IReadOnlyList<OrderItemDto>>(true, 200, "Order items retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<OrderItemDto>>(ex);
        }
    }

    public async Task<ServiceResult<OrderItemDto>> GetByIdAsync(int id)
    {
        try
        {
            var item = await _unitOfWork.OrderItems.GetFirstOrDefaultAsync(
                orderItem => orderItem.OrderItemId == id,
                true,
                orderItem => orderItem.Product!);
            if (item == null)
            {
                return new ServiceResult<OrderItemDto>(false, 404, "Order item not found.");
            }

            return new ServiceResult<OrderItemDto>(true, 200, "Order item retrieved successfully", item.ToOrderItemDto());
        }
        catch (Exception ex)
        {
            return Failure<OrderItemDto>(ex);
        }
    }

    public async Task<ServiceResult<OrderItemDto>> CreateAsync(int orderId, CreateOrderItemDto dto)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                return new ServiceResult<OrderItemDto>(false, 404, "Order not found.");
            }

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                return new ServiceResult<OrderItemDto>(false, 404, "Product not found.");
            }

            var item = new OrderItem
            {
                OrderId = orderId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = product.BasePrice,
                Subtotal = product.BasePrice * dto.Quantity
            };

            order.TotalAmount += item.Subtotal;
            await _unitOfWork.OrderItems.AddAsync(item);
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CompleteAsync();

            item.Product = product;
            return new ServiceResult<OrderItemDto>(true, 201, "Order item created successfully", item.ToOrderItemDto());
        }
        catch (Exception ex)
        {
            return Failure<OrderItemDto>(ex);
        }
    }

    public async Task<ServiceResult<OrderItemDto>> UpdateAsync(int id, UpdateOrderItemDto dto)
    {
        try
        {
            var item = await _unitOfWork.OrderItems.GetByIdAsync(id);
            if (item == null)
            {
                return new ServiceResult<OrderItemDto>(false, 404, "Order item not found.");
            }

            var order = await _unitOfWork.Orders.GetByIdAsync(item.OrderId);
            var oldSubtotal = item.Subtotal;

            item.Quantity = dto.Quantity;
            item.Subtotal = item.UnitPrice * dto.Quantity;

            if (order != null)
            {
                order.TotalAmount = order.TotalAmount - oldSubtotal + item.Subtotal;
                await _unitOfWork.Orders.UpdateAsync(order);
            }

            await _unitOfWork.OrderItems.UpdateAsync(item);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<OrderItemDto>(true, 200, "Order item updated successfully", item.ToOrderItemDto());
        }
        catch (Exception ex)
        {
            return Failure<OrderItemDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var item = await _unitOfWork.OrderItems.GetByIdAsync(id);
            if (item == null)
            {
                return new ServiceResult(false, 404, "Order item not found.");
            }

            var order = await _unitOfWork.Orders.GetByIdAsync(item.OrderId);
            if (order != null)
            {
                order.TotalAmount -= item.Subtotal;
                await _unitOfWork.Orders.UpdateAsync(order);
            }

            await _unitOfWork.OrderItems.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "Order item deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");
}
