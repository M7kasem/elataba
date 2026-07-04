using ElAtaba.Domain.Entities;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;

namespace Elattba.InfraStructure.Repository
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(El3atbaDbContext context) : base(context)
        {
        }
    }
}
