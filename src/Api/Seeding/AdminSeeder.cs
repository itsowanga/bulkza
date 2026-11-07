using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Api.Seeding;

public class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    /// <summary>Phone number the admin logs in with. Leave unset to skip seeding.</summary>
    public string? Phone {get; set;}

    /// <summary>Admin password. Leave unset to skip seeding. Never put it in appsettings.json.</summary>
    public string? Password {get; set;}

    public string Name {get; set;} = "Administrator";

    // A merchant must have an address. These defaults are placeholders the admin can change later.
    public string Street {get; set;} = "Head Office";
    public string City {get; set;} = "Not provided";
    public string Province {get; set;} = "Not provided";
    public string PostalCode {get; set;} = "0000";
    public string Country {get; set;} = "South Africa";
}

/// <summary>
/// Creates the first Admin merchant at startup when AdminSeed:Phone and AdminSeed:Password are configured.
/// It is idempotent and never changes an existing merchant: if the phone number is already registered,
/// nothing is modified, and a non-admin is not promoted. Supply the values through user secrets or
/// environment variables (AdminSeed__Phone, AdminSeed__Password), not appsettings.json.
/// </summary>
public class AdminSeeder(IServiceScopeFactory scopeFactory,
                         IOptions<AdminSeedOptions> options,
                         ILogger<AdminSeeder> logger) : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly AdminSeedOptions _options = options.Value;
    private readonly ILogger<AdminSeeder> _logger = logger;

    public async Task StartAsync(CancellationToken ct)
    {
        var hasPhone = !string.IsNullOrWhiteSpace(_options.Phone);
        var hasPassword = !string.IsNullOrWhiteSpace(_options.Password);

        if (!hasPhone && !hasPassword)
        {
            _logger.LogInformation("Admin seeding skipped: {Phone} and {Password} are not configured.",
                                   $"{AdminSeedOptions.SectionName}:Phone", $"{AdminSeedOptions.SectionName}:Password");
            return;
        }

        if (hasPhone != hasPassword)
        {
            throw new InvalidOperationException(
                $"Admin seeding is half configured. Set both {AdminSeedOptions.SectionName}:Phone and {AdminSeedOptions.SectionName}:Password, or neither.");
        }

        // Invalid values stop startup here with the domain's own validation message.
        var phone = PhoneNumber.Create(_options.Phone!);
        var passwordHash = PasswordHash.Create(_options.Password!);
        var address = Address.Create(_options.Street, _options.City, _options.Province, _options.PostalCode, _options.Country);

        using var scope = _scopeFactory.CreateScope();
        var merchants = scope.ServiceProvider.GetRequiredService<IMerchantRepository>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var existing = await merchants.GetByPhoneNumberAsync(phone, ct);
        if (existing is not null)
        {
            if (existing.Role == MerchantRole.Admin)
                _logger.LogInformation("Admin seeding skipped: merchant {MerchantId} is already an admin.", existing.MerchantId.Value);
            else
                _logger.LogWarning("Admin seeding skipped: the configured phone number belongs to merchant {MerchantId}, who is not an admin. " +
                                   "Existing merchants are not promoted. Use a different phone number.", existing.MerchantId.Value);
            return;
        }

        var admin = new Merchant(MerchantId.New(), _options.Name, PasswordHash.Create(hasher.Hash(passwordHash.Password)),
                                 MerchantStatus.Active, MerchantRole.Admin, address, phone, DateTimeOffset.UtcNow);

        await merchants.AddAsync(admin, ct);
        await uow.SaveChangesAsync(ct);

        _logger.LogInformation("Admin seeded: merchant {MerchantId} created with the Admin role.", admin.MerchantId.Value);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
