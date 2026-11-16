namespace SportsCenter.ViewModels
{
    // Một dòng trong bảng "Các lớp của tôi" (màn Số buổi học, dành cho Member)
    public class MyProgressViewModel
    {
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? SportName { get; set; }
        public int Total { get; set; }          // tổng số buổi của lớp
        public int Present { get; set; }        // đã học (có mặt)
        public int Absent { get; set; }         // nghỉ (vắng)
        public int NotMarked { get; set; }      // đã qua nhưng chưa điểm danh
        public int Remaining { get; set; }      // chưa tới ngày học
    }
}
