using System.Text.Json;
using Domain;
using Domain.ValueObjects;
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
        builder.HasOne<Merchant>().WithMany().HasForeignKey(merchant => merchant.MerchantId).HasPrincipalKey(merchant => merchant.Id);
        builder.Property(order => order.Status).HasConversion<string>().IsRequired();
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.TotalAmount).HasConversion(money => money.Amount, value => Money.Create(value)).HasPrecision(18, 2).IsRequired();
        builder.Property(order => order.OrderLines).HasConversion( orderlines => JsonSerializer.Serialize(orderlines, (JsonSerializerOptions?)null)!,
                                                                   json => JsonSerializer.Deserialize<IReadOnlyList<OrderLine>>(json, (JsonSerializerOptions?)null)!).HasField("_orderLines").IsRequired();
        
    }
}