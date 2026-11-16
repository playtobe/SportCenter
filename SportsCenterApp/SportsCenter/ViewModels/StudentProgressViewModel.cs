namespace SportsCenter.ViewModels
{
    // Một dòng trong bảng học viên (màn Số buổi học viên, dành cho Coach)
    public class StudentProgressViewModel
    {
        public int MemberId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public int Total { get; set; }          // tổng số buổi của lớp
        public int Present { get; set; }        // đã học (có mặt)
        public int Absent { get; set; }         // nghỉ (vắng)
        public int NotMarked { get; set; }      // đã qua nhưng chưa điểm danh
    }
}
