using System.Threading.Tasks;
using OrderService.Contracts.DTOs.Auth;

namespace OrderService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }
} 