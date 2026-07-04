using System;

namespace Elattba.Core.InterFaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IStoreRepository Stores { get; }
        IGovernorateRepository Governorates { get; }
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IProductImageRepository ProductImages { get; }
        IPricingTierRepository PricingTiers { get; }
        IOfferRepository Offers { get; }
        IOfferProductRepository OfferProducts { get; }
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }
        IReviewRepository Reviews { get; }
        IMessageRepository Messages { get; }
        ICarrierRepository Carriers { get; }
        IShippingRateRepository ShippingRates { get; }

        Task<int> CompleteAsync();
    }
}
