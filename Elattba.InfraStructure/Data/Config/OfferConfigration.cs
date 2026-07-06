using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elattba.InfraStructure.Data.Config
{
    public class OfferConfigration : IEntityTypeConfiguration<Offer>
    {
        public void Configure(EntityTypeBuilder<Offer> builder)
        {
            builder.HasOne(o => o.Store)
                .WithMany(s => s.Offers)
                .HasForeignKey(o => o.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(o => o.DiscountPercentage).HasPrecision(5, 2);
            builder.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Offers_DiscountPercentage_Range", "[DiscountPercentage] > 0 AND [DiscountPercentage] <= 100");
                table.HasCheckConstraint("CK_Offers_Date_Range", "[StartDate] < [EndDate]");
            });
        }
    }
}
