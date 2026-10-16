using Domain;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class BulkPoolConfiguration : IEntityTypeConfiguration<BulkPool>
{
    public void Configure(EntityTypeBuilder<BulkPool> builder)
    {
        builder.ToTable("BulkPools");
        builder.HasKey(pool => pool.PoolId);
        builder.Property(pool => pool.PoolId).HasConversion(id => id.Value, value => PoolId.Create(value)).IsRequired();
        builder.HasOne<Product>().WithMany().HasForeignKey(pool => pool.ProductId).HasPrincipalKey(pool => pool.ProductId);
        builder.Property(pool => pool.ProductId).HasConversion(pid => pid.Value, value => ProductId.Create(value)).IsRequired();
        builder.Property(pool => pool.CurrentQuantity).IsRequired();
        builder.Property(pool => pool.MinimumQuantity).HasConversion(quantity => quantity.Value, value => Quantity.Create(value)).IsRequired();
        builder.Property(pool => pool.MinimumMerchants).IsRequired();
        builder.Property(pool => pool.Deadline).IsRequired();
        builder.Property(pool => pool.CreatedAt).IsRequired();
        builder.ComplexCollection(pool => pool.Participants, participantBuilder =>
        {
           participantBuilder.Property(participant => participant.MerchantId).HasConversion(merchantId => merchantId.Value, value => MerchantId.Create(value)).IsRequired();
           participantBuilder.Property(participant => participant.Quantity).HasConversion(quantity => quantity.Value, value => Quantity.Create(value)).IsRequired(); 
        });

    }
}