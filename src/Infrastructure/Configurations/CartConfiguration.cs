using Domain;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(cart => cart.CartId);
        builder.Property(cart => cart.CartId).HasConversion(id => id.Value, value => CartId.Create(value)).IsRequired();
        builder.HasOne<Merchant>().WithMany().HasForeignKey(cart => cart.MerchantId).HasForeignKey(cart => cart.MerchantId);
        builder.Property(cart => cart.MerchantId).HasConversion(merchantId => merchantId.Value, value => MerchantId.Create(value)).IsRequired();
        builder.ComplexCollection(cart => cart.CartItems, cartBuilder =>
        {
            cartBuilder.Property(line => line.ProductId).HasConversion(pid => pid.Value, value => ProductId.Create(value)).IsRequired();
            cartBuilder.Property(line => line.Quantity).HasConversion(quantity => quantity.Value, value => Quantity.Create(value)).IsRequired();
        });
    }
}