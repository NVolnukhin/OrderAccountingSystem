using DeliveryMicroservice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryMicroservice.Infrastructure.Data;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options)
    {
    }

    public DeliveryDbContext() { }

    public DbSet<Delivery> Deliveries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.DeliveryId);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.TrackingNumber).IsRequired();
            entity.Property(e => e.Address).IsRequired();
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=DeliveryServiceDb;Username=postgres;Password=postgres");
        }
    }
} 