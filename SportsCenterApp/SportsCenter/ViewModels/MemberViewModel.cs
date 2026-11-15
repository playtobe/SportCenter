namespace SportsCenter.ViewModels
{
    // Một dòng trong bảng "Danh sách member" (màn Member)
    public class MemberViewModel
    {
        public int MemberId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? HasAccount { get; set; }     // "Có" hoặc "Chưa có"
        public string? PackageName { get; set; }    // gói hiện tại hoặc gần nhất, "—" nếu chưa có
        public DateOnly? EndDate { get; set; }
        public string? Status { get; set; }         // "Còn hạn", "Hết hạn", "Chưa có gói", "Đã hủy"
    }
}
