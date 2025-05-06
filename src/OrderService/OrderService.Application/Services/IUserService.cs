using OrderService.Contracts.DTOs.Auth;
using OrderService.Domain.Models;

namespace OrderService.Application.Services;

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
} 