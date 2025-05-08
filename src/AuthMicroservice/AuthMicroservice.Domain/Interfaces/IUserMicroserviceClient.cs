namespace AuthMicroservice.Domain.Interfaces;

public interface IUserMicroserviceClient
{
    Task CreateUserAsync(Guid userId, string email);
} 