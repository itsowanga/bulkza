using Domain;
using Domain.ValueObjects;
using Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("Merchants");
        builder.HasKey(merchant => merchant.MerchantId);
        builder.Property(merchant => merchant.MerchantId).HasConversion(Id => Id.Value, value => MerchantId.Create(value)).IsRequired();
        builder.Property(merchant => merchant.Name).HasMaxLength(100).IsRequired();
        builder.Property(merchant => merchant.Status).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(merchant => merchant.Role).HasConversion<string>().HasMaxLength(11).IsRequired();
        builder.Property(merchant => merchant.RegistrationDate).IsRequired();
        builder.HasIndex(merchant => merchant.PhoneNumber).IsUnique();
        builder.Property(merchant => merchant.PhoneNumber).HasConversion(pnumber => pnumber.Value, value => PhoneNumber.Create(value)).HasMaxLength(16).IsRequired();
        builder.Property(merchant => merchant.Address).HasConversion<AddressJsonConverter>().IsRequired();
        builder.Property(merchant => merchant.PasswordHash).HasConversion(password => password.Password, value => PasswordHash.Create(value)).HasMaxLength(100).IsRequired();

    }
}