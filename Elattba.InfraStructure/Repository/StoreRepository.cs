using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Elattba.InfraStructure.Repository
{
    public class StoreRepository : GenericRepository<Store>, IStoreRepository
    {
        private readonly El3atbaDbContext _context;

        public StoreRepository(El3atbaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PagedList<Store>> GetPagedAsync(StoreParams storeParams)
        {
            var query = _context.Stores
                .AsNoTracking()
                .Include(s => s.Owner!)
                .Include(s => s.Manager!)
                .Include(s => s.Category!)
                .Include(s => s.ProductLines)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(storeParams.Search))
            {
                var search = storeParams.Search.ToLower();
                query = query.Where(s =>
                    s.StoreName.ToLower().Contains(search) ||
                    s.Description.ToLower().Contains(search));
            }

            if (storeParams.CategoryId.HasValue)
                query = query.Where(s => s.CategoryId == storeParams.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(storeParams.Location))
                query = query.Where(s => s.Location.ToLower().Contains(storeParams.Location.ToLower()));

            var count = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.StoreName)
                .Skip((storeParams.PageNumber - 1) * storeParams.PageSize)
                .Take(storeParams.PageSize)
                .ToListAsync();

            return new PagedList<Store>(storeParams.PageNumber, storeParams.PageSize, count, items);
        }
    }
}

