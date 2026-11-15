namespace SportsCenter.ViewModels
{
    // Một dòng trong bảng học viên (màn Lớp của tôi, dành cho Coach)
    public class StudentViewModel
    {
        public int MemberId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? PackageStatus { get; set; } // "Còn hạn", "Hết hạn", "Chưa có gói"...
    }
}
