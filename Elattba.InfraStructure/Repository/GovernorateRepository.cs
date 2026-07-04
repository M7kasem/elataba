using ElAtaba.Domain.Entities;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;

namespace Elattba.InfraStructure.Repository
{
    public class GovernorateRepository : GenericRepository<Governorate>, IGovernorateRepository
    {
        public GovernorateRepository(El3atbaDbContext context) : base(context)
        {
        }
    }
}
