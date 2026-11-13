namespace SportsCenter.Entities;

public partial class Coach
{
    public int CoachId { get; set; }

    public int? UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Specialty { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual User? User { get; set; }
}
