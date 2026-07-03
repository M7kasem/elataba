using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elattba.InfraStructure.Data.Config
{
    internal class ReviewConfigration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasIndex(r => r.OrderId).IsUnique();

            builder.HasOne(r => r.Order)
             .WithOne(o => o.Review)
             .HasForeignKey<Review>(r => r.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Store)
             .WithMany(s => s.Reviews)
             .HasForeignKey(r => r.StoreId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Buyer)
             .WithMany(u => u.Reviews)
             .HasForeignKey(r => r.BuyerId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
