using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elattba.InfraStructure.Data.Config
{
    public class ProductImageConfigration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.HasKey(i => i.ImageId);
            builder.HasOne(i => i.Product)
                   .WithMany(p => p.Images)
                   .HasForeignKey(i => i.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.Property(i => i.EmbeddingVector).HasColumnType("nvarchar(max)");
        }
    }
}
