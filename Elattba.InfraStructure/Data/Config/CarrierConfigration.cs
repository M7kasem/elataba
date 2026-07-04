using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elattba.InfraStructure.Data.Config
{
    internal class CarrierConfigration : IEntityTypeConfiguration<Carrier>
    {
        public void Configure(EntityTypeBuilder<Carrier> builder)
        {
            builder.Property(c => c.Name).HasMaxLength(150);
            builder.HasIndex(c => c.Name).IsUnique();

            builder.HasData(
                new Carrier { CarrierId = 1, Name = "Bosta", IsActive = true },
                new Carrier { CarrierId = 2, Name = "Aramex", IsActive = true },
                new Carrier { CarrierId = 3, Name = "Mylerz", IsActive = true });
        }
    }
}
