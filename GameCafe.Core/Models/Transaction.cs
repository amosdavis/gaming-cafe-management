namespace GameCafe.Core.Models;

public class Transaction
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; } = TransactionType.SessionCharge;
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ExternalTransactionId { get; set; }
    public string? Notes { get; set; }
}

public enum TransactionType
{
    SessionCharge = 0,
    Payment = 1,
    Refund = 2,
    Adjustment = 3
}

public enum TransactionStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3
}
