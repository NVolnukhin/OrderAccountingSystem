using AuthMicroservice.API.DTOs;
using AuthMicroservice.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthMicroservice.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;

    public AuthController(
        IUserService userService,
        ITokenService tokenService,
        IJwtProvider jwtProvider,
        IPasswordHasher passwordHasher)
    {
        _userService = userService;
        _tokenService = tokenService;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userService.GetByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest("User with this email already exists");

        var user = await _userService.CreateAsync(request.Email, request.Password);
        var token = _jwtProvider.GenerateToken(user.Id);

        return Ok(new { Token = token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var isValid = await _userService.ValidateCredentialsAsync(request.Email, request.Password);
        if (!isValid)
            return Unauthorized("Invalid email or password");

        var user = await _userService.GetByEmailAsync(request.Email);
        var token = _jwtProvider.GenerateToken(user!.Id);

        return Ok(new { Token = token });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userService.GetByEmailAsync(request.Email);
        if (user == null)
            return Ok(); // Don't reveal that the user doesn't exist

        var token = await _tokenService.CreatePasswordResetTokenAsync(user.Id);
        // TODO: Send email with reset link

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var isValid = await _tokenService.ValidatePasswordResetTokenAsync(request.Token);
        if (!isValid)
            return BadRequest("Invalid or expired token");

        var resetToken = await _tokenService.GetPasswordResetTokenAsync(request.Token);
        if (resetToken == null)
            return BadRequest("Token not found");

        var user = await _userService.GetByIdAsync(resetToken.UserId);
        if (user == null)
            return BadRequest("User not found");

        // Update password
        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _userService.UpdateAsync(user);

        // Invalidate token
        await _tokenService.InvalidatePasswordResetTokenAsync(request.Token);

        return Ok();
    }
} 