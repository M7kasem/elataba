using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Elattba.InfraStructure.Data.Config
{
    public class ProductsConfigration : IEntityTypeConfiguration<Product>
    {

        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasOne(p => p.Store)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.StoreId)
                    .OnDelete(DeleteBehavior.Cascade);
            builder.Property(p => p.BasePrice).HasPrecision(10, 2);
            builder.Property(p => p.RowVersion).IsRowVersion();
            builder.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Products_BasePrice_Positive", "[BasePrice] > 0");
                table.HasCheckConstraint("CK_Products_StockQuantity_NonNegative", "[StockQuantity] >= 0");
            });

        }
    }
}
