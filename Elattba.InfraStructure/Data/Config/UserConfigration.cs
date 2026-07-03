using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Elattba.InfraStructure.Data.Config
{
    public class UserConfigration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            builder.HasOne(u => u.Governorate)
                   .WithMany(g => g.Users)
                   .HasForeignKey(u => u.GovernorateId);
            
        }
        
    }
}
