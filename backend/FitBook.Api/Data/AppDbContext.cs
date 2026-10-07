using FitBook.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitBook.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // All dates are stored in UTC. SQL Server does not keep DateTimeKind,
        // so mark values read from the database as UTC to serialize them with a "Z" suffix.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(150).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.Property(t => t.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(t => t.LastName).HasMaxLength(50).IsRequired();
            entity.Property(t => t.Bio).HasMaxLength(1000);
            entity.Property(t => t.ImageUrl).HasMaxLength(500);
            entity.Property(t => t.Specialty).HasConversion<string>().HasMaxLength(20);
            entity.Ignore(t => t.FullName);
        });

        modelBuilder.Entity<TrainingSession>(entity =>
        {
            entity.Property(s => s.Title).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Description).HasMaxLength(1000);
            entity.Property(s => s.Location).HasMaxLength(100);
            entity.Property(s => s.Type).HasConversion<string>().HasMaxLength(20);
            entity.Ignore(s => s.EndTime);
            entity.HasIndex(s => s.StartTime);

            entity.HasOne(s => s.Trainer)
                .WithMany(t => t.Sessions)
                .HasForeignKey(s => s.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            // A member can book the same session only once.
            entity.HasIndex(b => new { b.UserId, b.TrainingSessionId }).IsUnique();

            entity.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.TrainingSession)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
