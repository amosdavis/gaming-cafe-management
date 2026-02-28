namespace GameCafe.Core.Models;

public class GameStation
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 5000;
    public StationStatus Status { get; set; } = StationStatus.Offline;
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    public int? CurrentSessionId { get; set; }
    public bool IsAvailable => Status == StationStatus.Available;
    public decimal HourlyRate { get; set; } = 5.00m;
    public bool IsActive { get; set; } = true;
}

public enum StationStatus
{
    Offline = 0,
    Available = 1,
    InUse = 2,
    Maintenance = 3
}
