using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elattba.InfraStructure.Data.Config
{
    public class PricingTierConfigration : IEntityTypeConfiguration<PricingTier>
    {
        public void Configure(EntityTypeBuilder<PricingTier> builder)
        {
            builder.HasKey(t => t.TierId);

            builder.HasOne(t => t.Product)
             .WithMany(p => p.PricingTiers)
             .HasForeignKey(t => t.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            builder.Property(t => t.PricePerUnit).HasPrecision(10, 2);
            builder.ToTable(table =>
            {
                table.HasCheckConstraint("CK_PricingTiers_MinQuantity_Positive", "[MinQuantity] > 0");
                table.HasCheckConstraint("CK_PricingTiers_PricePerUnit_Positive", "[PricePerUnit] > 0");
            });
        }
    }
}
