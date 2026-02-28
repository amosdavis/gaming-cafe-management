using GameCafe.Core.Models;
using GameCafe.Core.Communication;

namespace GameCafe.ManagementServer.Services;

/// <summary>
/// Manages stations across the gaming cafe network
/// </summary>
public interface IStationManagementService
{
    Task<GameStation> RegisterStationAsync(StationHeartbeatRequest request);
    Task<bool> UpdateStationStatusAsync(int stationId, StationStatus status);
    Task<List<GameStation>> GetAllStationsAsync();
    Task<GameStation?> GetStationAsync(int stationId);
    Task<bool> ProcessHeartbeatAsync(StationHeartbeatRequest request);
}

public class StationManagementService : IStationManagementService
{
    // TODO: Replace with database context
    private readonly Dictionary<int, GameStation> _stations = new();
    private int _stationIdCounter = 1;

    public async Task<GameStation> RegisterStationAsync(StationHeartbeatRequest request)
    {
        var station = new GameStation
        {
            Id = _stationIdCounter++,
            Name = request.StationName,
            IpAddress = request.IpAddress,
            Port = 5000,
            Status = StationStatus.Available,
            LastHeartbeat = DateTime.UtcNow,
            IsActive = true
        };

        _stations[station.Id] = station;
        return await Task.FromResult(station);
    }

    public async Task<bool> UpdateStationStatusAsync(int stationId, StationStatus status)
    {
        if (_stations.TryGetValue(stationId, out var station))
        {
            station.Status = status;
            station.LastHeartbeat = DateTime.UtcNow;
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    public async Task<List<GameStation>> GetAllStationsAsync()
    {
        return await Task.FromResult(_stations.Values.ToList());
    }

    public async Task<GameStation?> GetStationAsync(int stationId)
    {
        var found = _stations.TryGetValue(stationId, out var station);
        return await Task.FromResult(found ? station : null);
    }

    public async Task<bool> ProcessHeartbeatAsync(StationHeartbeatRequest request)
    {
        return await UpdateStationStatusAsync(request.StationId, 
            request.IsAvailable ? StationStatus.Available : StationStatus.InUse);
    }
}

/// <summary>
/// Manages multi-station billing aggregation
/// </summary>
public interface IMultiStationBillingService
{
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetStationRevenueAsync(int stationId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<(string GameName, int PlayCount, decimal Revenue)>> GetTopGamesAsync(int limit = 10);
    Task<bool> RecordBillingAsync(int sessionId, int stationId, decimal amount);
}

public class MultiStationBillingService : IMultiStationBillingService
{
    // TODO: Replace with database queries
    private readonly Dictionary<int, decimal> _sessionBilling = new();

    public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var total = _sessionBilling.Values.Sum();
        return await Task.FromResult(total);
    }

    public async Task<decimal> GetStationRevenueAsync(int stationId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // TODO: Filter by station and date range
        var revenue = _sessionBilling.Values.Sum();
        return await Task.FromResult(revenue);
    }

    public async Task<List<(string GameName, int PlayCount, decimal Revenue)>> GetTopGamesAsync(int limit = 10)
    {
        // TODO: Aggregate from database by game name
        return await Task.FromResult(new List<(string, int, decimal)>());
    }

    public async Task<bool> RecordBillingAsync(int sessionId, int stationId, decimal amount)
    {
        _sessionBilling[sessionId] = amount;
        return await Task.FromResult(true);
    }
}

/// <summary>
/// Manages session synchronization across stations
/// </summary>
public interface ISessionSyncService
{
    Task<bool> SyncSessionAsync(SessionSyncRequest request);
    Task<List<Session>> GetActiveSessionsAsync();
    Task<List<Session>> GetSessionsByStationAsync(int stationId);
}

public class SessionSyncService : ISessionSyncService
{
    // TODO: Replace with database context
    private readonly Dictionary<int, Session> _sessions = new();

    public async Task<bool> SyncSessionAsync(SessionSyncRequest request)
    {
        var session = new Session
        {
            Id = request.SessionId,
            UserId = request.UserId,
            StationId = request.StationId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            GameName = request.GameName,
            TotalCost = request.TotalCost
        };

        _sessions[session.Id] = session;
        return await Task.FromResult(true);
    }

    public async Task<List<Session>> GetActiveSessionsAsync()
    {
        return await Task.FromResult(
            _sessions.Values.Where(s => s.Status == SessionStatus.Active).ToList()
        );
    }

    public async Task<List<Session>> GetSessionsByStationAsync(int stationId)
    {
        return await Task.FromResult(
            _sessions.Values.Where(s => s.StationId == stationId).ToList()
        );
    }
}
