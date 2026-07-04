using ElAtaba.Domain.Entities;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;

namespace Elattba.InfraStructure.Repository
{
    public class PricingTierRepository : GenericRepository<PricingTier>, IPricingTierRepository
    {
        public PricingTierRepository(El3atbaDbContext context) : base(context)
        {
        }
    }
}
