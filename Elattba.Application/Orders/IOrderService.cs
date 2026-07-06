using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Orders;

public interface IOrderService
{
    Task<ServiceResult<IReadOnlyList<OrderDto>>> GetAllAsync();
    Task<ServiceResult<OrderDto>> GetByIdAsync(int id);
    Task<ServiceResult<OrderDto>> CreateAsync(CreateOrderDto dto);
    Task<ServiceResult<OrderDto>> UpdateStatusAsync(int id, UpdateOrderStatusDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
