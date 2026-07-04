using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elattba.InfraStructure.Data.Config
{
    internal class OrderConfigration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasOne(o => o.Buyer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Store)
             .WithMany(s => s.Orders)
             .HasForeignKey(o => o.StoreId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Carrier)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(o => o.TotalAmount).HasPrecision(10, 2);
            builder.Property(o => o.ShippingCost).HasPrecision(10, 2);
            builder.Property(o => o.TrackingNumber).HasMaxLength(100);

            // Enums stored as readable strings rather than raw ints.
            builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(20);
            builder.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(20);
            builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        }
    }
}
