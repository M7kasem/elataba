using ElAtaba.Domain.Entities;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Elattba.InfraStructure.Repository
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(El3atbaDbContext context) : base(context)
        {

        }

        public Task<IReadOnlyList<Category>> GetAllAsync(params Expression<Func<Category, object>>[] includes)
        {
            throw new NotImplementedException();
        }
    }
}
