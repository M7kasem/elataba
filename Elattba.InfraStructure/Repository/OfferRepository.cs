using ElAtaba.Domain.Entities;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;

namespace Elattba.InfraStructure.Repository
{
    public class OfferRepository : GenericRepository<Offer>, IOfferRepository
    {
        public OfferRepository(El3atbaDbContext context) : base(context)
        {
        }
    }
}
