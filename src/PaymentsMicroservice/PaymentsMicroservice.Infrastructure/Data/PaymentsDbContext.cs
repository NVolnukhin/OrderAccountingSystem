using Microsoft.EntityFrameworkCore;
using PaymentsMicroservice.Domain.Entities;

namespace PaymentsMicroservice.Infrastructure.Data;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
                
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .IsRequired();
                
            entity.Property(e => e.OrderId)
                .IsRequired();
                
            entity.Property(e => e.CreatedAt)
                .IsRequired();
                
            // Add index for OrderId
            entity.HasIndex(e => e.OrderId)
                .HasDatabaseName("IX_Payments_OrderId");
                
            // Add index for Status
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Payments_Status");
        });
    }
} 