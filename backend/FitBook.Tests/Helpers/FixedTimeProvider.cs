namespace FitBook.Tests.Helpers;

/// <summary>TimeProvider with a fixed "now" so date-based rules are deterministic.</summary>
public class FixedTimeProvider(DateTime utcNow) : TimeProvider
{
    public DateTime UtcNow { get; set; } = utcNow;

    public override DateTimeOffset GetUtcNow() => new(UtcNow, TimeSpan.Zero);
}
