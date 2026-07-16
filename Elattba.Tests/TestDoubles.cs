using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using ElAtaba.Domain.Entities;
using Elattba.Application.Auth;
using Elattba.Application.ProductImages;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Elattba.Core.Services;

namespace Elattba.Tests;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public FakeUserRepository UsersRepo { get; } = new();
    public FakeStoreRepository StoresRepo { get; } = new();
    public FakeGovernorateRepository GovernoratesRepo { get; } = new();
    public FakeCategoryRepository CategoriesRepo { get; } = new();
    public FakeProductRepository ProductsRepo { get; } = new();
    public FakeProductImageRepository ProductImagesRepo { get; } = new();
    public FakePricingTierRepository PricingTiersRepo { get; } = new();
    public FakeOfferRepository OffersRepo { get; } = new();
    public FakeOfferProductRepository OfferProductsRepo { get; } = new();
    public FakeOrderRepository OrdersRepo { get; } = new();
    public FakeOrderItemRepository OrderItemsRepo { get; } = new();
    public FakeReviewRepository ReviewsRepo { get; } = new();
    public FakeMessageRepository MessagesRepo { get; } = new();
    public FakeCarrierRepository CarriersRepo { get; } = new();
    public FakeShippingRateRepository ShippingRatesRepo { get; } = new();

    public Exception? CompleteException { get; set; }
    public int CompleteCalls { get; private set; }

    public IUserRepository Users => UsersRepo;
    public IStoreRepository Stores => StoresRepo;
    public IGovernorateRepository Governorates => GovernoratesRepo;
    public ICategoryRepository Categories => CategoriesRepo;
    public IProductRepository Products => ProductsRepo;
    public IProductImageRepository ProductImages => ProductImagesRepo;
    public IPricingTierRepository PricingTiers => PricingTiersRepo;
    public IOfferRepository Offers => OffersRepo;
    public IOfferProductRepository OfferProducts => OfferProductsRepo;
    public IOrderRepository Orders => OrdersRepo;
    public IOrderItemRepository OrderItems => OrderItemsRepo;
    public IReviewRepository Reviews => ReviewsRepo;
    public IMessageRepository Messages => MessagesRepo;
    public ICarrierRepository Carriers => CarriersRepo;
    public IShippingRateRepository ShippingRates => ShippingRatesRepo;

    public Task<int> CompleteAsync()
    {
        CompleteCalls++;
        if (CompleteException != null)
        {
            throw CompleteException;
        }

        return Task.FromResult(1);
    }

    public void Dispose()
    {
    }
}

internal class FakeRepository<T> : IGenericRepository<T> where T : class
{
    private int _nextId = 1;

    public List<T> Items { get; } = [];

    public Task<IReadOnlyList<T>> GetAllAsync() => Task.FromResult<IReadOnlyList<T>>(Items.ToList());

    public Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes) => GetAllAsync();

    public Task<T?> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool disableTracking = true,
        params Expression<Func<T, object>>[] includes) =>
        Task.FromResult(Items.FirstOrDefault(predicate.Compile()));

    public Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        bool disableTracking = true,
        params Expression<Func<T, object>>[] includes)
    {
        IEnumerable<T> query = Items;
        if (predicate != null)
        {
            query = query.Where(predicate.Compile());
        }

        return Task.FromResult<IReadOnlyList<T>>(query.ToList());
    }

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
        Task.FromResult(Items.Any(predicate.Compile()));

    public Task<int> CountAsync() => Task.FromResult(Items.Count);

    public Task<T?> GetByIdAsync(int id) => Task.FromResult(Items.FirstOrDefault(item => GetId(item) == id));

    public Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes) => GetByIdAsync(id);

    public Task AddAsync(T entity)
    {
        AssignIdIfNeeded(entity);
        Items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var entity = Items.FirstOrDefault(item => GetId(item) == id);
        if (entity != null)
        {
            Items.Remove(entity);
        }

        return Task.CompletedTask;
    }

    private void AssignIdIfNeeded(T entity)
    {
        var idProperty = GetIdProperty();
        if (idProperty == null || idProperty.GetValue(entity) is not int id || id != 0)
        {
            return;
        }

        while (Items.Any(item => GetId(item) == _nextId))
        {
            _nextId++;
        }

        idProperty.SetValue(entity, _nextId++);
    }

    private static int? GetId(T entity) => GetIdProperty()?.GetValue(entity) as int?;

    private static PropertyInfo? GetIdProperty() =>
        typeof(T)
            .GetProperties()
            .FirstOrDefault(property => property.PropertyType == typeof(int) && property.Name.EndsWith("Id", StringComparison.Ordinal));
}

