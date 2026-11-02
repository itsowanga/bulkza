using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using Domain;
using Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;
public class BulkzaDbContext(DbContextOptions<BulkzaDbContext> options) : DbContext(options)
{
    public DbSet<Merchant> Merchants {get; set;}
    public DbSet<Product> Products {get; set;}
    public DbSet<BulkPool> BulkPools {get; set;}
    public DbSet<Order> Orders {get; set;}
    public DbSet<Cart> Carts {get; set;}
    public DbSet<OutboxMessage> Outbox {get; set;}

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var poolEntries = ChangeTracker.Entries<BulkPool>().Where(entry => entry.Entity.DomainEvents.Count > 0)
                                                           .ToList();
        var outboxRows = poolEntries.SelectMany(entry => entry.Entity.DomainEvents)
                                    .Select(ToOutboxMessage)
                                    .ToList();

        Outbox.AddRange(outboxRows);
        var result = await base.SaveChangesAsync(ct);
        foreach (var entry in poolEntries) entry.Entity.ClearDomainEvents();

        return result;
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BulkzaDbContext).Assembly);
    }

    private static OutboxMessage ToOutboxMessage(IDomainEvent domainEvent) 
    {
        return OutboxMessage.Create(domainEvent.GetType().AssemblyQualifiedName!,
                                    JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                                    DateTimeOffset.UtcNow);
    }
}