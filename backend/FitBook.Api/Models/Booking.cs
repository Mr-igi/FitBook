namespace FitBook.Api.Models;

public class Booking
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TrainingSessionId { get; set; }
    public TrainingSession TrainingSession { get; set; } = null!;
}
