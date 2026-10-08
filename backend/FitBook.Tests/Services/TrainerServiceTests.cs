using FitBook.Api.Dtos;
using FitBook.Api.Exceptions;
using FitBook.Api.Models;
using FitBook.Api.Services;
using FitBook.Tests.Helpers;

namespace FitBook.Tests.Services;

public class TrainerServiceTests : IDisposable
{
    private readonly TestDatabase _database = new();

    private TrainerService CreateService() => new(_database.CreateContext(), _database.Time);

    [Fact]
    public async Task CreateAsync_TrimsValuesAndReturnsTrainer()
    {
        var result = await CreateService().CreateAsync(new TrainerRequest
        {
            FirstName = "  Anna ",
            LastName = "Stone",
            Specialty = TrainingType.Boxing,
            Bio = "Boxing coach"
        });

        Assert.Equal("Anna Stone", result.FullName);
        Assert.Equal(TrainingType.Boxing, result.Specialty);
        Assert.Equal(0, result.UpcomingSessions);
    }

    [Fact]
    public async Task GetAllAsync_FiltersBySpecialty()
    {
        _database.AddTrainer(TrainingType.Yoga);
        _database.AddTrainer(TrainingType.CrossFit);

        var result = await CreateService().GetAllAsync(TrainingType.CrossFit);

        Assert.Equal(TrainingType.CrossFit, Assert.Single(result).Specialty);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenTrainerHasSessions()
    {
        var trainer = _database.AddTrainer();
        _database.AddSession(trainer, TestDatabase.Now.AddDays(1));

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().DeleteAsync(trainer.Id));
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenTrainerDoesNotExist()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().DeleteAsync(123));
    }

    public void Dispose() => _database.Dispose();
}
