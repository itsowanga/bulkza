using Domain;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("Merchants");
        builder.HasKey(merchant => merchant.Id);
        builder.Property(merchant => merchant.Id).HasConversion(Id => Id.Value, value => MerchantId.Create(value));
        builder.Property(merchant => merchant.Name).HasMaxLength(100).IsRequired();
        builder.Property(merchant => merchant.Status).HasConversion<string>().IsRequired();
        builder.Property(merchant => merchant.Role).HasConversion<string>().IsRequired();
        builder.Property(merchant => merchant.RegistrationDate).IsRequired();
     
        builder.ComplexProperty(merchant => merchant.PhoneNumber, merchantBuilder =>
        {
            merchantBuilder.Property(pnumber => pnumber.Value).HasMaxLength(16).IsRequired();
        }
        );
        builder.HasIndex(merchant => merchant.PhoneNumber).IsUnique();
        builder.ComplexProperty(merchant => merchant.Address, addressBuilder =>
        {   
            addressBuilder.Property(line => line.Street).HasMaxLength(100).IsRequired();
            addressBuilder.Property(line => line.Suburb).HasMaxLength(60);
            addressBuilder.Property(line => line.City).HasMaxLength(60).IsRequired();
            addressBuilder.Property(line => line.Province).HasMaxLength(60).IsRequired();
            addressBuilder.Property(line => line.PostalCode).HasMaxLength(10).IsRequired();
            addressBuilder.Property(line => line.Country).HasMaxLength(60).IsRequired();
        }
        );

    }
}