namespace SportsCenter.Entities;

public partial class Sport
{
    public int SportId { get; set; }

    public string SportName { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
