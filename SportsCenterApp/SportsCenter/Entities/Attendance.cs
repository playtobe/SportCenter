namespace SportsCenter.Entities;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int RegistrationId { get; set; }

    public DateOnly SessionDate { get; set; }

    // Present hoặc Absent
    public string Status { get; set; } = null!;

    public int? CheckedInBy { get; set; }

    public string? Note { get; set; }

    public virtual Registration Registration { get; set; } = null!;
}
