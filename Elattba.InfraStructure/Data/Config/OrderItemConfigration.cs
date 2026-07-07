using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elattba.InfraStructure.Data.Config
{
    internal class OrderItemConfigration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasOne(oi => oi.Order)
                 .WithMany(o => o.OrderItems)
                 .HasForeignKey(oi => oi.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(oi => oi.Product)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(oi => oi.ProductId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.Property(oi => oi.UnitPrice).HasPrecision(10, 2);
            builder.Property(oi => oi.Subtotal).HasPrecision(10, 2);
            builder.ToTable(table =>
            {
                table.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "[Quantity] > 0");
                table.HasCheckConstraint("CK_OrderItems_UnitPrice_Positive", "[UnitPrice] > 0");
                table.HasCheckConstraint("CK_OrderItems_Subtotal_NonNegative", "[Subtotal] >= 0");
            });
        }
    }
}
