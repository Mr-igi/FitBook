using FitBook.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitBook.Api.Data;

/// <summary>
/// Applies migrations and fills an empty database with an admin account and demo data.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbSeeder));

        await db.Database.MigrateAsync();

        await SeedAdminAsync(db, hasher, config, logger);

        if (env.IsDevelopment())
        {
            await SeedDemoDataAsync(db, hasher, config);
        }
    }

    private static async Task SeedAdminAsync(AppDbContext db, IPasswordHasher<User> hasher, IConfiguration config, ILogger logger)
    {
        if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            return;
        }

        var email = config["Seed:AdminEmail"];
        var password = config["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No admin account was created. Set Seed:AdminEmail and Seed:AdminPassword in the configuration.");
            return;
        }

        var admin = new User { FullName = "FitBook Admin", Email = email.Trim().ToLowerInvariant(), Role = UserRole.Admin };
        admin.PasswordHash = hasher.HashPassword(admin, password);
        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }

    private static async Task SeedDemoDataAsync(AppDbContext db, IPasswordHasher<User> hasher, IConfiguration config)
    {
        if (await db.Trainers.AnyAsync())
        {
            return;
        }

        var trainers = new List<Trainer>
        {
            new() { FirstName = "Sophia", LastName = "Bennett", Specialty = TrainingType.Yoga, Bio = "Certified Vinyasa and Yin yoga teacher with 8 years of experience. Focused on mobility, breathing and mindful movement." },
            new() { FirstName = "Daniel", LastName = "Reed", Specialty = TrainingType.Yoga, Bio = "Power yoga coach who helps athletes recover faster and build core strength." },
            new() { FirstName = "Marcus", LastName = "Cole", Specialty = TrainingType.CrossFit, Bio = "CrossFit Level 2 trainer. Loves high-intensity workouts, Olympic lifting and pushing people past their limits." },
            new() { FirstName = "Olivia", LastName = "Hart", Specialty = TrainingType.CrossFit, Bio = "Former competitive rower focused on conditioning, technique and safe progress for beginners." },
            new() { FirstName = "Jake", LastName = "Morrison", Specialty = TrainingType.Boxing, Bio = "Ex-amateur boxing champion. Teaches footwork, combinations and boxing conditioning for all levels." },
            new() { FirstName = "Nina", LastName = "Lopez", Specialty = TrainingType.Boxing, Bio = "Kickboxing and boxing coach specialised in cardio boxing and self-confidence training." }
        };
        db.Trainers.AddRange(trainers);

        // Demo members so the schedule already has some bookings and one full session.
        var memberPassword = config["Seed:MemberPassword"] ?? "Member123!";
        var members = new[]
        {
            ("Demo Member", "member@fitbook.com"),
            ("Emma Johnson", "emma@fitbook.com"),
            ("Liam Smith", "liam@fitbook.com"),
            ("Ava Wilson", "ava@fitbook.com")
        }
        .Select(m =>
        {
            var user = new User { FullName = m.Item1, Email = m.Item2, Role = UserRole.Member };
            user.PasswordHash = hasher.HashPassword(user, memberPassword);
            return user;
        })
        .ToList();
        db.Users.AddRange(members);

        // Weekly template: (hour, minute, type, title, trainer index, duration, capacity, location)
        var template = new (int Hour, int Minute, TrainingType Type, string Title, int Trainer, int Duration, int Capacity, string Location)[]
        {
            (7, 0, TrainingType.Yoga, "Morning Flow Yoga", 0, 60, 12, "Studio A"),
            (9, 30, TrainingType.CrossFit, "CrossFit Fundamentals", 3, 60, 10, "Box Area"),
            (12, 0, TrainingType.Boxing, "Lunch Cardio Boxing", 5, 45, 14, "Ring Room"),
            (17, 30, TrainingType.CrossFit, "CrossFit WOD", 2, 60, 12, "Box Area"),
            (18, 0, TrainingType.Yoga, "Power Yoga", 1, 60, 10, "Studio A"),
            (19, 30, TrainingType.Boxing, "Boxing Technique & Sparring", 4, 75, 3, "Ring Room")
        };

        var sessions = new List<TrainingSession>();
        var now = DateTime.UtcNow;
        for (var day = 0; day < 14; day++)
        {
            var date = DateTime.Today.AddDays(day);
            foreach (var (slot, index) in template.Select((slot, index) => (slot, index)))
            {
                // Keep the schedule varied: skip some slots on alternating days.
                if ((day + index) % 4 == 3)
                {
                    continue;
                }

                var start = date.AddHours(slot.Hour).AddMinutes(slot.Minute).ToUniversalTime();
                if (start <= now)
                {
                    continue;
                }

                sessions.Add(new TrainingSession
                {
                    Title = slot.Title,
                    Type = slot.Type,
                    Description = Descriptions[slot.Type],
                    StartTime = start,
                    DurationMinutes = slot.Duration,
                    Capacity = slot.Capacity,
                    Location = slot.Location,
                    Trainer = trainers[slot.Trainer]
                });
            }
        }
        db.TrainingSessions.AddRange(sessions);
        await db.SaveChangesAsync();

        // Fill the first small sparring session completely and add a few random bookings.
        var bookings = new List<Booking>();
        var sparring = sessions.FirstOrDefault(s => s.Capacity == 3);
        if (sparring is not null)
        {
            bookings.AddRange(members.Skip(1).Select(m => new Booking { User = m, TrainingSession = sparring }));
        }

        var random = new Random(42);
        foreach (var session in sessions.Where(s => s != sparring).Take(20))
        {
            foreach (var member in members.Skip(1).Where(_ => random.NextDouble() < 0.5))
            {
                bookings.Add(new Booking { User = member, TrainingSession = session, CreatedAt = now.AddHours(-random.Next(1, 120)) });
            }
        }

        db.Bookings.AddRange(bookings);
        await db.SaveChangesAsync();
    }

    private static readonly Dictionary<TrainingType, string> Descriptions = new()
    {
        [TrainingType.Yoga] = "Improve flexibility, balance and focus. Mats are provided, all levels welcome.",
        [TrainingType.CrossFit] = "High-intensity functional training combining weightlifting, gymnastics and cardio.",
        [TrainingType.Boxing] = "Learn punches, footwork and defence while building serious cardio. Bring your own wraps."
    };
}
