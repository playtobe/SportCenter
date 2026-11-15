namespace SportsCenter.ViewModels
{
    // Một dòng trong bảng phòng tập (màn Bộ môn và phòng)
    public class RoomViewModel
    {
        public int RoomId { get; set; }
        public string? RoomName { get; set; }
        public int Capacity { get; set; }          // sức chứa của phòng
        public int ClassCount { get; set; }        // số lớp đang dùng phòng này
    }
}
