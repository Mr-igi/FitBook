using FitBook.Api.Dtos;
using FitBook.Api.Exceptions;
using FitBook.Api.Models;
using FitBook.Api.Options;
using FitBook.Api.Services;
using FitBook.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitBook.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly TestDatabase _database = new();

    private AuthService CreateService()
    {
        var jwt = Microsoft.Extensions.Options.Options.Create(new JwtSettings
        {
            Key = "test-secret-key-that-is-long-enough-123456",
            Issuer = "FitBook.Tests",
            Audience = "FitBook.Tests",
            ExpiresInMinutes = 60
        });
        return new AuthService(_database.CreateContext(), new PasswordHasher<User>(), new TokenService(jwt, _database.Time));
    }

    private static RegisterRequest NewRegistration(string email = "john@example.com") => new()
    {
        FullName = "John Doe",
        Email = email,
        Password = "Secret123!"
    };

    [Fact]
    public async Task RegisterAsync_CreatesMemberWithHashedPassword()
    {
        var result = await CreateService().RegisterAsync(NewRegistration());

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("Member", result.User.Role);

        using var db = _database.CreateContext();
        var user = await db.Users.SingleAsync();
        Assert.Equal(UserRole.Member, user.Role);
        Assert.NotEqual("Secret123!", user.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenEmailAlreadyExists_IgnoringCase()
    {
        await CreateService().RegisterAsync(NewRegistration("john@example.com"));

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().RegisterAsync(NewRegistration("  JOHN@example.com ")));
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_ForValidCredentials()
    {
        await CreateService().RegisterAsync(NewRegistration());

        var result = await CreateService().LoginAsync(new LoginRequest { Email = "john@example.com", Password = "Secret123!" });

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("john@example.com", result.User.Email);
        Assert.Equal(TestDatabase.Now.AddMinutes(60), result.ExpiresAt);
    }

    [Theory]
    [InlineData("john@example.com", "WrongPassword")]
    [InlineData("nobody@example.com", "Secret123!")]
    public async Task LoginAsync_Throws_ForInvalidCredentials(string email, string password)
    {
        await CreateService().RegisterAsync(NewRegistration());

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            CreateService().LoginAsync(new LoginRequest { Email = email, Password = password }));
    }

    public void Dispose() => _database.Dispose();
}
