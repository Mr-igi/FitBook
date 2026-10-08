using FitBook.Api.Dtos;
using FitBook.Api.Exceptions;
using FitBook.Api.Models;
using FitBook.Api.Services;
using FitBook.Tests.Helpers;

namespace FitBook.Tests.Services;

public class TrainingSessionServiceTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private static DateTime Now => TestDatabase.Now;

    private TrainingSessionService CreateService() => new(_database.CreateContext(), _database.Time);

    private static SessionRequest NewRequest(Trainer trainer, DateTime start) => new()
    {
        Title = "Evening Class",
        Type = trainer.Specialty,
        StartTime = start,
        DurationMinutes = 60,
        Capacity = 8,
        Location = "Studio B",
        TrainerId = trainer.Id
    };

    [Fact]
    public async Task CreateAsync_CreatesSession()
    {
        var trainer = _database.AddTrainer(TrainingType.CrossFit);

        var result = await CreateService().CreateAsync(NewRequest(trainer, Now.AddDays(2)));

        Assert.Equal("Evening Class", result.Title);
        Assert.Equal(TrainingType.CrossFit, result.Type);
        Assert.Equal(8, result.AvailableSpots);
        Assert.False(result.IsFull);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenStartIsInThePast()
    {
        var trainer = _database.AddTrainer();

        await Assert.ThrowsAsync<BadRequestException>(() => CreateService().CreateAsync(NewRequest(trainer, Now.AddMinutes(-5))));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenTrainerSpecialtyDoesNotMatchType()
    {
        var yogaTrainer = _database.AddTrainer(TrainingType.Yoga);
        var request = NewRequest(yogaTrainer, Now.AddDays(1));
        request.Type = TrainingType.Boxing;

        await Assert.ThrowsAsync<BadRequestException>(() => CreateService().CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenTrainerHasOverlappingSession()
    {
        var trainer = _database.AddTrainer();
        _database.AddSession(trainer, Now.AddDays(1), durationMinutes: 60);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService().CreateAsync(NewRequest(trainer, Now.AddDays(1).AddMinutes(30))));
    }

    [Fact]
    public async Task CreateAsync_AllowsSessionRightAfterPreviousOne()
    {
        var trainer = _database.AddTrainer();
        _database.AddSession(trainer, Now.AddDays(1), durationMinutes: 60);

        var result = await CreateService().CreateAsync(NewRequest(trainer, Now.AddDays(1).AddMinutes(60)));

        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenCapacityIsLowerThanBookings()
    {
        var trainer = _database.AddTrainer();
        var session = _database.AddSession(trainer, Now.AddDays(1), capacity: 5);
        _database.AddBooking(_database.AddUser("A"), session);
        _database.AddBooking(_database.AddUser("B"), session);

        var request = NewRequest(trainer, session.StartTime);
        request.Capacity = 1;

        await Assert.ThrowsAsync<BadRequestException>(() => CreateService().UpdateAsync(session.Id, request));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAvailabilityAndCurrentUserBooking()
    {
        var trainer = _database.AddTrainer();
        var member = _database.AddUser("Me");
        var fullSession = _database.AddSession(trainer, Now.AddDays(1), capacity: 1);
        var openSession = _database.AddSession(trainer, Now.AddDays(2), capacity: 3);
        var booking = _database.AddBooking(member, fullSession);

        var result = await CreateService().GetAllAsync(new SessionQuery(), member.Id);

        var full = result.Single(s => s.Id == fullSession.Id);
        Assert.True(full.IsFull);
        Assert.Equal(booking.Id, full.MyBookingId);
        Assert.True(full.CanCancel);

        var open = result.Single(s => s.Id == openSession.Id);
        Assert.False(open.IsFull);
        Assert.Null(open.MyBookingId);
        Assert.Equal(3, open.AvailableSpots);
    }

    [Fact]
    public async Task GetAllAsync_HidesPastSessions_AndFiltersByType()
    {
        var yoga = _database.AddTrainer(TrainingType.Yoga);
        var boxing = _database.AddTrainer(TrainingType.Boxing);
        _database.AddSession(yoga, Now.AddDays(-1));
        var upcomingYoga = _database.AddSession(yoga, Now.AddDays(1));
        _database.AddSession(boxing, Now.AddDays(1));

        var result = await CreateService().GetAllAsync(new SessionQuery { Type = TrainingType.Yoga });

        var single = Assert.Single(result);
        Assert.Equal(upcomingYoga.Id, single.Id);
    }

    [Fact]
    public async Task GetParticipantsAsync_ReturnsBookedMembers()
    {
        var session = _database.AddSession(_database.AddTrainer(), Now.AddDays(1));
        _database.AddBooking(_database.AddUser("Alice"), session);
        _database.AddBooking(_database.AddUser("Bob"), session);

        var result = await CreateService().GetParticipantsAsync(session.Id);

        Assert.Equal(["Alice", "Bob"], result.Select(p => p.FullName).Order());
    }

    public void Dispose() => _database.Dispose();
}
