using GameCafe.Core.Services;
using GameCafe.Core.Models;

namespace GameCafe.Tests;

public class SessionServiceTests
{
    private readonly SessionService _service = new();

    [Fact]
    public async Task CreateSessionAsync_CreatesSessionWithCorrectData()
    {
        // Arrange
        const int userId = 1;
        const int stationId = 1;
        const string gameName = "Elden Ring";

        // Act
        var session = await _service.CreateSessionAsync(userId, stationId, gameName);

        // Assert
        Assert.NotNull(session);
        Assert.Equal(userId, session.UserId);
        Assert.Equal(stationId, session.StationId);
        Assert.Equal(gameName, session.GameName);
        Assert.Equal(SessionStatus.Active, session.Status);
    }

    [Fact]
    public async Task CreateSessionAsync_FiresSessionStartedEvent()
    {
        // Arrange
        var eventFired = false;
        _service.SessionStarted += (sender, e) => eventFired = true;

        // Act
        await _service.CreateSessionAsync(1, 1, "Game");

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task EndSessionAsync_EndsActiveSession()
    {
        // Arrange
        var session = await _service.CreateSessionAsync(1, 1, "Game");

        // Act
        var result = await _service.EndSessionAsync(session.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(SessionStatus.Completed, session.Status);
        Assert.NotNull(session.EndTime);
    }

    [Fact]
    public async Task EndSessionAsync_FiresSessionEndedEvent()
    {
        // Arrange
        var session = await _service.CreateSessionAsync(1, 1, "Game");
        var eventFired = false;
        _service.SessionEnded += (sender, e) => eventFired = true;

        // Act
        await _service.EndSessionAsync(session.Id);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task EndSessionAsync_WithInvalidSessionId_ReturnsFalse()
    {
        // Act
        var result = await _service.EndSessionAsync(9999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetActiveSessionAsync_ReturnsActiveSession()
    {
        // Arrange
        const int stationId = 1;
        var session = await _service.CreateSessionAsync(1, stationId, "Game");

        // Act
        var retrievedSession = await _service.GetActiveSessionAsync(stationId);

        // Assert
        Assert.NotNull(retrievedSession);
        Assert.Equal(session.Id, retrievedSession.Id);
    }

    [Fact]
    public async Task GetActiveSessionAsync_WithNoActiveSession_ReturnsNull()
    {
        // Act
        var session = await _service.GetActiveSessionAsync(9999);

        // Assert
        Assert.Null(session);
    }

    [Fact]
    public async Task GetActiveSessionAsync_WithEndedSession_ReturnsNull()
    {
        // Arrange
        const int stationId = 1;
        var session = await _service.CreateSessionAsync(1, stationId, "Game");
        await _service.EndSessionAsync(session.Id);

        // Act
        var retrievedSession = await _service.GetActiveSessionAsync(stationId);

        // Assert
        Assert.Null(retrievedSession);
    }

    [Fact]
    public async Task GetSessionDurationMinutesAsync_CalculatesDurationCorrectly()
    {
        // Arrange
        var session = await _service.CreateSessionAsync(1, 1, "Game");
        await Task.Delay(100); // Wait 100ms

        // Act
        var duration = await _service.GetSessionDurationMinutesAsync(session.Id);

        // Assert
        Assert.Equal(0, duration); // Less than 1 minute
    }

    [Fact]
    public async Task CreateSessionAsync_WithoutGameName_Succeeds()
    {
        // Act
        var session = await _service.CreateSessionAsync(1, 1);

        // Assert
        Assert.NotNull(session);
        Assert.Null(session.GameName);
    }

    [Fact]
    public async Task CreateSessionAsync_MultipleSessionsDifferentStations()
    {
        // Arrange & Act
        var session1 = await _service.CreateSessionAsync(1, 1, "Game1");
        var session2 = await _service.CreateSessionAsync(2, 2, "Game2");

        // Assert
        Assert.NotEqual(session1.Id, session2.Id);
        Assert.NotEqual(session1.StationId, session2.StationId);
    }
}
