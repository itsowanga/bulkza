using System.Data.Common;
using Domain;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.ProductId);
        builder.Property(product => product.ProductId).HasConversion(id => id.Value, value => ProductId.Create(value)).IsRequired();
        builder.Property(product => product.SKU).HasMaxLength(50).IsRequired();
        builder.Property(product => product.Name).HasMaxLength(100).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(500);
        builder.Property(product => product.Category).HasConversion<string>().HasMaxLength(11).IsRequired();
        builder.Property(product => product.UnitOfMeasure).HasConversion<string>().HasMaxLength(9).IsRequired();
        builder.Property(product => product.BasePrice).HasConversion(money => money.Amount, value => Money.Create(value)).HasPrecision(18, 2).IsRequired();
        builder.Property(product => product.IsActive).IsRequired();




    }

}
