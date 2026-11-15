namespace SportsCenter.ViewModels
{
    // Một dòng trong bảng bộ môn (màn Bộ môn và phòng)
    public class SportViewModel
    {
        public int SportId { get; set; }
        public string? SportName { get; set; }
        public int ClassCount { get; set; }        // số lớp đang dùng bộ môn này
    }
}
