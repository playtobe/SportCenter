namespace SportsCenter.ViewModels
{
    // Một dòng trong bảng "Danh sách lớp" (màn Lớp học)
    public class ClassViewModel
    {
        public int ClassId { get; set; }
        public string? ClassCode { get; set; }
        public string? ClassName { get; set; }
        public string? SportName { get; set; }
        public string? CoachName { get; set; }
        public string? RoomName { get; set; }
        public string? Schedule { get; set; }      // vd "T2 · Ca 2 (07:30-09:00)"
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Registered { get; set; }        // số học viên đang đăng ký
        public int Capacity { get; set; }          // sĩ số tối đa
        public string? Status { get; set; }        // "Mở" hoặc "Đã hủy"

        // Sĩ số hiển thị dạng "2/20" (tự tính từ hai thuộc tính trên)
        public string Slots => $"{Registered}/{Capacity}";
    }
}
