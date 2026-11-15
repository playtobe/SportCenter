namespace SportsCenter.ViewModels
{
    // Một dòng trong bảng "Chọn gói" (màn Mua / gia hạn gói), chỉ gồm gói đang bán nên không có cột trạng thái
    public class PackageChoiceViewModel
    {
        public int PackageId { get; set; }
        public string? PackageName { get; set; }
        public int DurationDays { get; set; }
        public decimal Price { get; set; }
    }
}
