using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elattba.InfraStructure.Data.Config
{
    public class OfferProductConfigration : IEntityTypeConfiguration<OfferProduct>
    {
        public void Configure(EntityTypeBuilder<OfferProduct> builder)
        {
            builder.HasKey(op => new { op.OfferId, op.ProductId });

            builder.HasOne(op => op.Offer)
             .WithMany(o => o.OfferProducts)
             .HasForeignKey(op => op.OfferId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(op => op.Product)
             .WithMany(p => p.OfferProducts)
             .HasForeignKey(op => op.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
