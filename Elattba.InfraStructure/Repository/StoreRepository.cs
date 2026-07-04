using ElAtaba.Domain.Entities;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;

namespace Elattba.InfraStructure.Repository
{
    public class StoreRepository : GenericRepository<Store>, IStoreRepository
    {
        public StoreRepository(El3atbaDbContext context) : base(context)
        {
        }
    }
}
