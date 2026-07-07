using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Elattba.InfraStructure.Repository;

internal class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly El3atbaDbContext _context;

    public ProductRepository(El3atbaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedList<Product>> GetPagedAsync(ProductParams productParams)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(product => product.Store)
            .Include(product => product.Category)
            .Include(product => product.Images)
            .Include(product => product.PricingTiers)
            .AsQueryable();

        if (productParams.CategoryId.HasValue && productParams.CategoryId.Value > 0)
        {
            query = query.Where(product => product.CategoryId == productParams.CategoryId.Value);
        }

        foreach (var token in TokenizeSearch(productParams.Search))
        {
            query = query.Where(product =>
                product.Name.ToLower().Contains(token) ||
                product.Description.ToLower().Contains(token));
        }

        query = productParams.Sort switch
        {
            "priceAsc" => query.OrderBy(product => product.BasePrice),
            "priceDesc" => query.OrderByDescending(product => product.BasePrice),
            _ => query.OrderBy(product => product.Name)
        };

        var count = await query.CountAsync();
        var items = await query
            .Skip((productParams.PageNumber - 1) * productParams.PageSize)
            .Take(productParams.PageSize)
            .ToListAsync();

        return new PagedList<Product>(
            productParams.PageNumber,
            productParams.PageSize,
            count,
            items);
    }

    private static IReadOnlyList<string> TokenizeSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return [];
        }

        return search
            .ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
