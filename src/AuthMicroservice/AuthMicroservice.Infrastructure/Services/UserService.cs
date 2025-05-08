using AuthMicroservice.Domain.Interfaces;
using AuthMicroservice.Domain.Models;

namespace AuthMicroservice.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IAuthUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserMicroserviceClient _userMicroserviceClient;

    public UserService(
        IAuthUserRepository userRepository, 
        IPasswordHasher passwordHasher,
        IUserMicroserviceClient userMicroserviceClient)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _userMicroserviceClient = userMicroserviceClient;
    }

    public async Task<AuthUser?> GetByIdAsync(Guid id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<AuthUser?> GetByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<AuthUser> CreateAsync(string email, string password)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = new AuthUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Create user in UserMicroservice
        await _userMicroserviceClient.CreateUserAsync(user.Id, user.Email);

        return user;
    }

    public async Task UpdateAsync(AuthUser user)
    {
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            await _userRepository.DeleteAsync(user);
            await _userRepository.SaveChangesAsync();
        }
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return false;

        return _passwordHasher.VerifyPassword(password, user.PasswordHash);
    }
} 