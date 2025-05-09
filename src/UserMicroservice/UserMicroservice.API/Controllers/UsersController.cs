using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserMicroservice.API.DTOs;
using UserMicroservice.Domain.Interfaces;
using UserMicroservice.Domain.Models;

namespace UserMicroservice.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAddressRepository _addressRepository;

    public UsersController(
        IUserRepository userRepository,
        IUserAddressRepository addressRepository)
    {
        _userRepository = userRepository;
        _addressRepository = addressRepository;
    }

    [HttpPost("from-auth")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateFromAuth([FromBody] CreateUserFromAuthRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Conflict(new { message = "User with this email already exists" });
        }

        var user = new User
        {
            Id = request.Id,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        var userId = GetUserId();
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(new UserProfileResponse(
            user.Id,
            user.Email,
            user.Phone,
            user.FullName,
            user.CreatedAt
        ));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        var userId = GetUserId();
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound();

        user.Phone = request.Phone;
        user.FullName = request.FullName;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("me/addresses")]
    public async Task<ActionResult<IEnumerable<UserAddressResponse>>> GetAddresses()
    {
        var userId = GetUserId();
        var addresses = await _addressRepository.GetByUserIdAsync(userId);

        return Ok(addresses.Select(a => new UserAddressResponse(
            a.Id,
            a.Label,
            a.Country,
            a.City,
            a.Street,
            a.House,
            a.Apartment,
            a.PostalCode,
            a.IsDefault
        )));
    }

    [HttpPost("me/addresses")]
    public async Task<ActionResult<UserAddressResponse>> AddAddress([FromBody] CreateUserAddressRequest request)
    {
        var userId = GetUserId();
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound();

        var address = new UserAddress
        {
            UserId = userId,
            Label = request.Label,
            Country = request.Country,
            City = request.City,
            Street = request.Street,
            House = request.House,
            Apartment = request.Apartment,
            PostalCode = request.PostalCode,
            IsDefault = request.IsDefault
        };

        if (request.IsDefault)
        {
            var existingAddresses = await _addressRepository.GetByUserIdAsync(userId);
            foreach (var existingAddress in existingAddresses)
            {
                existingAddress.IsDefault = false;
                await _addressRepository.UpdateAsync(existingAddress);
            }
        }

        await _addressRepository.AddAsync(address);
        await _addressRepository.SaveChangesAsync();

        return Ok(new UserAddressResponse(
            address.Id,
            address.Label,
            address.Country,
            address.City,
            address.Street,
            address.House,
            address.Apartment,
            address.PostalCode,
            address.IsDefault
        ));
    }

    [HttpPut("me/addresses/{id}")]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateUserAddressRequest request)
    {
        var userId = GetUserId();
        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null || address.UserId != userId)
            return NotFound();

        address.Label = request.Label;
        address.Country = request.Country;
        address.City = request.City;
        address.Street = request.Street;
        address.House = request.House;
        address.Apartment = request.Apartment;
        address.PostalCode = request.PostalCode;

        if (request.IsDefault && !address.IsDefault)
        {
            var existingAddresses = await _addressRepository.GetByUserIdAsync(userId);
            foreach (var existingAddress in existingAddresses)
            {
                existingAddress.IsDefault = false;
                await _addressRepository.UpdateAsync(existingAddress);
            }
        }

        address.IsDefault = request.IsDefault;
        await _addressRepository.UpdateAsync(address);
        await _addressRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("me/addresses/{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = GetUserId();
        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null || address.UserId != userId)
            return NotFound();

        await _addressRepository.DeleteAsync(address);
        await _addressRepository.SaveChangesAsync();

        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new InvalidOperationException("User ID not found in claims");

        return userId;
    }
} 