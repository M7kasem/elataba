using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;

namespace Elattba.InfraStructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly El3atbaDbContext _context;

        public UnitOfWork(El3atbaDbContext context)
        {
            _context = context;

            Users = new UserRepository(_context);
            Stores = new StoreRepository(_context);
            Governorates = new GovernorateRepository(_context);
            Categories = new CategoryRepository(_context);
            Products = new ProductRepository(_context);
            ProductImages = new ProductImageRepository(_context);
            PricingTiers = new PricingTierRepository(_context);
            Offers = new OfferRepository(_context);
            OfferProducts = new OfferProductRepository(_context);
            Orders = new OrderRepository(_context);
            OrderItems = new OrderItemRepository(_context);
            Reviews = new ReviewRepository(_context);
            Messages = new MessageRepository(_context);
            Carriers = new CarrierRepository(_context);
            ShippingRates = new ShippingRateRepository(_context);
        }

        public IUserRepository Users { get; }
        public IStoreRepository Stores { get; }
        public IGovernorateRepository Governorates { get; }
        public ICategoryRepository Categories { get; }
        public IProductRepository Products { get; }
        public IProductImageRepository ProductImages { get; }
        public IPricingTierRepository PricingTiers { get; }
        public IOfferRepository Offers { get; }
        public IOfferProductRepository OfferProducts { get; }
        public IOrderRepository Orders { get; }
        public IOrderItemRepository OrderItems { get; }
        public IReviewRepository Reviews { get; }
        public IMessageRepository Messages { get; }
        public ICarrierRepository Carriers { get; }
        public IShippingRateRepository ShippingRates { get; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
