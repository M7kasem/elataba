using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elattba.InfraStructure.Data.Config
{
    internal class ShippingRateConfigration : IEntityTypeConfiguration<ShippingRate>
    {
        public void Configure(EntityTypeBuilder<ShippingRate> builder)
        {
            builder.HasOne(sr => sr.Carrier)
                .WithMany(c => c.ShippingRates)
                .HasForeignKey(sr => sr.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.Governorate)
                .WithMany(g => g.ShippingRates)
                .HasForeignKey(sr => sr.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(sr => sr.Cost).HasPrecision(10, 2);
            builder.HasIndex(sr => new { sr.CarrierId, sr.GovernorateId }).IsUnique();
        }
    }
}
