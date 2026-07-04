using ElAtaba.Domain.Entities;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;

namespace Elattba.InfraStructure.Repository
{
    public class ShippingRateRepository : GenericRepository<ShippingRate>, IShippingRateRepository
    {
        public ShippingRateRepository(El3atbaDbContext context) : base(context)
        {
        }
    }
}
