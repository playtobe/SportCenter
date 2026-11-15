namespace SportsCenter.ViewModels
{
    // Một lựa chọn trong ComboBox: Id là giá trị lấy ra, Text là chữ hiện cho người dùng.
    // Dùng cho ô Thứ, ô Ca học (màn Lớp học) và ô Chọn lớp (màn Lớp của tôi).
    public class OptionViewModel
    {
        public int Id { get; set; }
        public string? Text { get; set; }
    }
}
