using CartMicroservice.Domain.Interfaces;
using CartMicroservice.Infrastructure.Data;
using CartMicroservice.Infrastructure.Repositories;
using CartMicroservice.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace CartMicroservice.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCartServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure DbContext
        services.AddDbContext<CartDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("CartMicroservice.Infrastructure")));

        // Configure HttpClient for CatalogService
        services.AddHttpClient<ICatalogService, CatalogService>(client =>
        {
            client.BaseAddress = new Uri(configuration["CatalogService:Url"] ?? "http://localhost:5082");
        });

        // Configure RabbitMQ
        services.AddSingleton<IConnection>(sp =>
        {
            var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
            var userName = configuration["RabbitMQ:UserName"] ?? "guest";
            var password = configuration["RabbitMQ:Password"] ?? "guest";

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = userName,
                Password = password
            };
            return factory.CreateConnection();
        });

        // Register repositories
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();

        // Register services
        services.AddSingleton<IMessageBroker, RabbitMQMessageBroker>();

        return services;
    }
} 