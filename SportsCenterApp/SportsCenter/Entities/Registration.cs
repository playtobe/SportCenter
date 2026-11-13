namespace SportsCenter.Entities;

public partial class Registration
{
    public int RegistrationId { get; set; }

    public int MemberId { get; set; }

    public int ClassId { get; set; }

    public DateTime RegisteredAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string Status { get; set; } = null!;

    public int? RegisteredBy { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Class Class { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