internal sealed class FakeUserRepository : FakeRepository<User>, IUserRepository;
internal sealed class FakeStoreRepository : FakeRepository<Store>, IStoreRepository
{
    public Task<PagedList<Store>> GetPagedAsync(StoreParams storeParams)
    {
        IEnumerable<Store> query = Items;

        if (!string.IsNullOrWhiteSpace(storeParams.Search))
        {
            var search = storeParams.Search.ToLower();
            query = query.Where(s =>
                s.StoreName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                s.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (storeParams.CategoryId.HasValue)
        {
            query = query.Where(s => s.CategoryId == storeParams.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(storeParams.Location))
        {
            var location = storeParams.Location.ToLower();
            query = query.Where(s => s.Location.Contains(location, StringComparison.OrdinalIgnoreCase));
        }

        var count = query.Count();
        var items = query
            .OrderBy(s => s.StoreName)
            .Skip((storeParams.PageNumber - 1) * storeParams.PageSize)
            .Take(storeParams.PageSize)
            .ToList();

        return Task.FromResult(new PagedList<Store>(
            storeParams.PageNumber,
            storeParams.PageSize,
            count,
            items));
    }
}
internal sealed class FakeGovernorateRepository : FakeRepository<Governorate>, IGovernorateRepository;
internal sealed class FakeCategoryRepository : FakeRepository<Category>, ICategoryRepository;
internal sealed class FakeProductRepository : FakeRepository<Product>, IProductRepository
{
    public Task<PagedList<Product>> GetPagedAsync(ProductParams productParams)
    {
        IEnumerable<Product> query = Items;

        if (productParams.CategoryId.HasValue && productParams.CategoryId.Value > 0)
        {
            query = query.Where(product => product.CategoryId == productParams.CategoryId.Value);
        }

        foreach (var token in TokenizeSearch(productParams.Search))
        {
            query = query.Where(product =>
                product.Name.Contains(token, StringComparison.OrdinalIgnoreCase) ||
                product.Description.Contains(token, StringComparison.OrdinalIgnoreCase));
        }

        query = productParams.Sort switch
        {
            "priceAsc" => query.OrderBy(product => product.BasePrice),
            "priceDesc" => query.OrderByDescending(product => product.BasePrice),
            _ => query.OrderBy(product => product.Name)
        };

        var count = query.Count();
        var items = query
            .Skip((productParams.PageNumber - 1) * productParams.PageSize)
            .Take(productParams.PageSize)
            .ToList();

        return Task.FromResult(new PagedList<Product>(
            productParams.PageNumber,
            productParams.PageSize,
            count,
            items));
    }

    private static IReadOnlyList<string> TokenizeSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return [];
        }

        return search
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
internal sealed class FakeProductImageRepository : FakeRepository<ProductImage>, IProductImageRepository;
internal sealed class FakePricingTierRepository : FakeRepository<PricingTier>, IPricingTierRepository;
internal sealed class FakeOfferRepository : FakeRepository<Offer>, IOfferRepository;
internal sealed class FakeOfferProductRepository : FakeRepository<OfferProduct>, IOfferProductRepository;
internal sealed class FakeOrderRepository : FakeRepository<Order>, IOrderRepository;
internal sealed class FakeOrderItemRepository : FakeRepository<OrderItem>, IOrderItemRepository;
internal sealed class FakeReviewRepository : FakeRepository<Review>, IReviewRepository;
internal sealed class FakeMessageRepository : FakeRepository<Message>, IMessageRepository;
internal sealed class FakeCarrierRepository : FakeRepository<Carrier>, ICarrierRepository;
internal sealed class FakeShippingRateRepository : FakeRepository<ShippingRate>, IShippingRateRepository;

internal sealed class FakeCurrentUserService : ICurrentUserService
{
    public bool IsAuthenticated { get; init; } = true;
    public string? IdentityUserId { get; init; }
    public int? UserId { get; init; }
    public int? StoreId { get; init; }
    public string? Role { get; init; } = AuthConstants.BuyerRole;
}

internal sealed class FakeImageManagementService : IImageManagementService
{
    private int _nextImage = 1;

    public List<string> DeletedImages { get; } = [];
    public Queue<string> UploadResults { get; } = [];
    public InvalidOperationException? UploadException { get; set; }

    public Task<string> AddImageAsync(ImageUploadFile file, string src)
    {
        if (UploadException != null)
        {
            throw UploadException;
        }

        return Task.FromResult(UploadResults.Count > 0 ? UploadResults.Dequeue() : $"/uploads/{src}/{_nextImage++}-{file.FileName}");
    }

    public async Task<IReadOnlyList<string>> AddImagesAsync(IEnumerable<ImageUploadFile> files, string src)
    {
        var urls = new List<string>();
        foreach (var file in files)
        {
            urls.Add(await AddImageAsync(file, src));
        }

        return urls;
    }

    public void DeleteImage(string src) => DeletedImages.Add(src);
}

internal sealed class FakeImageEmbeddingService : IImageEmbeddingService
{
    public Task<float[]> GenerateEmbeddingAsync(Stream imageStream, CancellationToken cancellationToken = default) =>
        Task.FromResult<float[]>([1, 0, 0]);
}

internal sealed class FakeProductImageEmbeddingQueue : IProductImageEmbeddingQueue
{
    public List<int> QueuedImageIds { get; } = [];

    public ValueTask QueueAsync(int productImageId, CancellationToken cancellationToken = default)
    {
        QueuedImageIds.Add(productImageId);
        return ValueTask.CompletedTask;
    }

    public async IAsyncEnumerable<int> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var imageId in QueuedImageIds)
        {
            yield return imageId;
            await Task.Yield();
        }
    }
}
