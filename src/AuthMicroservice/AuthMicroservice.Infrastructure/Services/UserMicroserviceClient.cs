using System.Net.Http.Json;
using AuthMicroservice.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AuthMicroservice.Infrastructure.Services;

public class UserMicroserviceClient : IUserMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public UserMicroserviceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["UserMicroservice:BaseUrl"] 
            ?? throw new InvalidOperationException("UserMicroservice:BaseUrl configuration is missing");
    }

    public async Task CreateUserAsync(Guid userId, string email)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/users/from-auth", new
        {
            Id = userId,
            Email = email
        });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create user in UserMicroservice. Status: {response.StatusCode}, Error: {error}");
        }
    }
} 