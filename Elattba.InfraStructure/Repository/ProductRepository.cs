using ElAtaba.Domain.Entities;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elattba.InfraStructure.Repository
{
    internal class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(El3atbaDbContext context) : base(context)
        {
        }
    }
}
