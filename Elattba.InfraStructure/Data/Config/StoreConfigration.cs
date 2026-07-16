using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Elattba.InfraStructure.Data.Config
{
    public class StoreConfigration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.HasOne(s => s.Owner)
                 .WithOne(u => u.OwnedStore)
                 .HasForeignKey<Store>(s => s.OwnerId)
                 .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(s => s.Manager)
                 .WithMany(u => u.ManagedStores)
                 .HasForeignKey(s => s.ManagerId)
                 .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(s => s.Category)
                 .WithMany(c => c.Stores)
                 .HasForeignKey(s => s.CategoryId)
                 .OnDelete(DeleteBehavior.Restrict);
            builder.Property(s => s.Rating).HasPrecision(3, 2); // 0.00 - 5.00
            
            builder.HasMany(s => s.ProductLines)
                 .WithMany(c => c.ProductLineStores)
                 .UsingEntity<Dictionary<string, object>>(
                     "StoreProductLine",
                     j => j.HasOne<Category>().WithMany().HasForeignKey("CategoryId"),
                     j => j.HasOne<Store>().WithMany().HasForeignKey("StoreId"),
                     j => j.ToTable("StoreProductLines")
                 );
            builder.HasIndex(s => s.OwnerId).IsUnique();
            builder.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Stores_Rating_Range", "[Rating] >= 0 AND [Rating] <= 5");
            });
        }
    }
}
