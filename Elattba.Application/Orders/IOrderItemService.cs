using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Orders;

public interface IOrderItemService
{
    Task<ServiceResult<IReadOnlyList<OrderItemDto>>> GetAllAsync();
    Task<ServiceResult<OrderItemDto>> GetByIdAsync(int id);
    Task<ServiceResult<OrderItemDto>> CreateAsync(int orderId, CreateOrderItemDto dto);
    Task<ServiceResult<OrderItemDto>> UpdateAsync(int id, UpdateOrderItemDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
