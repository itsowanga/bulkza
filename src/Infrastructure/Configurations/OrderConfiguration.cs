using System.Text.Json;
using Domain;
using Domain.ValueObjects;
using Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{

    public void Configure(EntityTypeBuilder<Order> builder)
    {

        builder.ToTable("Orders");
        builder.HasKey(order => order.OrderId);
        builder.Property(order => order.OrderId).HasConversion(id => id.Value, value => OrderId.Create(value)).IsRequired();
        builder.HasOne<Merchant>().WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(order => order.MerchantId).HasPrincipalKey(merchant => merchant.Id);
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.TotalAmount).HasConversion(money => money.Amount, value => Money.Create(value)).HasPrecision(18, 2).IsRequired();
        
        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new OrderLineJsonConverter());

        builder.Property(order => order.OrderLines)
        .HasConversion(
            lines => JsonSerializer.Serialize(lines, jsonOptions),
            json => JsonSerializer.Deserialize<List<OrderLine>>(json, jsonOptions)!
        )
        .HasField("_orderLines")
        .IsRequired();
        
    }
}