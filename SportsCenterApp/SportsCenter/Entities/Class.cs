namespace SportsCenter.Entities;

public partial class Class
{
    public int ClassId { get; set; }

    public string ClassCode { get; set; } = null!;

    public string ClassName { get; set; } = null!;

    public int SportId { get; set; }

    public int CoachId { get; set; }

    public int RoomId { get; set; }

    // 1 = Thứ Hai ... 7 = Chủ Nhật
    public byte DayOfWeek { get; set; }

    public int SlotId { get; set; }

    public int Capacity { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Coach Coach { get; set; } = null!;

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual Room Room { get; set; } = null!;

    public virtual TimeSlot Slot { get; set; } = null!;

    public virtual Sport Sport { get; set; } = null!;
}
