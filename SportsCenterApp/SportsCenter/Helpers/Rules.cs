using SportsCenter.Entities;

namespace SportsCenter
{
    // Phần lõi của Rules: ngày hôm nay, tên hiển thị, đổi thứ và mã hóa đơn. Chủ file: Người 5.
    // Luật của từng phần nằm ở file riêng (cùng class Rules, khai báo partial) để mỗi người sửa file của mình:
    //   Rules.Packages.cs (Người 2: gói thành viên), Rules.Classes.cs (Người 3: ca trống, hủy lớp),
    //   Rules.Booking.cs (Người 4: đặt lớp, hủy đăng ký, buổi điểm danh).
    public static partial class Rules
    {
        public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

        public static readonly string[] DayNames =
            { "", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy", "Chủ Nhật" };

        public static readonly string[] DayShort = { "", "T2", "T3", "T4", "T5", "T6", "T7", "CN" };

        public static readonly Dictionary<string, string> RoleNames = new()
        {
            ["Manager"] = "Quản lý trung tâm",
            ["Receptionist"] = "Nhân viên lễ tân",
            ["Coach"] = "Huấn luyện viên",
            ["Member"] = "Thành viên"
        };

        // Phương thức thanh toán: khóa lưu trong DB -> tên hiển thị
        public static readonly Dictionary<string, string> MethodNames = new()
        {
            ["Cash"] = "Tiền mặt",
            ["BankTransfer"] = "Chuyển khoản",
            ["Card"] = "Thẻ"
        };

        public static readonly List<object> MethodList =
            MethodNames.Select(kv => (object)new { Key = kv.Key, Name = kv.Value }).ToList();

        // Hành động ghi trong Lịch sử hoạt động: mã lưu trong DB -> tên hiển thị
        // (thêm hành động mới thì thêm một dòng vào đúng nhóm của mình, nhóm theo thứ tự Người 1 → 5)
        public static readonly Dictionary<string, string> ActionNames = new()
        {
            // Người 1: tài khoản
            ["Login"] = "Đăng nhập",
            ["ChangePassword"] = "Đổi mật khẩu",
            ["CreateUser"] = "Tạo tài khoản",
            ["SetRole"] = "Đổi vai trò",
            ["ToggleUser"] = "Khóa/Mở tài khoản",
            ["LinkMember"] = "Liên kết hồ sơ",
            // Người 2: member, gói
            ["CreateMember"] = "Đăng ký tại quầy",
            ["CreatePackage"] = "Tạo gói",
            ["TogglePackage"] = "Mở/Ngừng bán gói",
            // Người 3: lớp học
            ["CreateSport"] = "Thêm bộ môn",
            ["DeleteSport"] = "Xóa bộ môn",
            ["CreateRoom"] = "Thêm phòng",
            ["DeleteRoom"] = "Xóa phòng",
            ["CreateClass"] = "Tạo lớp",
            // Người 4: đặt lớp, điểm danh
            ["CancelClass"] = "Hủy lớp/đăng ký",
            ["RegisterClass"] = "Đăng ký lớp",
            ["Attendance"] = "Điểm danh",
            // Người 5: thanh toán
            ["Payment"] = "Thanh toán"
        };

        public static string ActionName(string code) =>
            ActionNames.TryGetValue(code, out var name) ? name : code;

        // 1 = Thứ Hai ... 7 = Chủ Nhật
        public static int IsoDay(DateOnly d) => ((int)d.DayOfWeek + 6) % 7 + 1;

        public static int IsoDay(DateTime d) => ((int)d.DayOfWeek + 6) % 7 + 1;

        public static string ScheduleText(int day, string slotName, TimeOnly start, TimeOnly end) =>
            $"{DayShort[day]} · {slotName} ({start:HH\\:mm}-{end:HH\\:mm})";

        public static string ScheduleText(Class c) =>
            ScheduleText(c.DayOfWeek, c.Slot.SlotName, c.Slot.StartTime, c.Slot.EndTime);

        // Mã hóa đơn tiếp theo dạng INV-0001 (Y1)
        public static string NextInvoiceNo(SportsCenterContext db)
        {
            int n = db.Payments.Count() + 1;
            string no;
            do
            {
                no = "INV-" + n.ToString("D4");
                n++;
            } while (db.Payments.Any(p => p.InvoiceNo == no));
            return no;
        }
    }
}
