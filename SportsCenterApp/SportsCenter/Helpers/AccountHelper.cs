using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SportsCenter
{
    // Tạo tài khoản Member và gắn vào hồ sơ đã đăng ký tại quầy (nếu có).
    // Không có MessageBox nào ở đây: phần hỏi xác nhận nằm ở RegisterWindow.
    public static class AccountHelper
    {
        // Bỏ dấu, đổi thành chữ thường, gộp khoảng trắng: "  Nguyễn  Văn Đạt " == "nguyen van dat"
        public static string Normalize(string text)
        {
            string lower = text.Trim().ToLowerInvariant().Replace('đ', 'd');
            var sb = new StringBuilder();
            foreach (char ch in lower.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        // Các hồ sơ member trùng CẢ họ tên LẪN số điện thoại (không phân biệt hoa thường và dấu), kể cả hồ sơ đã có tài khoản.
        // Coi là cùng một người: không tạo thêm hồ sơ.
        public static List<Member> FindSamePerson(SportsCenterContext db, string fullName, string phone)
        {
            string name = Normalize(fullName);
            return db.Members.AsNoTracking()
                .Where(m => m.Phone == phone)
                .ToList()
                .Where(m => Normalize(m.FullName) == name)
                .ToList();
        }

        // Hồ sơ khác đang dùng cùng số điện thoại nhưng khác họ tên (có thể là người nhà dùng chung số): chỉ cảnh báo
        public static List<Member> FindSamePhoneOtherName(SportsCenterContext db, string fullName, string phone)
        {
            string name = Normalize(fullName);
            return db.Members.AsNoTracking()
                .Where(m => m.Phone == phone)
                .ToList()
                .Where(m => Normalize(m.FullName) != name)
                .ToList();
        }

        // Email đơn giản: có @ và có dấu chấm ở phần sau, không có khoảng trắng
        public static bool IsValidEmail(string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        // Hồ sơ đăng ký tại quầy (chưa có tài khoản) khớp CẢ số điện thoại LẪN họ tên.
        // Chỉ biết số điện thoại thì không đủ để tìm ra hồ sơ.
        public static List<Member> FindCounterProfiles(SportsCenterContext db, string fullName, string phone)
        {
            string name = Normalize(fullName);
            return db.Members
                .Where(m => m.UserId == null && m.Phone == phone)
                .ToList()
                .Where(m => Normalize(m.FullName) == name)
                .ToList();
        }

        // Tạo tài khoản Member. counterProfile != null: gắn tài khoản vào hồ sơ quầy đó (giữ nguyên gói, hóa đơn, lớp đã đăng ký);
        // counterProfile == null: tạo hồ sơ Member mới. Cả hai trường hợp lưu trong một lần SaveChanges.
        public static User CreateMemberAccount(SportsCenterContext db, string username, string password,
            string fullName, string phone, Member? counterProfile)
        {
            if (counterProfile != null && counterProfile.UserId != null)
            {
                throw new InvalidOperationException("Hồ sơ này đã có tài khoản.");
            }

            var user = new User
            {
                Username = username,
                PasswordHash = PasswordHelper.Hash(password),
                FullName = fullName,
                Phone = phone,
                Role = "Member",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            if (counterProfile == null)
            {
                db.Members.Add(new Member { User = user, FullName = fullName, Phone = phone, CreatedAt = DateTime.Now });
            }
            else
            {
                if (db.Entry(counterProfile).State == EntityState.Detached)
                    db.Members.Attach(counterProfile);
                db.Users.Add(user);
                counterProfile.User = user;
            }

            db.SaveChanges();
            return user;
        }
    }
}
