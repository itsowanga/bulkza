using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("Outbox");
        builder.HasKey(outbox => outbox.Id);
        builder.Property(outbox => outbox.Type).HasMaxLength(500).IsRequired();
        builder.Property(outbox => outbox.Content).IsRequired();
        builder.HasIndex(outbox => outbox.ProcessedAt);
    }
}