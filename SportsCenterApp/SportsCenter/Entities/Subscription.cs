namespace SportsCenter.Entities;

public partial class Subscription
{
    public int SubscriptionId { get; set; }

    public int MemberId { get; set; }

    public int PackageId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;

    public virtual MembershipPackage Package { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
