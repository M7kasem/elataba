using ElAtaba.Domain.Entities;
using Elattba.Application.Auth;
using Elattba.Application.Common;
using Elattba.Application.Offers;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.Checkouts;

public sealed class CheckoutService : ICheckoutService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public CheckoutService(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<CheckoutResultDto>> CreateAsync(CreateCheckoutDto dto)
    {
        try
        {
            if (dto.Items == null || dto.Items.Count == 0)
            {
                return new ServiceResult<CheckoutResultDto>(false, 400, "Checkout must contain at least one item.");
            }

            if (dto.Items.Any(item => item.Quantity <= 0))
            {
                return new ServiceResult<CheckoutResultDto>(false, 400, "Checkout item quantity must be greater than zero.");
            }

            if (dto.Items.Any(item => item.ProductId <= 0))
            {
                return new ServiceResult<CheckoutResultDto>(false, 400, "Checkout item product id must be greater than zero.");
            }

            var buyerAuthorizationError = EnsureCurrentBuyer(dto.BuyerId);
            if (buyerAuthorizationError != null)
            {
                return buyerAuthorizationError;
            }

            var buyer = await _unitOfWork.Users.GetByIdAsync(dto.BuyerId);
            if (buyer == null)
            {
                return new ServiceResult<CheckoutResultDto>(false, 404, "Buyer not found.");
            }

            Carrier? carrier = null;
            if (dto.CarrierId.HasValue)
            {
                carrier = await _unitOfWork.Carriers.GetByIdAsync(dto.CarrierId.Value);
                if (carrier == null)
                {
                    return new ServiceResult<CheckoutResultDto>(false, 404, "Carrier not found.");
                }
            }

            var requestedItems = dto.Items
                .GroupBy(item => item.ProductId)
                .Select(group => new CheckoutRequestedItem(group.Key, group.Sum(item => item.Quantity)))
                .ToList();

            var productsById = new Dictionary<int, Product>();
            foreach (var requestedItem in requestedItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(requestedItem.ProductId);
                if (product == null)
                {
                    return new ServiceResult<CheckoutResultDto>(false, 404, $"Product {requestedItem.ProductId} not found.");
                }

                if (product.StockQuantity < requestedItem.Quantity)
                {
                    return new ServiceResult<CheckoutResultDto>(
                        false,
                        400,
                        $"Product {requestedItem.ProductId} has insufficient stock. Available: {product.StockQuantity}, requested: {requestedItem.Quantity}.");
                }

                productsById[product.ProductId] = product;
            }

            var orders = new List<Order>();
            foreach (var storeGroup in requestedItems.GroupBy(item => productsById[item.ProductId].StoreId))
            {
                var store = await _unitOfWork.Stores.GetByIdAsync(storeGroup.Key);
                if (store == null)
                {
                    return new ServiceResult<CheckoutResultDto>(false, 404, $"Store {storeGroup.Key} not found.");
                }

                var activeOffers = await GetActiveOffersAsync(storeGroup.Key);
                var order = new Order
                {
                    BuyerId = dto.BuyerId,
                    Buyer = buyer,
                    StoreId = store.StoreId,
                    Store = store,
                    CarrierId = dto.CarrierId,
                    Carrier = carrier,
                    ShippingAddressSnapshot = dto.ShippingAddressSnapshot,
                    PaymentMethod = dto.PaymentMethod
                };

                foreach (var requestedItem in storeGroup)
                {
                    var product = productsById[requestedItem.ProductId];
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
                orders.Add(order);
            }

            await _unitOfWork.CompleteAsync();

            var result = new CheckoutResultDto(
                GenerateCheckoutReference(),
                orders.Sum(order => order.TotalAmount),
                orders
                    .Select(order => new CheckoutOrderDto(
                        order.OrderId,
                        order.StoreId,
                        order.Store?.StoreName,
                        order.TotalAmount,
                        order.OrderItems.Sum(item => item.Quantity)))
                    .ToList());

            return new ServiceResult<CheckoutResultDto>(true, 201, "Checkout completed successfully", result);
        }
        catch (Exception ex) when (IsConcurrencyException(ex))
        {
            throw;
        }
        catch (Exception ex)
        {
            return Failure(ex);
        }
    }

    private async Task<IReadOnlyList<Offer>> GetActiveOffersAsync(int storeId)
    {
        var now = DateTime.UtcNow;
        return await _unitOfWork.Offers.ListAsync(
            offer => offer.StoreId == storeId && offer.StartDate <= now && offer.EndDate >= now,
            true,
            offer => offer.OfferProducts);
    }

    private ServiceResult<CheckoutResultDto>? EnsureCurrentBuyer(int buyerId)
    {
        if (_currentUserService?.IsAuthenticated != true || _currentUserService.Role == AuthConstants.AdminRole)
        {
            return null;
        }

        return _currentUserService.UserId == buyerId
            ? null
            : new ServiceResult<CheckoutResultDto>(false, 403, "You are not allowed to checkout for another buyer.");
    }

    private static ServiceResult<CheckoutResultDto> Failure(Exception ex) =>
        new(false, 500, "Unexpected server error.");

    private static string GenerateCheckoutReference() =>
        $"CHK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..21].ToUpperInvariant();

    private static bool IsConcurrencyException(Exception ex) =>
        ex.GetType().FullName == "Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException";

    private sealed record CheckoutRequestedItem(int ProductId, int Quantity);
}
