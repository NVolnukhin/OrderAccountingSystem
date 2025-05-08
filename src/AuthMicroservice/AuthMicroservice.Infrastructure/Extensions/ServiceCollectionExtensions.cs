using AuthMicroservice.Domain.Interfaces;
using AuthMicroservice.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AuthMicroservice.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserMicroserviceClient(this IServiceCollection services)
    {
        services.AddHttpClient<IUserMicroserviceClient, UserMicroserviceClient>();
        return services;
    }
} 