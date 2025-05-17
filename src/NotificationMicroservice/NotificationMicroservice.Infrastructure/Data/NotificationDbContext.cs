using Microsoft.EntityFrameworkCore;
using NotificationMicroservice.Domain.Entities;

namespace NotificationMicroservice.Infrastructure.Data
{
    public class NotificationDbContext : DbContext
    {
        public DbSet<Notification> Notifications { get; set; }

        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.NotificationType).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsRead).IsRequired();
            });
        }
    }
} 