using Application.Interfaces;
using Domain;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BulkzaDbContext>(options => options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IBulkPoolRepository, BulkPoolRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IMerchantRepository, MerchantRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        // Hashing
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        // Payment gateway
        services.AddScoped<IPaymentGateway, MockPaymentGateway>();

        // Unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}