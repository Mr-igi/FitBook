using FitBook.Api.Data;
using FitBook.Api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FitBook.Tests.Helpers;

/// <summary>
/// In-memory SQLite database. Unlike the EF InMemory provider it enforces
/// unique indexes, foreign keys and supports transactions.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    public static readonly DateTime Now = new(2030, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;

    public TestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public FixedTimeProvider Time { get; } = new(Now);

    public AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);

    public User AddUser(string name = "Test Member", UserRole role = UserRole.Member)
    {
        using var db = CreateContext();
        var user = new User
        {
            FullName = name,
            Email = $"{Guid.NewGuid():N}@test.com",
            PasswordHash = "hash",
            Role = role
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public Trainer AddTrainer(TrainingType specialty = TrainingType.Yoga)
    {
        using var db = CreateContext();
        var trainer = new Trainer { FirstName = "Test", LastName = "Trainer", Specialty = specialty, Bio = "Bio" };
        db.Trainers.Add(trainer);
        db.SaveChanges();
        return trainer;
    }

    public TrainingSession AddSession(Trainer trainer, DateTime start, int capacity = 10, int durationMinutes = 60)
    {
        using var db = CreateContext();
        var session = new TrainingSession
        {
            Title = $"{trainer.Specialty} class",
            Type = trainer.Specialty,
            StartTime = start,
            DurationMinutes = durationMinutes,
            Capacity = capacity,
            Location = "Studio A",
            TrainerId = trainer.Id
        };
        db.TrainingSessions.Add(session);
        db.SaveChanges();
        return session;
    }

    public Booking AddBooking(User user, TrainingSession session)
    {
        using var db = CreateContext();
        var booking = new Booking { UserId = user.Id, TrainingSessionId = session.Id, CreatedAt = Now };
        db.Bookings.Add(booking);
        db.SaveChanges();
        return booking;
    }

    public void Dispose() => _connection.Dispose();
}
