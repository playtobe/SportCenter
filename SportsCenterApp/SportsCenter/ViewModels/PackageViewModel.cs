namespace SportsCenter.ViewModels
{
    // Một dòng trong bảng "Danh sách gói" (màn Gói thành viên)
    public class PackageViewModel
    {
        public int PackageId { get; set; }          // cột Id bị ẩn, dùng để biết đang chọn gói nào
        public string? PackageName { get; set; }
        public int DurationDays { get; set; }
        public decimal Price { get; set; }
        public string? Status { get; set; }         // "Đang bán" hoặc "Ngừng bán"
    }
}
