using GameCafe.Core.Security;
using GameCafe.Core.Services;

namespace GameCafe.Tests;

public class AuthenticationServiceTests
{
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        var passwordHasher = new PasswordHasher();
        _service = new AuthenticationService(passwordHasher);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        const string username = "testuser";
        const string password = "password123";

        // Act
        var result = await _service.LoginAsync(username, password);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal(username, result.User.Username);
    }

    [Fact]
    public async Task LoginAsync_ReturnsSessionToken()
    {
        // Act
        var result = await _service.LoginAsync("user", "pass");

        // Assert
        Assert.NotNull(result.SessionToken);
        Assert.NotEmpty(result.SessionToken);
    }

    [Fact]
    public async Task LoginAsync_CreatesDifferentTokensPerLogin()
    {
        // Act
        var result1 = await _service.LoginAsync("user1", "pass1");
        var result2 = await _service.LoginAsync("user2", "pass2");

        // Assert
        Assert.NotEqual(result1.SessionToken, result2.SessionToken);
    }

    [Fact]
    public async Task ValidateSessionAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var loginResult = await _service.LoginAsync("user", "pass");
        var token = loginResult.SessionToken!;

        // Act
        var isValid = await _service.ValidateSessionAsync(token);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateSessionAsync_WithInvalidToken_ReturnsFalse()
    {
        // Act
        var isValid = await _service.ValidateSessionAsync("invalid-token-12345");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithValidToken_ReturnsUser()
    {
        // Arrange
        var loginResult = await _service.LoginAsync("alice", "password");
        var token = loginResult.SessionToken!;

        // Act
        var user = await _service.GetCurrentUserAsync(token);

        // Assert
        Assert.NotNull(user);
        Assert.Equal("alice", user.Username);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithInvalidToken_ReturnsNull()
    {
        // Act
        var user = await _service.GetCurrentUserAsync("invalid-token");

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public async Task LogoutAsync_RemovesSession()
    {
        // Arrange
        var loginResult = await _service.LoginAsync("user", "pass");
        var token = loginResult.SessionToken!;

        // Act
        var logoutSuccess = await _service.LogoutAsync(token);
        var stillValid = await _service.ValidateSessionAsync(token);

        // Assert
        Assert.True(logoutSuccess);
        Assert.False(stillValid);
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidToken_ReturnsFalse()
    {
        // Act
        var result = await _service.LogoutAsync("invalid-token");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsSuccess()
    {
        // Act
        var result = await _service.RegisterAsync("newuser", "email@test.com", "password");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task LoginAsync_WithDifferentUsers_ReturnsDifferentUsers()
    {
        // Act
        var result1 = await _service.LoginAsync("user1", "pass");
        var result2 = await _service.LoginAsync("user2", "pass");

        // Assert
        Assert.NotEqual(result1.User!.Username, result2.User!.Username);
    }
}
