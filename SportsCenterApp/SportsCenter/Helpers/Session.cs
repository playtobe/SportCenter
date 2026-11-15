using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;

namespace SportsCenter
{
    // =========================================================================
    // LỚP QUẢN LÝ PHIÊN ĐĂNG NHẬP (SESSION)
    // Tác dụng: Lưu thông tin người dùng đang đăng nhập trong suốt vòng đời ứng dụng.
    // Các màn hình và trang khác (MainWindow, MySchedulePage, CoachPage...) đều lấy từ đây.
    // =========================================================================
    public static class Session
    {
        // 1. Lưu thông tin tài khoản User hiện tại đang đăng nhập
        public static User? CurrentUser { get; set; }

        // 2. Lưu hồ sơ hội viên nếu tài khoản đăng nhập có vai trò là Member
        public static Member? CurrentMember { get; set; }

        // 3. Lưu hồ sơ huấn luyện viên nếu tài khoản đăng nhập có vai trò là Coach
        public static Coach? CurrentCoach { get; set; }

        // 4. Lấy vai trò (Role) của người dùng hiện tại (trả về chuỗi rỗng nếu chưa đăng nhập)
        public static string Role
        {
            get
            {
                // Nếu CurrentUser tồn tại thì lấy Role, ngược lại trả về chuỗi rỗng
                if (CurrentUser != null)
                {
                    return CurrentUser.Role;
                }
                return string.Empty;
            }
        }

        // =====================================================================
        // Hàm thiết lập phiên đăng nhập khi tài khoản đăng nhập thành công
        // =====================================================================
        public static void SignIn(SportsCenterContext db, User user)
        {
            // Bước 1: Gán thông tin User vào biến static CurrentUser
            CurrentUser = user;

            // Bước 2: Nếu vai trò là Member, tìm và nạp hồ sơ Member tương ứng trong DB
            if (user.Role == "Member")
            {
                CurrentMember = db.Members.AsNoTracking().FirstOrDefault(m => m.UserId == user.UserId);
            }
            else
            {
                CurrentMember = null;
            }

            // Bước 3: Nếu vai trò là Coach, tìm và nạp hồ sơ Coach tương ứng trong DB
            if (user.Role == "Coach")
            {
                CurrentCoach = db.Coaches.AsNoTracking().FirstOrDefault(c => c.UserId == user.UserId);
            }
            else
            {
                CurrentCoach = null;
            }

            // Bước 4: Tự động ghi lại lịch sử hoạt động (Audit Log) việc đăng nhập
            AuditHelper.Log(db, "Login", "Đăng nhập hệ thống");
        }

        // =====================================================================
        // Hàm xóa phiên đăng nhập khi người dùng bấm Đăng xuất (Logout)
        // =====================================================================
        public static void Clear()
        {
            // Gán tất cả về null để dọn sạch phiên làm việc của người dùng cũ
            CurrentUser = null;
            CurrentMember = null;
            CurrentCoach = null;
        }
    }
}
