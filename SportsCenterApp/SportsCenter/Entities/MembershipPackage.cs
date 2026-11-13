namespace SportsCenter.Entities;

public partial class MembershipPackage
{
    public int PackageId { get; set; }

    public string PackageName { get; set; } = null!;

    public int DurationDays { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
