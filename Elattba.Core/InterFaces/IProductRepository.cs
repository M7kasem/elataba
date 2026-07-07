using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;

namespace Elattba.Core.InterFaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<PagedList<Product>> GetPagedAsync(ProductParams productParams);
}
