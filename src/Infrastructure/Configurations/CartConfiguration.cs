using System.Text.Json;
using Domain;
using Domain.ValueObjects;
using Infrastructure.Converters;
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
        builder.HasOne<Merchant>().WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(cart => cart.MerchantId).HasPrincipalKey(merchant => merchant.Id);
        builder.Property(cart => cart.MerchantId).HasConversion(merchantId => merchantId.Value, value => MerchantId.Create(value)).IsRequired();
        
        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new CartItemJsonConverter());

        builder.Property(cart => cart.CartItems)
        .HasConversion(
            items => JsonSerializer.Serialize(items, jsonOptions),
            json => JsonSerializer.Deserialize<List<CartItem>>(json, jsonOptions)!
        )
        .HasField("_cartItems")
        .IsRequired();
    }
}