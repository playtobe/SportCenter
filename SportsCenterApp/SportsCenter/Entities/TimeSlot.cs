namespace SportsCenter.Entities;

public partial class TimeSlot
{
    public int SlotId { get; set; }

    public string SlotName { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
