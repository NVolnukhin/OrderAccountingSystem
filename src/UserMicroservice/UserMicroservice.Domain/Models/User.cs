namespace UserMicroservice.Domain.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
} 