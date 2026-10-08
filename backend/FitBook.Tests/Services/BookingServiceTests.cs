using FitBook.Api.Exceptions;
using FitBook.Api.Services;
using FitBook.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FitBook.Tests.Services;

public class BookingServiceTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private static DateTime Now => TestDatabase.Now;

    private BookingService CreateService() => new(_database.CreateContext(), _database.Time);

    [Fact]
    public async Task BookAsync_CreatesBooking_WhenSpotsAreAvailable()
    {
        var member = _database.AddUser();
        var session = _database.AddSession(_database.AddTrainer(), Now.AddDays(1), capacity: 5);

        var result = await CreateService().BookAsync(member.Id, session.Id);

        Assert.Equal(session.Id, result.Session.Id);
        Assert.Equal(1, result.Session.BookedCount);
        Assert.Equal(4, result.Session.AvailableSpots);
        Assert.True(result.CanCancel);

        using var db = _database.CreateContext();
        Assert.Equal(1, await db.Bookings.CountAsync());
    }

    [Fact]
    public async Task BookAsync_Throws_WhenSessionIsFull()
    {
        var session = _database.AddSession(_database.AddTrainer(), Now.AddDays(1), capacity: 2);
        _database.AddBooking(_database.AddUser("Member 1"), session);
        _database.AddBooking(_database.AddUser("Member 2"), session);
        var lateMember = _database.AddUser("Member 3");

        var ex = await Assert.ThrowsAsync<ConflictException>(() => CreateService().BookAsync(lateMember.Id, session.Id));

        Assert.Equal("This session is full.", ex.Message);
    }

    [Fact]
    public async Task BookAsync_LastSpotMakesSessionFull()
    {
        var session = _database.AddSession(_database.AddTrainer(), Now.AddDays(1), capacity: 1);

        var result = await CreateService().BookAsync(_database.AddUser().Id, session.Id);

        Assert.True(result.Session.IsFull);
        Assert.Equal(0, result.Session.AvailableSpots);
    }

    [Fact]
    public async Task BookAsync_Throws_WhenMemberAlreadyBookedSession()
    {
        var member = _database.AddUser();
        var session = _database.AddSession(_database.AddTrainer(), Now.AddDays(1));
        _database.AddBooking(member, session);

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().BookAsync(member.Id, session.Id));
    }

    [Fact]
    public async Task BookAsync_Throws_WhenSessionAlreadyStarted()
    {
        var member = _database.AddUser();
        var session = _database.AddSession(_database.AddTrainer(), Now.AddHours(-1));

        await Assert.ThrowsAsync<BadRequestException>(() => CreateService().BookAsync(member.Id, session.Id));
    }

    [Fact]
    public async Task BookAsync_Throws_WhenSessionDoesNotExist()
    {
        var member = _database.AddUser();

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().BookAsync(member.Id, 999));
    }

    [Fact]
    public async Task CancelAsync_RemovesBooking_BeforeDeadline()
    {
        var member = _database.AddUser();
        var session = _database.AddSession(_database.AddTrainer(), Now.AddHours(5));
        var booking = _database.AddBooking(member, session);

        await CreateService().CancelAsync(member.Id, booking.Id);

        using var db = _database.CreateContext();
        Assert.False(await db.Bookings.AnyAsync());
    }

    [Fact]
    public async Task CancelAsync_Throws_WhenInsideCancellationWindow()
    {
        var member = _database.AddUser();
        var session = _database.AddSession(_database.AddTrainer(), Now.AddMinutes(90));
        var booking = _database.AddBooking(member, session);

        await Assert.ThrowsAsync<BadRequestException>(() => CreateService().CancelAsync(member.Id, booking.Id));

        using var db = _database.CreateContext();
        Assert.True(await db.Bookings.AnyAsync());
    }

    [Fact]
    public async Task CancelAsync_Throws_WhenBookingBelongsToAnotherMember()
    {
        var owner = _database.AddUser("Owner");
        var otherMember = _database.AddUser("Other");
        var session = _database.AddSession(_database.AddTrainer(), Now.AddDays(1));
        var booking = _database.AddBooking(owner, session);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().CancelAsync(otherMember.Id, booking.Id));
    }

    [Fact]
    public async Task CancelAsync_FreesSpotForAnotherMember()
    {
        var session = _database.AddSession(_database.AddTrainer(), Now.AddDays(1), capacity: 1);
        var first = _database.AddUser("First");
        var second = _database.AddUser("Second");
        var booking = _database.AddBooking(first, session);

        await CreateService().CancelAsync(first.Id, booking.Id);
        var result = await CreateService().BookAsync(second.Id, session.Id);

        Assert.Equal(1, result.Session.BookedCount);
    }

    [Fact]
    public async Task GetMyBookingsAsync_ReturnsOnlyBookingsOfMember_OrderedByStart()
    {
        var member = _database.AddUser("Me");
        var other = _database.AddUser("Other");
        var trainer = _database.AddTrainer();
        var later = _database.AddSession(trainer, Now.AddDays(3));
        var sooner = _database.AddSession(trainer, Now.AddDays(1));
        _database.AddBooking(member, later);
        _database.AddBooking(member, sooner);
        _database.AddBooking(other, sooner);

        var result = await CreateService().GetMyBookingsAsync(member.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(sooner.Id, result[0].Session.Id);
        Assert.Equal(later.Id, result[1].Session.Id);
        Assert.Equal(2, result[0].Session.BookedCount);
    }

    public void Dispose() => _database.Dispose();
}
