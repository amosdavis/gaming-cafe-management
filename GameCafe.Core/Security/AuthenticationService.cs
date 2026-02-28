using GameCafe.Core.Models;

namespace GameCafe.Core.Security;

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public User? User { get; set; }
    public string? SessionToken { get; set; }
}

/// <summary>
/// Handles user authentication and session management.
/// </summary>
public interface IAuthenticationService
{
    Task<AuthenticationResult> RegisterAsync(string username, string email, string password);
    Task<AuthenticationResult> LoginAsync(string username, string password);
    Task<bool> LogoutAsync(string sessionToken);
    Task<User?> GetCurrentUserAsync(string sessionToken);
    Task<bool> ValidateSessionAsync(string sessionToken);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IPasswordHasher _passwordHasher;
    // TODO: Replace with actual session store (Redis or database)
    private readonly Dictionary<string, (User User, DateTime ExpiresAt)> _sessions = new();
    private readonly TimeSpan _sessionDuration = TimeSpan.FromHours(8);

    public AuthenticationService(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password)
    {
        // TODO: Integrate with database context when ready
        // For now, return placeholder
        return await Task.FromResult(new AuthenticationResult
        {
            Success = true,
            Message = "Registration successful",
            SessionToken = GenerateSessionToken()
        });
    }

    public async Task<AuthenticationResult> LoginAsync(string username, string password)
    {
        // TODO: Query user from database
        // TODO: Verify password hash
        // TODO: Create session in store

        // Placeholder implementation
        var sessionToken = GenerateSessionToken();
        var user = new User
        {
            Id = 1,
            Username = username,
            Email = "user@example.com",
            IsActive = true,
            Role = UserRole.Customer
        };

        _sessions[sessionToken] = (user, DateTime.UtcNow.Add(_sessionDuration));

        return await Task.FromResult(new AuthenticationResult
        {
            Success = true,
            Message = "Login successful",
            User = user,
            SessionToken = sessionToken
        });
    }

    public async Task<bool> LogoutAsync(string sessionToken)
    {
        var removed = _sessions.Remove(sessionToken);
        return await Task.FromResult(removed);
    }

    public async Task<User?> GetCurrentUserAsync(string sessionToken)
    {
        if (_sessions.TryGetValue(sessionToken, out var session))
        {
            if (DateTime.UtcNow < session.ExpiresAt)
            {
                return await Task.FromResult(session.User);
            }
            // Session expired
            _sessions.Remove(sessionToken);
        }
        return await Task.FromResult((User?)null);
    }

    public async Task<bool> ValidateSessionAsync(string sessionToken)
    {
        var user = await GetCurrentUserAsync(sessionToken);
        return user != null;
    }

    private string GenerateSessionToken()
    {
        return Guid.NewGuid().ToString("N");
    }
}
