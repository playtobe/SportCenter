namespace SportsCenter.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public string InvoiceNo { get; set; } = null!;

    public int MemberId { get; set; }

    public int? SubscriptionId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime PaidAt { get; set; }

    public int? ReceivedBy { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual Subscription? Subscription { get; set; }
}
