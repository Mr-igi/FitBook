using FitBook.Api.Services;

namespace FitBook.Tests.Services;

public class BookingRulesTests
{
    private static readonly DateTime SessionStart = new(2030, 1, 15, 18, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(-24 * 60, true)] // a day before
    [InlineData(-121, true)]     // just before the deadline
    [InlineData(-120, true)]     // exactly at the deadline
    [InlineData(-119, false)]    // inside the 2h window
    [InlineData(10, false)]      // after the start
    public void CanCancel_RespectsTwoHourWindow(int minutesRelativeToStart, bool expected)
    {
        var now = SessionStart.AddMinutes(minutesRelativeToStart);

        Assert.Equal(expected, BookingRules.CanCancel(SessionStart, now));
    }

    [Fact]
    public void CancellationDeadline_IsTwoHoursBeforeStart()
    {
        Assert.Equal(SessionStart.AddHours(-2), BookingRules.CancellationDeadline(SessionStart));
    }
}
