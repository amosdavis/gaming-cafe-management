using System.Net.Http.Json;

namespace GameCafe.Core.Communication;

/// <summary>
/// DTOs for Station Agent ↔ Management Server communication
/// </summary>

public class StationHeartbeatRequest
{
    public int StationId { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int CurrentSessionId { get; set; }
    public bool IsAvailable { get; set; }
}

public class StationStatusResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? ServerTime { get; set; }
}

public class SessionSyncRequest
{
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public int StationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? GameName { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TotalCost { get; set; }
}

public class SessionSyncResponse
{
    public bool Success { get; set; }
    public int SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UserAuthRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int StationId { get; set; }
}

public class UserAuthResponse
{
    public bool Success { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal AccountBalance { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class BillingRequest
{
    public int SessionId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class BillingResponse
{
    public bool Success { get; set; }
    public decimal Amount { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// HTTP-based client for Station Agent to communicate with Management Server
/// </summary>
public interface IManagementServerClient
{
    Task<StationStatusResponse> RegisterStationAsync(StationHeartbeatRequest request);
    Task<StationStatusResponse> SendHeartbeatAsync(StationHeartbeatRequest request);
    Task<UserAuthResponse> AuthenticateUserAsync(UserAuthRequest request);
    Task<SessionSyncResponse> SyncSessionAsync(SessionSyncRequest request);
    Task<BillingResponse> ProcessBillingAsync(BillingRequest request);
}

public class ManagementServerClient : IManagementServerClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ManagementServerClient(string baseUrl = "http://localhost:5000")
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<StationStatusResponse> RegisterStationAsync(StationHeartbeatRequest request)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/stations/register", request);
            return await response.Content.ReadFromJsonAsync<StationStatusResponse>() 
                ?? new StationStatusResponse { Success = false, Message = "Invalid response" };
        }
        catch (Exception ex)
        {
            return new StationStatusResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<StationStatusResponse> SendHeartbeatAsync(StationHeartbeatRequest request)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/stations/heartbeat", request);
            return await response.Content.ReadFromJsonAsync<StationStatusResponse>()
                ?? new StationStatusResponse { Success = false, Message = "Invalid response" };
        }
        catch (Exception ex)
        {
            return new StationStatusResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<UserAuthResponse> AuthenticateUserAsync(UserAuthRequest request)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/auth/login", request);
            return await response.Content.ReadFromJsonAsync<UserAuthResponse>()
                ?? new UserAuthResponse { Success = false, Message = "Invalid response" };
        }
        catch (Exception ex)
        {
            return new UserAuthResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<SessionSyncResponse> SyncSessionAsync(SessionSyncRequest request)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/sessions/sync", request);
            return await response.Content.ReadFromJsonAsync<SessionSyncResponse>()
                ?? new SessionSyncResponse { Success = false, Message = "Invalid response" };
        }
        catch (Exception ex)
        {
            return new SessionSyncResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<BillingResponse> ProcessBillingAsync(BillingRequest request)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/billing/process", request);
            return await response.Content.ReadFromJsonAsync<BillingResponse>()
                ?? new BillingResponse { Success = false, Message = "Invalid response" };
        }
        catch (Exception ex)
        {
            return new BillingResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }
}
