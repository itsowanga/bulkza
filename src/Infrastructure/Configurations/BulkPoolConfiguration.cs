using System.Text.Json;
using Domain;
using Domain.ValueObjects;
using Infrastructure.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class BulkPoolConfiguration : IEntityTypeConfiguration<BulkPool>
{
    public void Configure(EntityTypeBuilder<BulkPool> builder)
    {
        builder.ToTable("BulkPools");
        builder.HasKey(pool => pool.PoolId);
        builder.Property(pool => pool.PoolId).HasConversion(id => id.Value, value => PoolId.Create(value)).IsRequired();
        builder.Property(pool => pool.Status).HasConversion<string>().HasMaxLength(13).IsRequired();
        builder.HasOne<Product>().WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(pool => pool.ProductId).HasPrincipalKey(product => product.ProductId);
        builder.Property(pool => pool.ProductId).HasConversion(pid => pid.Value, value => ProductId.Create(value)).IsRequired();
        builder.Property(pool => pool.CurrentQuantity).IsRequired();
        builder.Property(pool => pool.MinimumQuantity).HasConversion(quantity => quantity.Value, value => Quantity.Create(value)).IsRequired();
        builder.Property(pool => pool.MinimumMerchants).IsRequired();
        builder.Property(pool => pool.Deadline).IsRequired();
        builder.Property(pool => pool.CreatedAt).IsRequired();
        builder.Ignore(pool => pool.DomainEvents);
        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new ParticipantsJsonConverter());

        builder.Property(participants => participants.Participants)
        .HasConversion(
            lines => JsonSerializer.Serialize(lines, jsonOptions),
            json => JsonSerializer.Deserialize<List<Participant>>(json, jsonOptions)!
        )
        .IsRequired();

        builder.Property(pool => pool.Participants)
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyList<Participant>>(
                (a, b) => a!.SequenceEqual(b!),
                v => v.Aggregate(0, (hash, p) => HashCode.Combine(hash, p.GetHashCode())),
                v => v.Select(p => p).ToList()  
            ));
    }
}