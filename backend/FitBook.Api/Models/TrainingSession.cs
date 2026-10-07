namespace FitBook.Api.Models;

public class TrainingSession
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TrainingType Type { get; set; }
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public int Capacity { get; set; }
    public string Location { get; set; } = string.Empty;

    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public DateTime EndTime => StartTime.AddMinutes(DurationMinutes);
}
