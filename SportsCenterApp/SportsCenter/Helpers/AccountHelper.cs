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

        // Mỗi số điện thoại chỉ thuộc về MỘT member (kể cả hồ sơ đã có tài khoản hay đăng ký tại quầy).
        // Trả về hồ sơ đang dùng số này, hoặc null nếu số còn trống.
        public static Member? FindByPhone(SportsCenterContext db, string phone) =>
            db.Members.AsNoTracking().OrderBy(m => m.MemberId).FirstOrDefault(m => m.Phone == phone);

        // Kết quả kiểm tra số điện thoại khi tự đăng ký tài khoản
        public enum PhoneCheck
        {
            Free,                // số chưa có ai dùng: tạo hồ sơ mới
            LinkCounterProfile,  // số thuộc hồ sơ tại quầy (chưa có tài khoản) và đúng họ tên: hỏi gắn vào hồ sơ đó
            AlreadyRegistered    // số đã thuộc về người khác (có tài khoản, hoặc hồ sơ quầy khác họ tên): không cho đăng ký
        }

        public static (PhoneCheck Result, Member? Profile) CheckPhoneForRegistration(SportsCenterContext db, string fullName, string phone)
        {
            if (FindByPhone(db, phone) == null) return (PhoneCheck.Free, null);
            var counter = FindCounterProfiles(db, fullName, phone);
            if (counter.Count > 0) return (PhoneCheck.LinkCounterProfile, counter[0]);
            return (PhoneCheck.AlreadyRegistered, null);
        }

        // Email đơn giản: có @ và có dấu chấm ở phần sau, không có khoảng trắng
        public static bool IsValidEmail(string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        // Tên đăng nhập: 3-30 ký tự, chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới (không có khoảng trắng). Trả về lỗi hoặc null.
        public static string? UsernameError(string username)
        {
            if (username.Length < 3 || username.Length > 30) return "Tên đăng nhập gồm 3-30 ký tự.";
            if (!Regex.IsMatch(username, @"^[A-Za-z0-9._]+$"))
                return "Tên đăng nhập chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới (không có khoảng trắng).";
            return null;
        }

        // Họ tên: 2-100 ký tự, chỉ gồm chữ cái (có dấu), khoảng trắng, dấu chấm, dấu nháy hoặc gạch nối; không có số. Trả về lỗi hoặc null.
        public static string? FullNameError(string fullName)
        {
            string n = fullName.Trim().Normalize(NormalizationForm.FormC);
            if (n.Length < 2 || n.Length > 100) return "Họ tên gồm 2-100 ký tự.";
            if (!Regex.IsMatch(n, @"^\p{L}[\p{L}\p{M} .'’-]*$"))
                return "Họ tên chỉ gồm chữ cái và khoảng trắng, không có số hoặc ký tự đặc biệt.";
            return null;
        }

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
