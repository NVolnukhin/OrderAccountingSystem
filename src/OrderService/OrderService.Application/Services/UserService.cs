using OrderService.Application.Services;
using OrderService.Contracts.DTOs.Auth;
using OrderService.Domain.Models;
using OrderService.Infrastructure.Auth;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        if (await _userRepository.ExistsByEmailAsync(registerDto.Email))
        {
            throw new Exception("User with this email already exists");
        }

        if (await _userRepository.ExistsByUsernameAsync(registerDto.Username))
        {
            throw new Exception("User with this username already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = _jwtProvider.GenerateToken(user);
        return new AuthResponseDto(
            Token: token,
            Username: user.Username,
            Email: user.Email,
            Role: user.Role
        );
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null)
        {
            throw new Exception("Invalid email or password");
        }

        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            throw new Exception("Invalid email or password");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = _jwtProvider.GenerateToken(user);
        return new AuthResponseDto(
            Token: token,
            Username: user.Username,
            Email: user.Email,
            Role: user.Role
        );
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        return _userRepository.GetByEmailAsync(email);
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return _userRepository.GetByUsernameAsync(username);
    }
} 