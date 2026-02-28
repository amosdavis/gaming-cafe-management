using GameCafe.Core.Models;

namespace GameCafe.Core.Services;

/// <summary>
/// Manages gaming sessions: creation, time tracking, and lifecycle events.
/// </summary>
public interface ISessionService
{
    Task<Session> CreateSessionAsync(int userId, int stationId, string? gameName = null);
    Task<bool> EndSessionAsync(int sessionId);
    Task<Session?> GetActiveSessionAsync(int stationId);
    Task<int> GetSessionDurationMinutesAsync(int sessionId);
    event EventHandler<SessionEventArgs>? SessionStarted;
    event EventHandler<SessionEventArgs>? SessionEnded;
}

public class SessionEventArgs : EventArgs
{
    public Session Session { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class SessionService : ISessionService
{
    // TODO: Replace with database context
    private readonly Dictionary<int, Session> _sessions = new();
    private int _sessionIdCounter = 1;

    public event EventHandler<SessionEventArgs>? SessionStarted;
    public event EventHandler<SessionEventArgs>? SessionEnded;

    public async Task<Session> CreateSessionAsync(int userId, int stationId, string? gameName = null)
    {
        var session = new Session
        {
            Id = _sessionIdCounter++,
            UserId = userId,
            StationId = stationId,
            GameName = gameName,
            StartTime = DateTime.UtcNow,
            Status = SessionStatus.Active
        };

        _sessions[session.Id] = session;
        SessionStarted?.Invoke(this, new SessionEventArgs { Session = session });

        return await Task.FromResult(session);
    }

    public async Task<bool> EndSessionAsync(int sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.EndTime = DateTime.UtcNow;
            session.Status = SessionStatus.Completed;
            SessionEnded?.Invoke(this, new SessionEventArgs { Session = session });
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    public async Task<Session?> GetActiveSessionAsync(int stationId)
    {
        var session = _sessions.Values
            .FirstOrDefault(s => s.StationId == stationId && s.Status == SessionStatus.Active);
        return await Task.FromResult(session);
    }

    public async Task<int> GetSessionDurationMinutesAsync(int sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return await Task.FromResult(session.DurationMinutes);
        }
        return await Task.FromResult(0);
    }
}
