using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;

namespace SportsCenter
{
    // Lưu thông tin người đang đăng nhập (dùng chung cho mọi màn hình)
    public static class Session
    {
        public static User? CurrentUser { get; set; }

        // Có giá trị khi người đăng nhập là Member
        public static Member? CurrentMember { get; set; }

        // Có giá trị khi người đăng nhập là Coach
        public static Coach? CurrentCoach { get; set; }

        public static string Role => CurrentUser?.Role ?? "";

        public static void SignIn(SportsCenterContext db, User user)
        {
            CurrentUser = user;
            CurrentMember = user.Role == "Member" ? db.Members.AsNoTracking().FirstOrDefault(m => m.UserId == user.UserId) : null;
            CurrentCoach = user.Role == "Coach" ? db.Coaches.AsNoTracking().FirstOrDefault(c => c.UserId == user.UserId) : null;
            AuditHelper.Log(db, "Login", "Đăng nhập hệ thống");
        }

        public static void Clear()
        {
            CurrentUser = null;
            CurrentMember = null;
            CurrentCoach = null;
        }
    }
}
