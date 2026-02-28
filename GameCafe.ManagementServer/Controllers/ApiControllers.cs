using Microsoft.AspNetCore.Mvc;
using GameCafe.Core.Communication;
using GameCafe.ManagementServer.Services;

namespace GameCafe.ManagementServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly IStationManagementService _stationService;

    public StationsController(IStationManagementService stationService)
    {
        _stationService = stationService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<StationStatusResponse>> Register(StationHeartbeatRequest request)
    {
        try
        {
            var station = await _stationService.RegisterStationAsync(request);
            return Ok(new StationStatusResponse
            {
                Success = true,
                Message = $"Station {station.Name} registered successfully",
                ServerTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new StationStatusResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost("heartbeat")]
    public async Task<ActionResult<StationStatusResponse>> Heartbeat(StationHeartbeatRequest request)
    {
        try
        {
            var success = await _stationService.ProcessHeartbeatAsync(request);
            if (!success)
                return NotFound(new StationStatusResponse
                {
                    Success = false,
                    Message = "Station not found"
                });

            return Ok(new StationStatusResponse
            {
                Success = true,
                Message = "Heartbeat received",
                ServerTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new StationStatusResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<StationInfo>>> GetAll()
    {
        var stations = await _stationService.GetAllStationsAsync();
        var stationInfos = stations.Select(s => new StationInfo
        {
            Id = s.Id,
            Name = s.Name,
            IpAddress = s.IpAddress,
            Status = s.Status.ToString(),
            LastHeartbeat = s.LastHeartbeat
        }).ToList();

        return Ok(stationInfos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StationInfo>> GetById(int id)
    {
        var station = await _stationService.GetStationAsync(id);
        if (station == null)
            return NotFound();

        return Ok(new StationInfo
        {
            Id = station.Id,
            Name = station.Name,
            IpAddress = station.IpAddress,
            Status = station.Status.ToString(),
            LastHeartbeat = station.LastHeartbeat
        });
    }
}

public class StationInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastHeartbeat { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionSyncService _sessionService;

    public SessionsController(ISessionSyncService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpPost("sync")]
    public async Task<ActionResult<SessionSyncResponse>> Sync(SessionSyncRequest request)
    {
        try
        {
            var success = await _sessionService.SyncSessionAsync(request);
            return Ok(new SessionSyncResponse
            {
                Success = success,
                SessionId = request.SessionId,
                Message = "Session synced"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new SessionSyncResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<SessionInfo>>> GetAll()
    {
        var sessions = await _sessionService.GetActiveSessionsAsync();
        return Ok(sessions.Select(s => new SessionInfo
        {
            Id = s.Id,
            UserId = s.UserId,
            StationId = s.StationId,
            GameName = s.GameName,
            DurationMinutes = s.DurationMinutes,
            Cost = s.TotalCost
        }).ToList());
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<SessionInfo>>> GetActive()
    {
        var sessions = await _sessionService.GetActiveSessionsAsync();
        var sessionInfos = sessions.Select(s => new SessionInfo
        {
            Id = s.Id,
            UserId = s.UserId,
            StationId = s.StationId,
            GameName = s.GameName,
            DurationMinutes = s.DurationMinutes,
            Cost = s.TotalCost
        }).ToList();

        return Ok(sessionInfos);
    }

    [HttpGet("station/{stationId}")]
    public async Task<ActionResult<List<SessionInfo>>> GetByStation(int stationId)
    {
        var sessions = await _sessionService.GetSessionsByStationAsync(stationId);
        var sessionInfos = sessions.Select(s => new SessionInfo
        {
            Id = s.Id,
            UserId = s.UserId,
            StationId = s.StationId,
            GameName = s.GameName,
            DurationMinutes = s.DurationMinutes,
            Cost = s.TotalCost
        }).ToList();

        return Ok(sessionInfos);
    }
}

public class SessionInfo
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int StationId { get; set; }
    public string? GameName { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Cost { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly IMultiStationBillingService _billingService;

    public BillingController(IMultiStationBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpPost("process")]
    public async Task<ActionResult<BillingResponse>> Process(BillingRequest request)
    {
        try
        {
            var success = await _billingService.RecordBillingAsync(request.SessionId, 0, request.Amount);
            return Ok(new BillingResponse
            {
                Success = success,
                Amount = request.Amount,
                TransactionId = Guid.NewGuid().ToString(),
                Message = "Billing processed"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new BillingResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<RevenueInfo>> GetAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var total = await _billingService.GetTotalRevenueAsync(startDate, endDate);
        var topGames = await _billingService.GetTopGamesAsync(10);
        return Ok(new RevenueInfo
        {
            TotalRevenue = total,
            DateRange = $"{startDate?.Date:yyyy-MM-dd} to {endDate?.Date:yyyy-MM-dd}",
            TopGames = topGames.Select(g => new GameStat
            {
                GameName = g.GameName,
                PlayCount = g.PlayCount,
                Revenue = g.Revenue
            }).ToList()
        });
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueInfo>> GetRevenue([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var total = await _billingService.GetTotalRevenueAsync(startDate, endDate);
        var topGames = await _billingService.GetTopGamesAsync(10);

        return Ok(new RevenueInfo
        {
            TotalRevenue = total,
            DateRange = $"{startDate?.Date:yyyy-MM-dd} to {endDate?.Date:yyyy-MM-dd}",
            TopGames = topGames.Select(g => new GameStat
            {
                GameName = g.GameName,
                PlayCount = g.PlayCount,
                Revenue = g.Revenue
            }).ToList()
        });
    }
}

public class RevenueInfo
{
    public decimal TotalRevenue { get; set; }
    public string DateRange { get; set; } = string.Empty;
    public List<GameStat> TopGames { get; set; } = new();
}

public class GameStat
{
    public string GameName { get; set; } = string.Empty;
    public int PlayCount { get; set; }
    public decimal Revenue { get; set; }
}
