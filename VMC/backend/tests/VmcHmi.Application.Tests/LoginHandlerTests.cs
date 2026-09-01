using VmcHmi.Application.DTOs;
using VmcHmi.Application.Handlers;
using VmcHmi.Application.Queries;
using VmcHmi.Domain.Entities;

namespace VmcHmi.Application.Tests;

public class LoginHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCredentials_ReturnsToken()
    {
        var user = new User("operator", "dummy-hash");
        var users = new InMemoryUserRepository();
        users.Seed(user);

        var tokenService = new FakeTokenService();
        var hasher = new FakePasswordHasher("Operator@123");
        var handler = new LoginHandler(users, hasher, tokenService, new NullLogger<LoginHandler>());

        LoginResponse response = await handler.HandleAsync(new LoginQuery
        {
            Username = "operator",
            Password = "Operator@123"
        });

        Assert.False(string.IsNullOrEmpty(response.Token));
        Assert.Equal(tokenService.GeneratedToken, response.Token);
    }

    [Fact]
    public async Task HandleAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new User("operator", "dummy-hash");
        var users = new InMemoryUserRepository();
        users.Seed(user);

        var handler = new LoginHandler(
            users,
            new FakePasswordHasher("Operator@123"),
            new FakeTokenService(),
            new NullLogger<LoginHandler>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.HandleAsync(new LoginQuery { Username = "operator", Password = "wrong-password" }));
    }

    [Fact]
    public async Task HandleAsync_WithUnknownUser_ThrowsUnauthorizedAccessException()
    {
        var users = new InMemoryUserRepository();
        var handler = new LoginHandler(
            users,
            new FakePasswordHasher("Operator@123"),
            new FakeTokenService(),
            new NullLogger<LoginHandler>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.HandleAsync(new LoginQuery { Username = "nobody", Password = "Operator@123" }));
    }
}
