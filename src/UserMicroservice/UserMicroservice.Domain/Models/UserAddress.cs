namespace UserMicroservice.Domain.Models;

public class UserAddress
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string? Label { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? House { get; set; }
    public string? Apartment { get; set; }
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
    public User User { get; set; } = null!;
} 