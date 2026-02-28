namespace GameCafe.Core.Models;

public class BillingRate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BillingModel BillingType { get; set; } = BillingModel.Hourly;
    public decimal Rate { get; set; }
    public int? DurationMinutes { get; set; }
    public int StationId { get; set; }
    public string? GameCategory { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum BillingModel
{
    Hourly = 0,
    PerMinute = 1,
    FlatRate = 2
}
