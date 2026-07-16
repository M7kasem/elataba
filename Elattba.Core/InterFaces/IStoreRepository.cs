using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;

namespace Elattba.Core.InterFaces
{
    public interface IStoreRepository : IGenericRepository<Store>
    {
        Task<PagedList<Store>> GetPagedAsync(StoreParams storeParams);
    }
}
