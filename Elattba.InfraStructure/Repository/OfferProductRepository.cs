using ElAtaba.Domain.Entities;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;

namespace Elattba.InfraStructure.Repository
{
    public class OfferProductRepository : GenericRepository<OfferProduct>, IOfferProductRepository
    {
        public OfferProductRepository(El3atbaDbContext context) : base(context)
        {
        }
    }
}
