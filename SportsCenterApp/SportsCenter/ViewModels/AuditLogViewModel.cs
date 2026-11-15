namespace SportsCenter.ViewModels
{
    // Một dòng trong bảng "Lịch sử" (màn Lịch sử hoạt động)
    public class AuditLogViewModel
    {
        public DateTime At { get; set; }
        public string? User { get; set; }           // tên đăng nhập, "-" nếu không có
        public string? Action { get; set; }         // tên hành động tiếng Việt
        public string? Details { get; set; }
    }
}
