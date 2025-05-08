using AuthMicroservice.Domain.Interfaces;
using AuthMicroservice.Infrastructure.Auth;
using AuthMicroservice.Infrastructure.Data;
using AuthMicroservice.Infrastructure.Repositories;
using AuthMicroservice.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthMicroservice.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtOptions>(options =>
        {
            options.Key = configuration["Jwt:Key"]!;
            options.Issuer = configuration["Jwt:Issuer"]!;
            options.Audience = configuration["Jwt:Audience"]!;
            options.ExpiryInMinutes = int.Parse(configuration["Jwt:ExpiryInMinutes"]!);
        });

        services.Configure<PasswordHashingSettings>(options =>
        {
            options.WorkFactor = int.Parse(configuration["PasswordHashing:WorkFactor"] ?? "12");
        });

        services.AddScoped<IAuthUserRepository, AuthUserRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();

        return services;
    }
} 