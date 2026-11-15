using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SportsCenter
{
    // =========================================================================
    // LỚP HỖ TRỢ XỬ LÝ TÀI KHOẢN VÀ HỒ SƠ (ACCOUNT HELPER)
    // Tác dụng:
    // 1. Chuẩn hóa chuỗi họ tên (bỏ dấu tiếng Việt, loại bỏ khoảng trắng thừa).
    // 2. Kiểm tra tính hợp lệ của Email, Username, FullName.
    // 3. Kiểm tra số điện thoại khi đăng ký (quy tắc duy nhất SĐT).
    // 4. Tạo tài khoản Member và gắn vào hồ sơ đăng ký tại quầy (nếu có).
    // Không chứa MessageBox trực tiếp: việc hỏi xác nhận thuộc về giao diện (UI).
    // =========================================================================
    public static class AccountHelper
    {
        // =====================================================================
        // Hàm chuẩn hóa chuỗi tiếng Việt: bỏ dấu, chuyển chữ thường, gộp khoảng trắng
        // Ví dụ: "  Nguyễn  Văn   Đạt  " -> "nguyen van dat"
        // Mục đích: So sánh tên khách hàng không phân biệt chữ hoa/thường hay dấu.
        // =====================================================================
        public static string Normalize(string text)
        {
            // Bước 1: Kiểm tra chuỗi rỗng
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            // Bước 2: Cắt khoảng trắng đầu cuối, chuyển về chữ thường, thay 'đ' thành 'd'
            string lowerText = text.Trim().ToLowerInvariant().Replace('đ', 'd');

            // Bước 3: Tách dấu tiếng Việt bằng chuẩn Unicode FormD (ký tự và dấu thành các phần tách biệt)
            string normalizedFormD = lowerText.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            // Bước 4: Lặp qua từng ký tự, chỉ giữ lại ký tự chữ cái, bỏ qua các dấu thanh (NonSpacingMark)
            foreach (char ch in normalizedFormD)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            // Bước 5: Tách theo dấu cách và gộp lại để chỉ còn 1 khoảng trắng giữa các từ
            string[] words = sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", words);
        }

        // =====================================================================
        // Tìm kiếm hồ sơ Member trong database dựa vào số điện thoại
        // Quy tắc: Mỗi số điện thoại chỉ thuộc về DUY NHẤT 1 hội viên
        // =====================================================================
        public static Member? FindByPhone(SportsCenterContext db, string phone)
        {
            // Bước 1: Kiểm tra số điện thoại truyền vào
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            // Bước 2: Truy vấn tìm Member đầu tiên khớp số điện thoại
            Member? member = db.Members
                .AsNoTracking()
                .OrderBy(m => m.MemberId)
                .FirstOrDefault(m => m.Phone == phone);

            return member;
        }

        // =====================================================================
        // Các trạng thái kết quả khi kiểm tra số điện thoại đăng ký tài khoản
        // =====================================================================
        public enum PhoneCheck
        {
            Free,                // Số điện thoại hoàn toàn mới, chưa ai dùng -> Tạo tài khoản và hồ sơ mới
            LinkCounterProfile,  // Số điện thoại trùng với hồ sơ tạo tại quầy (chưa có User) và đúng họ tên -> Cho phép liên kết
            AlreadyRegistered    // Số điện thoại đã có người khác đăng ký tài khoản -> Không cho đăng ký trùng
        }

        // =====================================================================
        // Kiểm tra số điện thoại khi khách hàng tự đăng ký tài khoản qua ứng dụng
        // =====================================================================
        public static (PhoneCheck Result, Member? Profile) CheckPhoneForRegistration(SportsCenterContext db, string fullName, string phone)
        {
            // Bước 1: Tìm xem số điện thoại này đã có trong danh sách Member chưa
            Member? existingMember = FindByPhone(db, phone);

            // Nếu số điện thoại chưa hề tồn tại trong hệ thống -> Trả về Free (số trống)
            if (existingMember == null)
            {
                return (PhoneCheck.Free, null);
            }

            // Bước 2: Nếu số điện thoại đã tồn tại, kiểm tra xem có phải hồ sơ quầy khớp họ tên không
            List<Member> counterProfiles = FindCounterProfiles(db, fullName, phone);
            if (counterProfiles.Count > 0)
            {
                // Khớp cả SĐT lẫn Họ tên tại quầy -> Gợi ý liên kết tài khoản
                return (PhoneCheck.LinkCounterProfile, counterProfiles[0]);
            }

            // Bước 3: Số này đã có tài khoản hoặc không khớp họ tên hồ sơ quầy -> Báo trùng
            return (PhoneCheck.AlreadyRegistered, null);
        }

        // =====================================================================
        // Kiểm tra định dạng Email hợp lệ bằng biểu thức chính quy (Regex)
        // =====================================================================
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            // Định dạng: có chữ trước @, có tên miền và dấu chấm sau @, không có dấu cách
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        // =====================================================================
        // Kiểm tra tính hợp lệ của Tên đăng nhập (Username)
        // Trả về chuỗi thông báo lỗi cụ thể, hoặc null nếu hợp lệ
        // =====================================================================
        public static string? UsernameError(string username)
        {
            // Bước 1: Kiểm tra độ dài từ 3 đến 30 ký tự
            if (username.Length < 3 || username.Length > 30)
            {
                return "Tên đăng nhập gồm 3-30 ký tự.";
            }

            // Bước 2: Kiểm tra ký tự chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới
            if (!Regex.IsMatch(username, @"^[A-Za-z0-9._]+$"))
            {
                return "Tên đăng nhập chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới (không có khoảng trắng).";
            }

            return null; // Không có lỗi
        }

        // =====================================================================
        // Kiểm tra tính hợp lệ của Họ và tên (FullName)
        // Trả về chuỗi thông báo lỗi cụ thể, hoặc null nếu hợp lệ
        // =====================================================================
        public static string? FullNameError(string fullName)
        {
            // Bước 1: Chuẩn hóa chuỗi họ tên sang chuẩn FormC
            string name = fullName.Trim().Normalize(NormalizationForm.FormC);

            // Bước 2: Kiểm tra độ dài từ 2 đến 100 ký tự
            if (name.Length < 2 || name.Length > 100)
            {
                return "Họ tên gồm 2-100 ký tự.";
            }

            // Bước 3: Kiểm tra chỉ gồm chữ cái tiếng Việt và khoảng trắng, không chứa số
            if (!Regex.IsMatch(name, @"^\p{L}[\p{L}\p{M} .'’-]*$"))
            {
                return "Họ tên chỉ gồm chữ cái và khoảng trắng, không có số hoặc ký tự đặc biệt.";
            }

            return null; // Không có lỗi
        }

        // =====================================================================
        // Tìm các hồ sơ đăng ký tại quầy (chưa có tài khoản UserId) khớp cả SĐT và Họ tên
        // =====================================================================
        public static List<Member> FindCounterProfiles(SportsCenterContext db, string fullName, string phone)
        {
            // Chuẩn hóa họ tên người dùng nhập vào để so sánh
            string normalizedInputName = Normalize(fullName);

            // Lấy danh sách Member có cùng số điện thoại và UserId đang là null (tạo tại quầy)
            List<Member> counterMembers = db.Members
                .Where(m => m.UserId == null && m.Phone == phone)
                .ToList();

            // Lọc ra các hồ sơ có họ tên chuẩn hóa khớp hoàn toàn
            List<Member> matchedMembers = counterMembers
                .Where(m => Normalize(m.FullName) == normalizedInputName)
                .ToList();

            return matchedMembers;
        }

        // =====================================================================
        // Tạo tài khoản Member trong hệ thống:
        // - Nếu counterProfile == null: Tạo User mới và tạo Member mới.
        // - Nếu counterProfile != null: Tạo User mới và gán vào Member đã có tại quầy.
        // =====================================================================
        public static User CreateMemberAccount(SportsCenterContext db, string username, string password,
            string fullName, string phone, Member? counterProfile)
        {
            // Bước 1: Kiểm tra nếu hồ sơ quầy đã có tài khoản rồi thì ném ngoại lệ
            if (counterProfile != null && counterProfile.UserId != null)
            {
                throw new InvalidOperationException("Hồ sơ này đã có tài khoản.");
            }

            // Bước 2: Khởi tạo đối tượng User mới với vai trò là "Member"
            User newUser = new User
            {
                Username = username,
                PasswordHash = PasswordHelper.Hash(password), // Băm mật khẩu bằng SHA-256
                FullName = fullName,
                Phone = phone,
                Role = "Member",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            // Bước 3: Xử lý theo từng trường hợp tạo mới hay gắn vào hồ sơ quầy
            if (counterProfile == null)
            {
                // Trường hợp 1: Tạo hồ sơ Member mới toanh đi kèm User
                Member newMember = new Member
                {
                    User = newUser,
                    FullName = fullName,
                    Phone = phone,
                    CreatedAt = DateTime.Now
                };
                db.Members.Add(newMember);
            }
            else
            {
                // Trường hợp 2: Gắn User mới vào hồ sơ quầy đã tồn tại
                if (db.Entry(counterProfile).State == EntityState.Detached)
                {
                    db.Members.Attach(counterProfile);
                }
                db.Users.Add(newUser);
                counterProfile.User = newUser;
            }

            // Bước 4: Lưu thay đổi vào cơ sở dữ liệu (tất cả gói trong 1 lần SaveChanges)
            db.SaveChanges();

            // Bước 5: Trả về đối tượng User vừa tạo thành công
            return newUser;
        }
    }
}
