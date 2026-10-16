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
        builder.Property(order => order.OrderId).HasConversion(id => id.Value, value => OrderId.Create(value));
        builder.Property(order => order.Status).HasConversion<string>().IsRequired();
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.ComplexProperty(order => order.TotalAmount, orderBuilder =>
        {
           orderBuilder.Property(money => money.Amount).HasPrecision(18, 2).IsRequired(); 
        });

        builder.ComplexCollection(order => order.OrderLines, linesBuilder =>
        {
            linesBuilder.Property(line => line.ProductId).HasConversion(pid => pid.Value, value => ProductId.Create(value)).IsRequired(); 
            linesBuilder.Property(line => line.Quantity).HasConversion(q => q.Value, value => Quantity.Create(value)).IsRequired();
            linesBuilder.ComplexProperty(line  => line.UnitPrice, moneyBuilder =>
            {
                moneyBuilder.Property(money => money.Amount).HasPrecision(18, 2).IsRequired(); 
            });
        });
        
    }
}