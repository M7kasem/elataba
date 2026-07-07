using ElAtaba.Domain.Entities;
using Elattba.Application.Auth;
using Elattba.Application.Common;
using Elattba.Application.Offers;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.Orders;

public sealed class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public OrderService(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<IReadOnlyList<OrderDto>>> GetAllAsync()
    {
        try
        {
            var orders = await GetOrdersWithDetailsAsync();
            var data = orders.Select(order => order.ToOrderDto()).ToList();

            return new ServiceResult<IReadOnlyList<OrderDto>>(true, 200, "Orders retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<OrderDto>>(ex);
        }
    }

    public async Task<ServiceResult<OrderDto>> GetByIdAsync(int id)
    {
        try
        {
            var order = await GetOrderWithDetailsAsync(id);
            if (order == null)
            {
                return new ServiceResult<OrderDto>(false, 404, "Order not found.");
            }

            return new ServiceResult<OrderDto>(true, 200, "Order retrieved successfully", order.ToOrderDto());
        }
        catch (Exception ex)
        {
            return Failure<OrderDto>(ex);
        }
    }

    public async Task<ServiceResult<OrderDto>> CreateAsync(CreateOrderDto dto)
    {
        try
        {
            if (dto.OrderItems == null || dto.OrderItems.Count == 0)
            {
                return new ServiceResult<OrderDto>(false, 400, "Order must contain at least one item.");
            }

            if (dto.OrderItems.Any(item => item.Quantity <= 0))
            {
                return new ServiceResult<OrderDto>(false, 400, "Order item quantity must be greater than zero.");
            }

            if (dto.OrderItems.Any(item => item.ProductId <= 0))
            {
                return new ServiceResult<OrderDto>(false, 400, "Order item product id must be greater than zero.");
            }

            var buyerAuthorizationError = EnsureCurrentBuyer(dto.BuyerId);
            if (buyerAuthorizationError != null)
            {
                return buyerAuthorizationError;
            }

            var buyer = await _unitOfWork.Users.GetByIdAsync(dto.BuyerId);
            if (buyer == null)
            {
                return new ServiceResult<OrderDto>(false, 404, "Buyer not found.");
            }

            var store = await _unitOfWork.Stores.GetByIdAsync(dto.StoreId);
            if (store == null)
            {
                return new ServiceResult<OrderDto>(false, 404, "Store not found.");
            }

            Carrier? carrier = null;
            if (dto.CarrierId.HasValue)
            {
                carrier = await _unitOfWork.Carriers.GetByIdAsync(dto.CarrierId.Value);
                if (carrier == null)
                {
                    return new ServiceResult<OrderDto>(false, 404, "Carrier not found.");
                }
            }

            var requestedItems = dto.OrderItems
                .GroupBy(item => item.ProductId)
                .Select(group => new
                {
                    ProductId = group.Key,
                    Quantity = group.Sum(item => item.Quantity)
                })
                .ToList();

            var products = new List<Product>();
            foreach (var requestedItem in requestedItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(requestedItem.ProductId);
                if (product == null)
                {
                    return new ServiceResult<OrderDto>(false, 404, $"Product {requestedItem.ProductId} not found.");
                }

                if (product.StoreId != dto.StoreId)
                {
                    return new ServiceResult<OrderDto>(false, 400, $"Product {requestedItem.ProductId} does not belong to store {dto.StoreId}.");
                }

                if (product.StockQuantity < requestedItem.Quantity)
                {
                    return new ServiceResult<OrderDto>(
                        false,
                        400,
                        $"Product {requestedItem.ProductId} has insufficient stock. Available: {product.StockQuantity}, requested: {requestedItem.Quantity}.");
                }

                products.Add(product);
            }

            var activeOffers = await GetActiveOffersAsync(dto.StoreId);
            var order = new Order
            {
                BuyerId = dto.BuyerId,
                StoreId = dto.StoreId,
                CarrierId = dto.CarrierId,
                ShippingAddressSnapshot = dto.ShippingAddressSnapshot,
                PaymentMethod = dto.PaymentMethod
            };

            foreach (var requestedItem in requestedItems)
            {
                var product = products.First(product => product.ProductId == requestedItem.ProductId);
                var unitPrice = OfferPricingCalculator.GetCurrentUnitPrice(product, activeOffers);
                var subtotal = unitPrice * requestedItem.Quantity;
                product.StockQuantity -= requestedItem.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = requestedItem.ProductId,
                    Product = product,
                    Quantity = requestedItem.Quantity,
                    UnitPrice = unitPrice,
                    Subtotal = subtotal
                });
                order.TotalAmount += subtotal;
            }

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            order.Buyer = buyer;
            order.Store = store;
            order.Carrier = carrier;

            return new ServiceResult<OrderDto>(true, 201, "Order created successfully", order.ToOrderDto());
        }
        catch (Exception ex) when (IsConcurrencyException(ex))
        {
            throw;
        }
        catch (Exception ex)
        {
            return Failure<OrderDto>(ex);
        }
    }

    public async Task<ServiceResult<OrderDto>> UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return new ServiceResult<OrderDto>(false, 404, "Order not found.");
            }

            var storeAuthorizationError = EnsureCanManageStore(order.StoreId);
            if (storeAuthorizationError != null)
            {
                return storeAuthorizationError;
            }

            if (dto.CarrierId.HasValue)
            {
                var carrier = await _unitOfWork.Carriers.GetByIdAsync(dto.CarrierId.Value);
                if (carrier == null)
                {
                    return new ServiceResult<OrderDto>(false, 404, "Carrier not found.");
                }
            }

            order.CarrierId = dto.CarrierId;
            order.ShippingCost = dto.ShippingCost;
            order.TrackingNumber = dto.TrackingNumber;
            order.PaymentStatus = dto.PaymentStatus;
            order.Status = dto.Status;

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<OrderDto>(true, 200, "Order status updated successfully", order.ToOrderDto());
        }
        catch (Exception ex)
        {
            return Failure<OrderDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return new ServiceResult(false, 404, "Order not found.");
            }

            await _unitOfWork.Orders.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "Order deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private Task<IReadOnlyList<Order>> GetOrdersWithDetailsAsync()
    {
        return _unitOfWork.Orders.ListAsync(
            null,
            true,
            order => order.Buyer!,
            order => order.Store!,
            order => order.Carrier!,
            order => order.OrderItems);
    }

    private Task<Order?> GetOrderWithDetailsAsync(int id)
    {
        return _unitOfWork.Orders.GetFirstOrDefaultAsync(
            order => order.OrderId == id,
            true,
            order => order.Buyer!,
            order => order.Store!,
            order => order.Carrier!,
            order => order.OrderItems);
    }

    private async Task<IReadOnlyList<Offer>> GetActiveOffersAsync(int storeId)
    {
        var now = DateTime.UtcNow;
        return await _unitOfWork.Offers.ListAsync(
            offer => offer.StoreId == storeId && offer.StartDate <= now && offer.EndDate >= now,
            true,
            offer => offer.OfferProducts);
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");

    private ServiceResult<OrderDto>? EnsureCurrentBuyer(int buyerId)
    {
        if (_currentUserService?.IsAuthenticated != true || _currentUserService.Role == AuthConstants.AdminRole)
        {
            return null;
        }

        return _currentUserService.UserId == buyerId
            ? null
            : new ServiceResult<OrderDto>(false, 403, "You are not allowed to create orders for another buyer.");
    }

    private ServiceResult<OrderDto>? EnsureCanManageStore(int storeId)
    {
        if (_currentUserService?.IsAuthenticated != true || _currentUserService.Role == AuthConstants.AdminRole)
        {
            return null;
        }

        return _currentUserService.StoreId == storeId
            ? null
            : new ServiceResult<OrderDto>(false, 403, "You are not allowed to manage orders for this store.");
    }

    private static bool IsConcurrencyException(Exception ex) =>
        ex.GetType().FullName == "Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException";
}
