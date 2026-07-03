using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elattba.InfraStructure.Data.Config
{
    internal class MessageConfigration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {

            builder.HasOne(m => m.Sender)
             .WithMany(u => u.SentMessages)
             .HasForeignKey(m => m.SenderId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Recipient)
             .WithMany(u => u.ReceivedMessages)
             .HasForeignKey(m => m.RecipientId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Product)
             .WithMany(p => p.Messages)
             .HasForeignKey(m => m.ProductId)
             .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
