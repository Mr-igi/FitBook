namespace FitBook.Api.Models;

public class Trainer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public TrainingType Specialty { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public ICollection<TrainingSession> Sessions { get; set; } = new List<TrainingSession>();

    public string FullName => $"{FirstName} {LastName}";
}
