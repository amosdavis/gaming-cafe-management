namespace GameCafe.Core.Models;

public class Session
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int StationId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public string? GameName { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public decimal TotalCost { get; set; }
    public int DurationMinutes => EndTime.HasValue 
        ? (int)(EndTime.Value - StartTime).TotalMinutes 
        : (int)(DateTime.UtcNow - StartTime).TotalMinutes;
}

public enum SessionStatus
{
    Active = 0,
    Paused = 1,
    Completed = 2,
    Cancelled = 3
}
