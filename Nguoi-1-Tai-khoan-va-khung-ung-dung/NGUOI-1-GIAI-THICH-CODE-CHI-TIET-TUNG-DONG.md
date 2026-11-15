# GIẢI THÍCH TOÀN BỘ CODE TỪNG DÒNG – PHẦN NGƯỜI 1: TÀI KHOẢN VÀ KHUNG ỨNG DỤNG
## DỰ ÁN SPORTS CENTER (PRN212 - WPF & EF CORE 8)
### *Phiên bản chuẩn hóa tối đa theo phong cách PRN Q2, loại bỏ hoàn toàn mã thừa*

> **Phạm vi Người 1 phụ trách gồm 9 file C#**:
> 1. `Entities/User.cs` – Mô hình dữ liệu tài khoản
> 2. `Helpers/PasswordHelper.cs` – Tiện ích băm mật khẩu SHA-256 (tách 4 bước tuần tự)
> 3. `Helpers/Session.cs` – Quản lý phiên làm việc toàn cục
> 4. `Helpers/AccountHelper.cs` – Chuẩn hóa chuỗi, kiểm tra định dạng & liên kết hồ sơ quầy
> 5. `LoginWindow.xaml.cs` – Màn hình Đăng nhập (tinh gọn, không focus thừa)
> 6. `RegisterWindow.xaml.cs` – Màn hình Đăng ký tài khoản hội viên (tinh gọn)
> 7. `MainWindow.xaml.cs` – Khung giao diện chính & điều hướng theo Role (Shell)
> 8. `ChangePasswordWindow.xaml.cs` – Hộp thoại Đổi mật khẩu (loại bỏ Lambda focus)
> 9. `Pages/UsersPage.xaml.cs` – Quản lý người dùng cho Quản lý (chuẩn cấu trúc Q2 PRN212, foreach ComboBox, hàm ClearForm riêng)

---

# 1. FILE: `Entities/User.cs` (Mô hình Thực thể Tài khoản)

### Đường dẫn: `SportsCenterApp/SportsCenter/Entities/User.cs`

### Toàn bộ mã nguồn:
```csharp
  1: namespace SportsCenter.Entities;
  2: 
  3: // =========================================================================
  4: // THỰC THỂ TÀI KHOẢN NGƯỜI DÙNG (USER ENTITY)
  5: // Bảng đại diện: Users trong CSDL SQL Server
  6: // Người phụ trách: Người 1 (Tài khoản và Khung ứng dụng)
  7: // =========================================================================
  8: public partial class User
  9: {
 10:     // Khóa chính tự tăng (Primary Key)
 11:     public int UserId { get; set; }
 12: 
 13:     // Tên đăng nhập duy nhất (Username: 3-30 ký tự)
 14:     public string Username { get; set; } = null!;
 15: 
 16:     // Mật khẩu đã được băm SHA-256 (64 ký tự hex)
 17:     public string PasswordHash { get; set; } = null!;
 18: 
 19:     // Họ và tên đầy đủ của người dùng (2-100 ký tự)
 20:     public string FullName { get; set; } = null!;
 21: 
 22:     // Địa chỉ email (có thể null)
 23:     public string? Email { get; set; }
 24: 
 25:     // Số điện thoại liên hệ (10-11 số, duy nhất)
 26:     public string? Phone { get; set; }
 27: 
 28:     // Vai trò trong hệ thống: "Manager", "Receptionist", "Coach", "Member"
 29:     public string Role { get; set; } = null!;
 30: 
 31:     // Trạng thái tài khoản: true = Đang hoạt động, false = Bị khóa
 32:     public bool IsActive { get; set; }
 33: 
 34:     // Ngày giờ tạo tài khoản
 35:     public DateTime CreatedAt { get; set; }
 36: 
 37:     // Quan hệ 1-N: Danh sách các nhật ký hoạt động do User này thực hiện
 38:     public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
 39: 
 40:     // Quan hệ 1-1: Hồ sơ huấn luyện viên (nếu Role là Coach)
 41:     public virtual ICollection<Coach> Coaches { get; set; } = new List<Coach>();
 42: 
 43:     // Quan hệ 1-1: Hồ sơ hội viên (nếu Role là Member)
 44:     public virtual ICollection<Member> Members { get; set; } = new List<Member>();
 45: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 1**: `namespace SportsCenter.Entities;` – Khai báo không gian tên chứa các thực thể CSDL (Entity Models).
- **Dòng 8**: `public partial class User` – Định nghĩa lớp thực thể `User` tương ứng với bảng `Users` trong SQL Server.
- **Dòng 11**: `public int UserId { get; set; }` – Khóa chính (`Primary Key`), tự tăng (`IDENTITY(1,1)`).
- **Dòng 14**: `public string Username { get; set; } = null!;` – Tên đăng nhập duy nhất (`UNIQUE`).
- **Dòng 17**: `public string PasswordHash { get; set; } = null!;` – Chuỗi mã băm SHA-256 (64 ký tự hex in hoa).
- **Dòng 20**: `public string FullName { get; set; } = null!;` – Họ và tên đầy đủ người dùng.
- **Dòng 23**: `public string? Email { get; set; }` – Địa chỉ email (nullable).
- **Dòng 26**: `public string? Phone { get; set; }` – Số điện thoại liên lạc (duy nhất).
- **Dòng 29**: `public string Role { get; set; } = null!;` – Vai trò: `Manager`, `Receptionist`, `Coach`, `Member`.
- **Dòng 32**: `public bool IsActive { get; set; }` – Trạng thái hoạt động: `true` là đang hoạt động, `false` là bị khóa.
- **Dòng 35**: `public DateTime CreatedAt { get; set; }` – Ngày tạo tài khoản.
- **Dòng 38-44**: Thuộc tính điều hướng quan hệ: `AuditLogs` (1-N), `Coaches` (1-1), `Members` (1-1).

---

# 2. FILE: `Helpers/PasswordHelper.cs` (Tiện ích Băm Mật khẩu)

### Đường dẫn: `SportsCenterApp/SportsCenter/Helpers/PasswordHelper.cs`

### Toàn bộ mã nguồn:
```csharp
  1: using System.Security.Cryptography;
  2: using System.Text;
  3: 
  4: namespace SportsCenter
  5: {
  6:     // =========================================================================
  7:     // LỚP HỖ TRỢ BĂM MẬT KHẨU (PASSWORD HELPER)
  8:     // Tác dụng: Mã hóa mật khẩu thành chuỗi băm SHA-256 để lưu an toàn vào DB.
  9:     // Chuẩn PRN: Viết tách bạch từng bước, dễ đọc, dễ hiểu, có comment chi tiết.
 10:     // =========================================================================
 11:     public static class PasswordHelper
 12:     {
 13:         // Hàm băm mật khẩu thành chuỗi Hex in hoa (SHA-256)
 14:         // Ví dụ: mật khẩu "123456" -> băm ra chuỗi hex 64 ký tự khớp trong SQL
 15:         public static string Hash(string password)
 16:         {
 17:             // Bước 1: Nếu mật khẩu rỗng hoặc null thì trả về rỗng
 18:             if (string.IsNullOrEmpty(password))
 19:             {
 20:                 return string.Empty;
 21:             }
 22: 
 23:             // Bước 2: Chuyển chuỗi mật khẩu sang mảng byte UTF-8
 24:             byte[] inputBytes = Encoding.UTF8.GetBytes(password);
 25: 
 26:             // Bước 3: Sử dụng thuật toán SHA-256 để tính toán mã băm ra mảng byte
 27:             byte[] hashBytes = SHA256.HashData(inputBytes);
 28: 
 29:             // Bước 4: Chuyển mảng byte sang chuỗi Hex dạng chữ in hoa (64 ký tự)
 30:             string hexString = Convert.ToHexString(hashBytes);
 31: 
 32:             // Bước 5: Trả về kết quả chuỗi hex đã băm
 33:             return hexString;
 34:         }
 35:     }
 36: }
```

### Giải thích chi tiết từng dòng (4 bước băm SHA-256 tuần tự):
- **Dòng 11**: `public static class PasswordHelper` – Lớp tiện ích tĩnh.
- **Dòng 15**: `public static string Hash(string password)` – Hàm băm mật khẩu.
- **Dòng 18-21 (Bước 1)**: Kiểm tra nếu rỗng trả về `string.Empty`.
- **Dòng 24 (Bước 2)**: `byte[] inputBytes = Encoding.UTF8.GetBytes(password);` – Đổi sang mảng byte UTF-8.
- **Dòng 27 (Bước 3)**: `byte[] hashBytes = SHA256.HashData(inputBytes);` – Băm SHA-256 an toàn ra mảng 32 bytes.
- **Dòng 30 (Bước 4)**: `string hexString = Convert.ToHexString(hashBytes);` – Đổi sang chuỗi Hexadecimal 64 ký tự IN HOA.
- **Dòng 33**: Trả về `hexString` (khớp với `HASHBYTES('SHA2_256', ...)` của SQL Server).

---

# 3. FILE: `Helpers/Session.cs` (Quản lý Phiên Đăng nhập)

### Đường dẫn: `SportsCenterApp/SportsCenter/Helpers/Session.cs`

### Toàn bộ mã nguồn:
```csharp
  1: using Microsoft.EntityFrameworkCore;
  2: using SportsCenter.Entities;
  3: 
  4: namespace SportsCenter
  5: {
  6:     // =========================================================================
  7:     // LỚP QUẢN LÝ PHIÊN ĐĂNG NHẬP (SESSION)
  8:     // Tác dụng: Lưu thông tin người dùng đang đăng nhập trong suốt vòng đời ứng dụng.
  9:     // Các màn hình và trang khác (MainWindow, MySchedulePage, CoachPage...) đều lấy từ đây.
 10:     // =========================================================================
 11:     public static class Session
 12:     {
 13:         // 1. Lưu thông tin tài khoản User hiện tại đang đăng nhập
 14:         public static User? CurrentUser { get; set; }
 15: 
 16:         // 2. Lưu hồ sơ hội viên nếu tài khoản đăng nhập có vai trò là Member
 17:         public static Member? CurrentMember { get; set; }
 18: 
 19:         // 3. Lưu hồ sơ huấn luyện viên nếu tài khoản đăng nhập có vai trò là Coach
 20:         public static Coach? CurrentCoach { get; set; }
 21: 
 22:         // 4. Lấy vai trò (Role) của người dùng hiện tại (trả về chuỗi rỗng nếu chưa đăng nhập)
 23:         public static string Role
 24:         {
 25:             get
 26:             {
 27:                 // Nếu CurrentUser tồn tại thì lấy Role, ngược lại trả về chuỗi rỗng
 28:                 if (CurrentUser != null)
 29:                 {
 30:                     return CurrentUser.Role;
 31:                 }
 32:                 return string.Empty;
 33:             }
 34:         }
 35: 
 36:         // =====================================================================
 37:         // Hàm thiết lập phiên đăng nhập khi tài khoản đăng nhập thành công
 38:         // =====================================================================
 39:         public static void SignIn(SportsCenterContext db, User user)
 40:         {
 41:             // Bước 1: Gán thông tin User vào biến static CurrentUser
 42:             CurrentUser = user;
 43: 
 44:             // Bước 2: Nếu vai trò là Member, tìm và nạp hồ sơ Member tương ứng trong DB
 45:             if (user.Role == "Member")
 46:             {
 47:                 CurrentMember = db.Members.AsNoTracking().FirstOrDefault(m => m.UserId == user.UserId);
 48:             }
 49:             else
 50:             {
 51:                 CurrentMember = null;
 52:             }
 53: 
 54:             // Bước 3: Nếu vai trò là Coach, tìm và nạp hồ sơ Coach tương ứng trong DB
 55:             if (user.Role == "Coach")
 56:             {
 57:                 CurrentCoach = db.Coaches.AsNoTracking().FirstOrDefault(c => c.UserId == user.UserId);
 58:             }
 59:             else
 60:             {
 61:                 CurrentCoach = null;
 62:             }
 63: 
 64:             // Bước 4: Tự động ghi lại lịch sử hoạt động (Audit Log) việc đăng nhập
 65:             AuditHelper.Log(db, "Login", "Đăng nhập hệ thống");
 66:         }
 67: 
 68:         // =====================================================================
 69:         // Hàm xóa phiên đăng nhập khi người dùng bấm Đăng xuất (Logout)
 70:         // =====================================================================
 71:         public static void Clear()
 72:         {
 73:             // Gán tất cả về null để dọn sạch phiên làm việc của người dùng cũ
 74:             CurrentUser = null;
 75:             CurrentMember = null;
 76:             CurrentCoach = null;
 77:         }
 78:     }
 79: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 11**: `public static class Session` – Duy trì phiên làm việc toàn cục trong RAM.
- **Dòng 14**: `CurrentUser`: Bản ghi `User` đang đăng nhập.
- **Dòng 17**: `CurrentMember`: Bản ghi `Member` nếu vai trò là Member.
- **Dòng 20**: `CurrentCoach`: Bản ghi `Coach` nếu vai trò là Coach.
- **Dòng 23-34**: Thuộc tính `Role` với getter tường minh: Trả về `CurrentUser.Role` hoặc `string.Empty`.
- **Dòng 39-66**: `SignIn(db, user)`: Gán `CurrentUser`, tự động nạp `CurrentMember`/`CurrentCoach` tương ứng qua `AsNoTracking()`, ghi nhật ký qua `AuditHelper.Log`.
- **Dòng 71-77**: `Clear()`: Xóa sạch cả 3 biến về `null` khi đăng xuất.

---

# 4. FILE: `Helpers/AccountHelper.cs` (Xác thực, Chuẩn hóa & Gắn Tài khoản)

### Đường dẫn: `SportsCenterApp/SportsCenter/Helpers/AccountHelper.cs`

### Toàn bộ mã nguồn:
```csharp
  1: using Microsoft.EntityFrameworkCore;
  2: using SportsCenter.Entities;
  3: using System.Globalization;
  4: using System.Text;
  5: using System.Text.RegularExpressions;
  6: 
  7: namespace SportsCenter
  8: {
  9:     // =========================================================================
 10:     // LỚP HỖ TRỢ XỬ LÝ TÀI KHOẢN VÀ HỒ SƠ (ACCOUNT HELPER)
 11:     // Tác dụng:
 12:     // 1. Chuẩn hóa chuỗi họ tên (bỏ dấu tiếng Việt, loại bỏ khoảng trắng thừa).
 13:     // 2. Kiểm tra tính hợp lệ của Email, Username, FullName.
 14:     // 3. Kiểm tra số điện thoại khi đăng ký (quy tắc duy nhất SĐT).
 15:     // 4. Tạo tài khoản Member và gắn vào hồ sơ đăng ký tại quầy (nếu có).
 16:     // Không chứa MessageBox trực tiếp: việc hỏi xác nhận thuộc về giao diện (UI).
 17:     // =========================================================================
 18:     public static class AccountHelper
 19:     {
 20:         // =====================================================================
 21:         // Hàm chuẩn hóa chuỗi tiếng Việt: bỏ dấu, chuyển chữ thường, gộp khoảng trắng
 22:         // Ví dụ: "  Nguyễn  Văn   Đạt  " -> "nguyen van dat"
 23:         // Mục đích: So sánh tên khách hàng không phân biệt chữ hoa/thường hay dấu.
 24:         // =====================================================================
 25:         public static string Normalize(string text)
 26:         {
 27:             // Bước 1: Kiểm tra chuỗi rỗng
 28:             if (string.IsNullOrWhiteSpace(text))
 29:             {
 30:                 return string.Empty;
 31:             }
 32: 
 33:             // Bước 2: Cắt khoảng trắng đầu cuối, chuyển về chữ thường, thay 'đ' thành 'd'
 34:             string lowerText = text.Trim().ToLowerInvariant().Replace('đ', 'd');
 35: 
 36:             // Bước 3: Tách dấu tiếng Việt bằng chuẩn Unicode FormD (ký tự và dấu thành các phần tách biệt)
 37:             string normalizedFormD = lowerText.Normalize(NormalizationForm.FormD);
 38:             StringBuilder sb = new StringBuilder();
 39: 
 40:             // Bước 4: Lặp qua từng ký tự, chỉ giữ lại ký tự chữ cái, bỏ qua các dấu thanh (NonSpacingMark)
 41:             foreach (char ch in normalizedFormD)
 42:             {
 43:                 UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(ch);
 44:                 if (category != UnicodeCategory.NonSpacingMark)
 45:                 {
 46:                     sb.Append(ch);
 47:                 }
 48:             }
 49: 
 50:             // Bước 5: Tách theo dấu cách và gộp lại để chỉ còn 1 khoảng trắng giữa các từ
 51:             string[] words = sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
 52:             return string.Join(" ", words);
 53:         }
 54: 
 55:         // =====================================================================
 56:         // Tìm kiếm hồ sơ Member trong database dựa vào số điện thoại
 57:         // Quy tắc: Mỗi số điện thoại chỉ thuộc về DUY NHẤT 1 hội viên
 58:         // =====================================================================
 59:         public static Member? FindByPhone(SportsCenterContext db, string phone)
 60:         {
 61:             // Bước 1: Kiểm tra số điện thoại truyền vào
 62:             if (string.IsNullOrWhiteSpace(phone))
 63:             {
 64:                 return null;
 65:             }
 66: 
 67:             // Bước 2: Truy vấn tìm Member đầu tiên khớp số điện thoại
 68:             Member? member = db.Members
 69:                 .AsNoTracking()
 70:                 .OrderBy(m => m.MemberId)
 71:                 .FirstOrDefault(m => m.Phone == phone);
 72: 
 73:             return member;
 74:         }
 75: 
 76:         // =====================================================================
 77:         // Các trạng thái kết quả khi kiểm tra số điện thoại đăng ký tài khoản
 78:         // =====================================================================
 79:         public enum PhoneCheck
 80:         {
 81:             Free,                // Số điện thoại hoàn toàn mới, chưa ai dùng -> Tạo tài khoản và hồ sơ mới
 82:             LinkCounterProfile,  // Số điện thoại trùng với hồ sơ tạo tại quầy (chưa có User) và đúng họ tên -> Cho phép liên kết
 83:             AlreadyRegistered    // Số điện thoại đã có người khác đăng ký tài khoản -> Không cho đăng ký trùng
 84:         }
 85: 
 86:         // =====================================================================
 87:         // Kiểm tra số điện thoại khi khách hàng tự đăng ký tài khoản qua ứng dụng
 88:         // =====================================================================
 89:         public static (PhoneCheck Result, Member? Profile) CheckPhoneForRegistration(SportsCenterContext db, string fullName, string phone)
 90:         {
 91:             // Bước 1: Tìm xem số điện thoại này đã có trong danh sách Member chưa
 92:             Member? existingMember = FindByPhone(db, phone);
 93: 
 94:             // Nếu số điện thoại chưa hề tồn tại trong hệ thống -> Trả về Free (số trống)
 95:             if (existingMember == null)
 96:             {
 97:                 return (PhoneCheck.Free, null);
 98:             }
 99: 
100:             // Bước 2: Nếu số điện thoại đã tồn tại, kiểm tra xem có phải hồ sơ quầy khớp họ tên không
101:             List<Member> counterProfiles = FindCounterProfiles(db, fullName, phone);
102:             if (counterProfiles.Count > 0)
103:             {
104:                 // Khớp cả SĐT lẫn Họ tên tại quầy -> Gợi ý liên kết tài khoản
105:                 return (PhoneCheck.LinkCounterProfile, counterProfiles[0]);
106:             }
107: 
108:             // Bước 3: Số này đã có tài khoản hoặc không khớp họ tên hồ sơ quầy -> Báo trùng
109:             return (PhoneCheck.AlreadyRegistered, null);
110:         }
111: 
112:         // =====================================================================
113:         // Kiểm tra định dạng Email hợp lệ bằng biểu thức chính quy (Regex)
114:         // =====================================================================
115:         public static bool IsValidEmail(string email)
116:         {
117:             if (string.IsNullOrWhiteSpace(email))
118:             {
119:                 return false;
120:             }
121: 
122:             // Định dạng: có chữ trước @, có tên miền và dấu chấm sau @, không có dấu cách
123:             string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
124:             return Regex.IsMatch(email, pattern);
125:         }
126: 
127:         // =====================================================================
128:         // Kiểm tra tính hợp lệ của Tên đăng nhập (Username)
129:         // Trả về chuỗi thông báo lỗi cụ thể, hoặc null nếu hợp lệ
130:         // =====================================================================
131:         public static string? UsernameError(string username)
132:         {
133:             // Bước 1: Kiểm tra độ dài từ 3 đến 30 ký tự
134:             if (username.Length < 3 || username.Length > 30)
135:             {
136:                 return "Tên đăng nhập gồm 3-30 ký tự.";
137:             }
138: 
139:             // Bước 2: Kiểm tra ký tự chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới
140:             if (!Regex.IsMatch(username, @"^[A-Za-z0-9._]+$"))
141:             {
142:                 return "Tên đăng nhập chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới (không có khoảng trắng).";
143:             }
144: 
145:             return null; // Không có lỗi
146:         }
147: 
148:         // =====================================================================
149:         // Kiểm tra tính hợp lệ của Họ và tên (FullName)
150:         // Trả về chuỗi thông báo lỗi cụ thể, hoặc null nếu hợp lệ
151:         // =====================================================================
152:         public static string? FullNameError(string fullName)
153:         {
154:             // Bước 1: Chuẩn hóa chuỗi họ tên sang chuẩn FormC
155:             string name = fullName.Trim().Normalize(NormalizationForm.FormC);
156: 
157:             // Bước 2: Kiểm tra độ dài từ 2 đến 100 ký tự
158:             if (name.Length < 2 || name.Length > 100)
159:             {
160:                 return "Họ tên gồm 2-100 ký tự.";
161:             }
162: 
163:             // Bước 3: Kiểm tra chỉ gồm chữ cái tiếng Việt và khoảng trắng, không chứa số
164:             if (!Regex.IsMatch(name, @"^\p{L}[\p{L}\p{M} .'’-]*$"))
165:             {
166:                 return "Họ tên chỉ gồm chữ cái và khoảng trắng, không có số hoặc ký tự đặc biệt.";
167:             }
168: 
169:             return null; // Không có lỗi
170:         }
171: 
172:         // =====================================================================
173:         // Tìm các hồ sơ đăng ký tại quầy (chưa có tài khoản UserId) khớp cả SĐT và Họ tên
174:         // =====================================================================
175:         public static List<Member> FindCounterProfiles(SportsCenterContext db, string fullName, string phone)
176:         {
177:             // Chuẩn hóa họ tên người dùng nhập vào để so sánh
178:             string normalizedInputName = Normalize(fullName);
179: 
180:             // Lấy danh sách Member có cùng số điện thoại và UserId đang là null (tạo tại quầy)
181:             List<Member> counterMembers = db.Members
182:                 .Where(m => m.UserId == null && m.Phone == phone)
183:                 .ToList();
184: 
185:             // Lọc ra các hồ sơ có họ tên chuẩn hóa khớp hoàn toàn
186:             List<Member> matchedMembers = counterMembers
187:                 .Where(m => Normalize(m.FullName) == normalizedInputName)
188:                 .ToList();
189: 
190:             return matchedMembers;
191:         }
192: 
193:         // =====================================================================
194:         // Tạo tài khoản Member trong hệ thống:
195:         // - Nếu counterProfile == null: Tạo User mới và tạo Member mới.
196:         // - Nếu counterProfile != null: Tạo User mới và gán vào Member đã có tại quầy.
197:         // =====================================================================
198:         public static User CreateMemberAccount(SportsCenterContext db, string username, string password,
199:             string fullName, string phone, Member? counterProfile)
200:         {
201:             // Bước 1: Kiểm tra nếu hồ sơ quầy đã có tài khoản rồi thì ném ngoại lệ
202:             if (counterProfile != null && counterProfile.UserId != null)
203:             {
204:                 throw new InvalidOperationException("Hồ sơ này đã có tài khoản.");
205:             }
206: 
207:             // Bước 2: Khởi tạo đối tượng User mới với vai trò là "Member"
208:             User newUser = new User
209:             {
210:                 Username = username,
211:                 PasswordHash = PasswordHelper.Hash(password), // Băm mật khẩu bằng SHA-256
212:                 FullName = fullName,
213:                 Phone = phone,
214:                 Role = "Member",
215:                 IsActive = true,
216:                 CreatedAt = DateTime.Now
217:             };
218: 
219:             // Bước 3: Xử lý theo từng trường hợp tạo mới hay gắn vào hồ sơ quầy
220:             if (counterProfile == null)
221:             {
222:                 // Trường hợp 1: Tạo hồ sơ Member mới toanh đi kèm User
223:                 Member newMember = new Member
224:                 {
225:                     User = newUser,
226:                     FullName = fullName,
227:                     Phone = phone,
228:                     CreatedAt = DateTime.Now
229:                 };
230:                 db.Members.Add(newMember);
231:             }
232:             else
233:             {
234:                 // Trường hợp 2: Gắn User mới vào hồ sơ quầy đã tồn tại
235:                 if (db.Entry(counterProfile).State == EntityState.Detached)
236:                 {
237:                     db.Members.Attach(counterProfile);
238:                 }
239:                 db.Users.Add(newUser);
240:                 counterProfile.User = newUser;
241:             }
242: 
243:             // Bước 4: Lưu thay đổi vào cơ sở dữ liệu (tất cả gói trong 1 lần SaveChanges)
244:             db.SaveChanges();
245: 
246:             // Bước 5: Trả về đối tượng User vừa tạo thành công
247:             return newUser;
248:         }
249:     }
250: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 24-52**: `Normalize(string text)` – Chuẩn hóa tiếng Việt: cắt khoảng trắng đầu cuối, chuyển chữ thường, thay 'đ' thành 'd', phân tách `FormD`, lọc bỏ `NonSpacingMark`, gộp khoảng trắng thừa.
- **Dòng 58-73**: `FindByPhone(db, phone)` – Tìm Member theo số điện thoại trong DB.
- **Dòng 78-83**: `enum PhoneCheck` – 3 trạng thái: `Free` (số mới), `LinkCounterProfile` (có hồ sơ quầy khớp họ tên), `AlreadyRegistered` (đã đăng ký hoặc khác họ tên).
- **Dòng 88-111**: `CheckPhoneForRegistration` – Kiểm tra SĐT khi khách tự đăng ký tài khoản online.
- **Dòng 116-126**: `IsValidEmail(email)` – Kiểm tra regex email tiêu chuẩn.
- **Dòng 132-148**: `UsernameError(username)` – Kiểm tra username 3-30 ký tự, không dấu, không khoảng trắng.
- **Dòng 154-171**: `FullNameError(fullName)` – Kiểm tra họ tên 2-100 ký tự, chỉ gồm chữ cái và khoảng trắng, không chứa số.
- **Dòng 176-192**: `FindCounterProfiles` – Tìm hồ sơ tạo tại quầy (`UserId == null`) khớp cả SĐT và Họ tên đã chuẩn hóa.
- **Dòng 199-250**: `CreateMemberAccount` – Tạo tài khoản Member và gắn hồ sơ quầy (hoặc tạo mới) chỉ trong 1 lần `SaveChanges()`.

---

# 5. FILE: `LoginWindow.xaml.cs` (Code-behind Đăng nhập)

### Đường dẫn: `SportsCenterApp/SportsCenter/LoginWindow.xaml.cs`

### Toàn bộ mã nguồn:
```csharp
  1: using SportsCenter.Entities;
  2: using System;
  3: using System.Linq;
  4: using System.Windows;
  5: 
  6: namespace SportsCenter
  7: {
  8:     /// <summary>
  9:     /// Interaction logic for LoginWindow.xaml
 10:     /// Màn hình Đăng nhập hệ thống (Login)
 11:     /// </summary>
 12:     public partial class LoginWindow : Window
 13:     {
 14:         // 1. Khởi tạo DbContext kết nối Database (chuẩn PRN212)
 15:         private SportsCenterContext _db;
 16: 
 17:         public LoginWindow()
 18:         {
 19:             InitializeComponent();
 20:             _db = new SportsCenterContext(); // Khởi tạo kết nối DB
 21:         }
 22: 
 23:         // =====================================================================
 24:         // Sự kiện khi người dùng bấm nút "Đăng nhập" (btnLogin)
 25:         // =====================================================================
 26:         private void btnLogin_Click(object sender, RoutedEventArgs e)
 27:         {
 28:             // Bước 1: Lấy chuỗi Tên đăng nhập và Mật khẩu từ giao diện
 29:             string username = txtUsername.Text.Trim();
 30:             string password = txtPassword.Password;
 31: 
 32:             // Bước 2: Kiểm tra dữ liệu rỗng (Validation)
 33:             if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
 34:             {
 35:                 MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu.");
 36:                 return;
 37:             }
 38: 
 39:             try
 40:             {
 41:                 // Bước 3: Tìm tài khoản trong Database khớp với Username vừa nhập
 42:                 User? user = _db.Users.FirstOrDefault(u => u.Username == username);
 43: 
 44:                 // Bước 4: Kiểm tra nếu tài khoản không tồn tại HOẶC mật khẩu băm không trùng khớp
 45:                 string hashedPassword = PasswordHelper.Hash(password);
 46:                 if (user == null || user.PasswordHash != hashedPassword)
 47:                 {
 48:                     MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu.");
 49:                     return;
 50:                 }
 51: 
 52:                 // Bước 5: Kiểm tra nếu tài khoản đã bị Quản lý khóa (IsActive == false)
 53:                 if (!user.IsActive)
 54:                 {
 55:                     MessageBox.Show("Tài khoản đã bị khóa. Vui lòng liên hệ ban quản lý.");
 56:                     return;
 57:                 }
 58: 
 59:                 // Bước 6: Đăng nhập thành công -> Lưu thông tin phiên làm việc vào Session
 60:                 Session.SignIn(_db, user);
 61: 
 62:                 // Bước 7: Mở màn hình chính (MainWindow) và đóng màn hình Đăng nhập
 63:                 MainWindow main = new MainWindow();
 64:                 main.Show();
 65:                 this.Close();
 66:             }
 67:             catch (Exception ex)
 68:             {
 69:                 MessageBox.Show("Đăng nhập thất bại: " + ex.Message);
 70:             }
 71:         }
 72: 
 73:         // =====================================================================
 74:         // Sự kiện khi người dùng bấm nút "Đăng ký tài khoản" (btnRegister)
 75:         // =====================================================================
 76:         private void btnRegister_Click(object sender, RoutedEventArgs e)
 77:         {
 78:             // Mở cửa sổ Đăng ký mới (RegisterWindow)
 79:             RegisterWindow registerWindow = new RegisterWindow();
 80:             registerWindow.Show();
 81: 
 82:             // Đóng cửa sổ Đăng nhập hiện tại
 83:             this.Close();
 84:         }
 85:     }
 86: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 15**: `private SportsCenterContext _db;` – Khởi tạo DbContext.
- **Dòng 17-21 (Constructor)**: `InitializeComponent(); _db = new SportsCenterContext();` – Tinh gọn chuẩn PRN, không chứa focus thừa.
- **Dòng 26-70 (`btnLogin_Click` - Luật A1)**:
  - Dòng 29-30: Đọc `username` và `password`.
  - Dòng 33-37: Validate dữ liệu bắt buộc không rỗng.
  - Dòng 42: Tìm User trong DB: `_db.Users.FirstOrDefault(u => u.Username == username);`.
  - Dòng 45-50: So khớp mã băm: `user.PasswordHash != PasswordHelper.Hash(password)`.
  - Dòng 53-57: Kiểm tra cờ `!user.IsActive` $ightarrow$ Báo tài khoản bị khóa.
  - Dòng 60: Lưu phiên qua `Session.SignIn(_db, user)`.
  - Dòng 63-65: Mở `MainWindow` và đóng `LoginWindow`.
  - Dòng 67-70: Khối `catch` ngắn gọn chuẩn PRN: `MessageBox.Show("Đăng nhập thất bại: " + ex.Message);`.
- **Dòng 76-83 (`btnRegister_Click`)**: Mở `RegisterWindow` và đóng màn hình đăng nhập.

---

# 6. FILE: `RegisterWindow.xaml.cs` (Code-behind Tự Đăng ký & Gắn Hồ Sơ)

### Đường dẫn: `SportsCenterApp/SportsCenter/RegisterWindow.xaml.cs`

### Toàn bộ mã nguồn:
```csharp
  1: using SportsCenter.Entities;
  2: using System;
  3: using System.Linq;
  4: using System.Text.RegularExpressions;
  5: using System.Windows;
  6: 
  7: namespace SportsCenter
  8: {
  9:     /// <summary>
 10:     /// Interaction logic for RegisterWindow.xaml
 11:     /// Màn hình Đăng ký tài khoản hội viên mới (Member)
 12:     /// </summary>
 13:     public partial class RegisterWindow : Window
 14:     {
 15:         // 1. Khởi tạo đối tượng DbContext để làm việc với CSDL (chuẩn PRN212)
 16:         private SportsCenterContext _db;
 17: 
 18:         public RegisterWindow()
 19:         {
 20:             InitializeComponent();
 21:             _db = new SportsCenterContext(); // Khởi tạo kết nối DB
 22:         }
 23: 
 24:         // =====================================================================
 25:         // Hàm xóa sạch các dòng chữ báo lỗi màu đỏ trên form trước khi kiểm tra
 26:         // =====================================================================
 27:         private void ClearErrors()
 28:         {
 29:             errFullName.Text = string.Empty;
 30:             errPhone.Text = string.Empty;
 31:             errUsername.Text = string.Empty;
 32:             errPassword.Text = string.Empty;
 33:             errConfirm.Text = string.Empty;
 34:         }
 35: 
 36:         // =====================================================================
 37:         // Sự kiện khi người dùng nhấn nút "Tạo tài khoản" (btnCreate)
 38:         // =====================================================================
 39:         private void btnCreate_Click(object sender, RoutedEventArgs e)
 40:         {
 41:             // Bước 1: Xóa thông báo lỗi cũ và lấy dữ liệu người dùng nhập
 42:             ClearErrors();
 43:             string fullName = txtFullName.Text.Trim();
 44:             string phone = txtPhone.Text.Trim();
 45:             string username = txtUsername.Text.Trim();
 46:             string password = txtPassword.Password;
 47:             string confirm = txtConfirm.Password;
 48: 
 49:             // Biến cờ theo dõi xem toàn bộ dữ liệu có hợp lệ không
 50:             bool isValid = true;
 51:             var phoneCheck = (Result: AccountHelper.PhoneCheck.Free, Profile: (Member?)null);
 52: 
 53:             // Bước 2: Kiểm tra Họ và tên (FullName)
 54:             if (string.IsNullOrEmpty(fullName))
 55:             {
 56:                 errFullName.Text = "Vui lòng nhập họ tên.";
 57:                 isValid = false;
 58:             }
 59:             else
 60:             {
 61:                 string? nameError = AccountHelper.FullNameError(fullName);
 62:                 if (nameError != null)
 63:                 {
 64:                     errFullName.Text = nameError;
 65:                     isValid = false;
 66:                 }
 67:             }
 68: 
 69:             // Bước 3: Kiểm tra Số điện thoại (Phone)
 70:             if (string.IsNullOrEmpty(phone))
 71:             {
 72:                 errPhone.Text = "Vui lòng nhập số điện thoại.";
 73:                 isValid = false;
 74:             }
 75:             else if (!Regex.IsMatch(phone, @"^0\d{9,10}$"))
 76:             {
 77:                 errPhone.Text = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0.";
 78:                 isValid = false;
 79:             }
 80:             else
 81:             {
 82:                 // Quy tắc A5: Mỗi SĐT chỉ thuộc về một hội viên duy nhất
 83:                 phoneCheck = AccountHelper.CheckPhoneForRegistration(_db, fullName, phone);
 84:                 if (phoneCheck.Result == AccountHelper.PhoneCheck.AlreadyRegistered)
 85:                 {
 86:                     errPhone.Text = "Số điện thoại này đã được đăng ký. Hãy đăng nhập nếu là số của bạn.";
 87:                     isValid = false;
 88:                 }
 89:             }
 90: 
 91:             // Bước 4: Kiểm tra Tên đăng nhập (Username)
 92:             if (string.IsNullOrEmpty(username))
 93:             {
 94:                 errUsername.Text = "Vui lòng nhập tên đăng nhập.";
 95:                 isValid = false;
 96:             }
 97:             else
 98:             {
 99:                 string? usernameError = AccountHelper.UsernameError(username);
100:                 if (usernameError != null)
101:                 {
102:                     errUsername.Text = usernameError;
103:                     isValid = false;
104:                 }
105:                 else if (_db.Users.Any(u => u.Username == username))
106:                 {
107:                     // Kiểm tra trùng tên đăng nhập trong Database
108:                     errUsername.Text = "Tên đăng nhập này đã có người dùng.";
109:                     isValid = false;
110:                 }
111:             }
112: 
113:             // Bước 5: Kiểm tra Mật khẩu (Password)
114:             if (string.IsNullOrEmpty(password))
115:             {
116:                 errPassword.Text = "Vui lòng nhập mật khẩu.";
117:                 isValid = false;
118:             }
119:             else if (password.Length < 6)
120:             {
121:                 errPassword.Text = "Mật khẩu cần ít nhất 6 ký tự.";
122:                 isValid = false;
123:             }
124:             else if (password.Equals(username, StringComparison.OrdinalIgnoreCase))
125:             {
126:                 errPassword.Text = "Mật khẩu không được trùng tên đăng nhập.";
127:                 isValid = false;
128:             }
129: 
130:             // Bước 6: Kiểm tra Xác nhận mật khẩu (Confirm Password)
131:             if (string.IsNullOrEmpty(confirm))
132:             {
133:                 errConfirm.Text = "Vui lòng nhập lại mật khẩu.";
134:                 isValid = false;
135:             }
136:             else if (confirm != password)
137:             {
138:                 errConfirm.Text = "Hai mật khẩu chưa khớp.";
139:                 isValid = false;
140:             }
141: 
142:             // Nếu có bất kỳ trường dữ liệu nào sai thì dừng lại không xử lý tiếp
143:             if (!isValid)
144:             {
145:                 return;
146:             }
147: 
148:             // Bước 7: Thực hiện lưu tài khoản vào Database
149:             try
150:             {
151:                 Member? linkProfile = null;
152: 
153:                 // Quy tắc A2: Nếu SĐT trùng với hồ sơ tạo tại quầy (chưa có tài khoản) và khớp họ tên
154:                 if (phoneCheck.Result == AccountHelper.PhoneCheck.LinkCounterProfile)
155:                 {
156:                     Member counterMember = phoneCheck.Profile!;
157:                     string confirmPrompt = $"Đã có hồ sơ tại quầy của {counterMember.FullName} (SĐT: {counterMember.Phone}).\n\n"
158:                                          + "Bạn có muốn gắn tài khoản mới này vào hồ sơ cũ để giữ lại các gói tập và lịch sử không?";
159:                     
160:                     MessageBoxResult answer = MessageBox.Show(confirmPrompt, "Xác nhận liên kết", MessageBoxButton.YesNo, MessageBoxImage.Question);
161:                     if (answer != MessageBoxResult.Yes)
162:                     {
163:                         errPhone.Text = "Số điện thoại này đã có hồ sơ tại quầy. Hãy gắn tài khoản hoặc dùng số khác.";
164:                         return;
165:                     }
166:                     linkProfile = counterMember;
167:                 }
168: 
169:                 // Gọi helper tạo User vai trò Member (và liên kết hồ sơ quầy nếu có)
170:                 User createdUser = AccountHelper.CreateMemberAccount(_db, username, password, fullName, phone, linkProfile);
171: 
172:                 // Tự động đăng nhập người dùng vừa tạo
173:                 Session.SignIn(_db, createdUser);
174: 
175:                 // Ghi nhận Audit Log nếu có thao tác liên kết tài khoản
176:                 if (linkProfile != null)
177:                 {
178:                     AuditHelper.Log(_db, "LinkMember", $"Tài khoản {username} gắn vào hồ sơ quầy #{linkProfile.MemberId}", "Members", linkProfile.MemberId);
179:                 }
180: 
181:                 // Thông báo thành công và chuyển sang màn hình chính
182:                 string successNote = linkProfile != null ? " Tài khoản đã được gắn vào hồ sơ tại quầy của bạn." : "";
183:                 MessageBox.Show("Tạo tài khoản thành công!" + successNote);
184: 
185:                 MainWindow main = new MainWindow();
186:                 main.Show();
187:                 this.Close();
188:             }
189:             catch (Exception ex)
190:             {
191:                 MessageBox.Show("Tạo tài khoản thất bại: " + ex.Message);
192:             }
193:         }
194: 
195:         // =====================================================================
196:         // Sự kiện khi người dùng bấm nút "Đã có tài khoản? Đăng nhập" (btnBack)
197:         // =====================================================================
198:         private void btnBack_Click(object sender, RoutedEventArgs e)
199:         {
200:             LoginWindow loginWindow = new LoginWindow();
201:             loginWindow.Show();
202:             this.Close();
203:         }
204:     }
205: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 15**: `private SportsCenterContext _db;` – Khởi tạo DbContext.
- **Dòng 17-21 (Constructor)**: Tinh gọn 2 dòng khởi tạo giao diện và kết nối DB.
- **Dòng 26-33 (`ClearErrors`)**: Xóa trắng 5 dòng thông báo lỗi đỏ.
- **Dòng 38-190 (`btnCreate_Click` - Luật A2, A5)**:
  - Đọc và validate 5 trường dữ liệu: Họ tên, Số điện thoại (10-11 số), Tên đăng nhập (3-30 ký tự, không trùng DB), Mật khẩu ($\ge 6$ ký tự, không trùng username), Xác nhận mật khẩu khớp nhau.
  - Kiểm tra số điện thoại qua `AccountHelper.CheckPhoneForRegistration`.
  - Nếu có hồ sơ quầy đúng họ tên: Hiện hộp thoại hỏi gắn tài khoản vào hồ sơ cũ để giữ lại gói tập.
  - Gọi `AccountHelper.CreateMemberAccount` lưu DB, tự động đăng nhập `Session.SignIn`, ghi nhật ký `AuditHelper.Log`, thông báo thành công và chuyển sang `MainWindow`.
  - Khối `catch` rút gọn: `MessageBox.Show("Tạo tài khoản thất bại: " + ex.Message);`.
- **Dòng 196-201 (`btnBack_Click`)**: Quay lại màn hình đăng nhập.

---

# 7. FILE: `MainWindow.xaml.cs` (Khung Điều Hướng Ứng Dụng)

### Đường dẫn: `SportsCenterApp/SportsCenter/MainWindow.xaml.cs`

### Toàn bộ mã nguồn:
```csharp
  1: using System;
  2: using System.Collections.Generic;
  3: using System.Windows;
  4: using System.Windows.Controls;
  5: 
  6: namespace SportsCenter
  7: {
  8:     /// <summary>
  9:     /// Interaction logic for MainWindow.xaml
 10:     /// Khung giao diện chính của ứng dụng (Application Shell)
 11:     /// </summary>
 12:     public partial class MainWindow : Window
 13:     {
 14:         // 1. Biến static Instance để các màn hình con có thể gọi chuyển trang: MainWindow.Instance?.ShowPage(...)
 15:         public static MainWindow? Instance { get; private set; }
 16: 
 17:         // Lưu danh sách các nút menu tương ứng với từng loại trang
 18:         private readonly Dictionary<Type, Button> _buttons = new();
 19: 
 20:         // Lưu quan hệ giữa loại trang và nhóm menu chứa nó (Expander)
 21:         private readonly Dictionary<Type, Expander> _groupOf = new();
 22: 
 23:         // Danh sách các nhóm menu để xử lý đóng/mở (Accordion)
 24:         private readonly List<Expander> _groups = new();
 25: 
 26:         // Trang mặc định được mở đầu tiên khi đăng nhập
 27:         private Type? _firstPage;
 28: 
 29:         public MainWindow()
 30:         {
 31:             InitializeComponent();
 32:             Instance = this; // Gán thể hiện hiện tại vào static Instance
 33: 
 34:             // Bước 1: Lấy thông tin người dùng đang đăng nhập từ Session
 35:             var user = Session.CurrentUser!;
 36:             txtUserName.Text = user.FullName;
 37:             txtUserLogin.Text = user.Username;
 38: 
 39:             // Hiển thị tên vai trò tiếng Việt (Quản lý, Lễ tân, Hội viên, Huấn luyện viên)
 40:             txtRole.Text = Rules.RoleNames.ContainsKey(user.Role) ? Rules.RoleNames[user.Role] : user.Role;
 41: 
 42:             // Bước 2: Xây dựng menu tương ứng với vai trò của người dùng
 43:             BuildMenu(user.Role);
 44: 
 45:             // Bước 3: Mở trang đầu tiên mặc định lên màn hình
 46:             if (_firstPage != null)
 47:             {
 48:                 UserControl initialPage = (UserControl)Activator.CreateInstance(_firstPage)!;
 49:                 ShowPage(initialPage);
 50:             }
 51:         }
 52: 
 53:         // =====================================================================
 54:         // Xây dựng hệ thống menu động dựa theo vai trò (Role-based Navigation)
 55:         // =====================================================================
 56:         private void BuildMenu(string role)
 57:         {
 58:             switch (role)
 59:             {
 60:                 case "Manager": // Vai trò Quản lý: Toàn quyền truy cập mọi tính năng
 61:                     AddItem("Báo cáo", typeof(DashboardPage));
 62:                     AddGroup("Quản lý lớp", ("Lớp học", typeof(ClassesPage)), ("Bộ môn, phòng", typeof(SportsRoomsPage)));
 63:                     AddGroup("Thành viên và gói", ("Member", typeof(MembersPage)), ("Gói thành viên", typeof(PackagesPage)));
 64:                     AddItem("Thanh toán", typeof(PaymentsPage));
 65:                     AddGroup("Hệ thống", ("Tài khoản", typeof(UsersPage)), ("Lịch sử hoạt động", typeof(AuditPage)));
 66:                     break;
 67: 
 68:                 case "Receptionist": // Vai trò Lễ tân: Quản lý hội viên, bán gói, đặt lớp, thanh toán
 69:                     AddGroup("Thành viên", ("Member", typeof(MembersPage)), ("Mua / gia hạn gói", typeof(BuyPage)));
 70:                     AddItem("Đặt lớp", typeof(BookingPage));
 71:                     AddItem("Thanh toán", typeof(PaymentsPage));
 72:                     break;
 73: 
 74:                 case "Member": // Vai trò Hội viên: Xem lịch tập, đặt lớp, đăng ký gói, xem thanh toán
 75:                     AddItem("Lịch của tôi", typeof(MySchedulePage));
 76:                     AddItem("Đặt lớp", typeof(BookingPage));
 77:                     AddItem("Gói thành viên", typeof(BuyPage));
 78:                     AddItem("Thanh toán", typeof(PaymentsPage));
 79:                     break;
 80: 
 81:                 case "Coach": // Vai trò Huấn luyện viên: Xem lớp dạy và điểm danh học viên
 82:                     AddItem("Lớp của tôi", typeof(CoachPage));
 83:                     AddItem("Điểm danh", typeof(AttendancePage));
 84:                     break;
 85:             }
 86:         }
 87: 
 88:         // =====================================================================
 89:         // Hàm tạo một nút bấm đơn trong menu
 90:         // =====================================================================
 91:         private Button MakeButton(string title, Type pageType)
 92:         {
 93:             // Tạo nút với tiêu đề và áp dụng Style MenuButton
 94:             Button btn = new Button
 95:             {
 96:                 Content = title,
 97:                 Style = (Style)FindResource("MenuButton")
 98:             };
 99: 
100:             // Bắt sự kiện khi click nút -> Khởi tạo trang và hiển thị vào contentHost
101:             btn.Click += (s, e) =>
102:             {
103:                 UserControl pageInstance = (UserControl)Activator.CreateInstance(pageType)!;
104:                 ShowPage(pageInstance);
105:             };
106: 
107:             // Lưu vào từ điển tra cứu
108:             _buttons[pageType] = btn;
109: 
110:             // Đặt trang đầu tiên xuất hiện nếu chưa có
111:             if (_firstPage == null)
112:             {
113:                 _firstPage = pageType;
114:             }
115: 
116:             return btn;
117:         }
118: 
119:         // =====================================================================
120:         // Thêm một mục menu đơn lẻ (không có nhóm con)
121:         // =====================================================================
122:         private void AddItem(string title, Type pageType)
123:         {
124:             Button btn = MakeButton(title, pageType);
125:             panelMenu.Children.Add(btn);
126:         }
127: 
128:         // =====================================================================
129:         // Thêm một nhóm menu dạng xổ xuống (Expander)
130:         // =====================================================================
131:         private void AddGroup(string title, params (string Title, Type PageType)[] items)
132:         {
133:             StackPanel innerPanel = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
134:             Expander group = new Expander
135:             {
136:                 Header = title,
137:                 FontWeight = FontWeights.SemiBold,
138:                 Margin = new Thickness(0, 2, 0, 2),
139:                 Content = innerPanel
140:             };
141: 
142:             // Tạo từng nút bấm bên trong nhóm
143:             foreach (var item in items)
144:             {
145:                 Button btn = MakeButton(item.Title, item.PageType);
146:                 innerPanel.Children.Add(btn);
147:                 _groupOf[item.PageType] = group;
148:             }
149: 
150:             // Tự động thu gọn các nhóm khác khi mở nhóm này (Accordion effect)
151:             group.Expanded += (s, e) =>
152:             {
153:                 foreach (Expander g in _groups)
154:                 {
155:                     if (g != group)
156:                     {
157:                         g.IsExpanded = false;
158:                     }
159:                 }
160:             };
161: 
162:             _groups.Add(group);
163:             panelMenu.Children.Add(group);
164:         }
165: 
166:         // =====================================================================
167:         // Hiển thị một trang con vào vùng nội dung chính (contentHost)
168:         // và tô sáng nút menu đang được chọn
169:         // =====================================================================
170:         public void ShowPage(UserControl page)
171:         {
172:             // Bước 1: Nạp UserControl vào Frame/ContentControl chính
173:             contentHost.Content = page;
174:             Type pageType = page.GetType();
175: 
176:             // Bước 2: Đánh dấu trạng thái "on" cho nút menu đang được chọn
177:             foreach (var kv in _buttons)
178:             {
179:                 kv.Value.Tag = (kv.Key == pageType) ? "on" : null;
180:             }
181: 
182:             // Bước 3: Nếu trang nằm trong nhóm Expander, tự động mở rộng nhóm đó
183:             if (_groupOf.TryGetValue(pageType, out Expander? group) && group != null)
184:             {
185:                 group.IsExpanded = true;
186:             }
187:         }
188: 
189:         // =====================================================================
190:         // Sự kiện khi bấm nút "Đổi mật khẩu" ở góc dưới thanh bên
191:         // =====================================================================
192:         private void btnChangePassword_Click(object sender, RoutedEventArgs e)
193:         {
194:             // Mở cửa sổ Đổi mật khẩu dưới dạng Modal Dialog
195:             ChangePasswordWindow dlg = new ChangePasswordWindow();
196:             dlg.Owner = this;
197:             dlg.ShowDialog();
198:         }
199: 
200:         // =====================================================================
201:         // Sự kiện khi bấm nút "Đăng xuất" (btnLogout)
202:         // =====================================================================
203:         private void btnLogout_Click(object sender, RoutedEventArgs e)
204:         {
205:             // Bước 1: Xóa thông tin phiên làm việc trong Session
206:             Session.Clear();
207: 
208:             // Bước 2: Mở lại màn hình Đăng nhập
209:             LoginWindow loginWindow = new LoginWindow();
210:             loginWindow.Show();
211: 
212:             // Bước 3: Đóng màn hình chính
213:             this.Close();
214:         }
215:     }
216: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 14**: `public static MainWindow? Instance` – Thể hiện tĩnh để các trang con có thể gọi chuyển màn hình.
- **Dòng 28-48 (Constructor)**: Lưu `Instance = this`, hiển thị họ tên, username, vai trò tiếng Việt từ `Session.CurrentUser`, gọi `BuildMenu(user.Role)` và mở trang đầu tiên.
- **Dòng 53-83 (`BuildMenu`)**: Dựng menu động theo 4 vai trò (Manager: 8 mục; Receptionist: 4 mục; Member: 4 mục; Coach: 2 mục).
- **Dòng 88-111 (`MakeButton`)**: Tạo nút menu gán style `MenuButton` và bắt sự kiện Click chuyển trang.
- **Dòng 125-163 (`AddGroup`)**: Tạo nhóm xổ xuống bằng `Expander`, hỗ trợ hiệu ứng đóng nhóm khác khi mở nhóm này.
- **Dòng 170-188 (`ShowPage`)**: Đưa UserControl vào `contentHost.Content` và đánh dấu nút sáng (`Tag = "on"`).
- **Dòng 193-199 (`btnChangePassword_Click`)**: Mở hộp thoại đổi mật khẩu dạng Modal Dialog (`ShowDialog()`).
- **Dòng 204-215 (`btnLogout_Click`)**: Xóa phiên làm việc qua `Session.Clear()`, mở lại `LoginWindow` và đóng màn hình chính.

---

# 8. FILE: `ChangePasswordWindow.xaml.cs` (Code-behind Đổi Mật Khẩu)

### Đường dẫn: `SportsCenterApp/SportsCenter/ChangePasswordWindow.xaml.cs`

### Toàn bộ mã nguồn:
```csharp
  1: using SportsCenter.Entities;
  2: using System;
  3: using System.Windows;
  4: 
  5: namespace SportsCenter
  6: {
  7:     /// <summary>
  8:     /// Interaction logic for ChangePasswordWindow.xaml
  9:     /// Màn hình Đổi mật khẩu của người dùng đang đăng nhập
 10:     /// </summary>
 11:     public partial class ChangePasswordWindow : Window
 12:     {
 13:         // 1. Khởi tạo đối tượng DbContext để làm việc với CSDL (chuẩn PRN212)
 14:         private SportsCenterContext _db;
 15: 
 16:         public ChangePasswordWindow()
 17:         {
 18:             InitializeComponent();
 19:             _db = new SportsCenterContext(); // Khởi tạo kết nối DB
 20: 
 21:             // Lấy thông tin người dùng hiện tại từ Session để hiển thị lên thanh thông tin
 22:             User currentUser = Session.CurrentUser!;
 23:             string roleDisplayName = Rules.RoleNames.ContainsKey(currentUser.Role) ? Rules.RoleNames[currentUser.Role] : currentUser.Role;
 24:             txtInfo.Text = $"{currentUser.FullName} · {currentUser.Username} · {roleDisplayName}";
 25:         }
 26: 
 27:         // =====================================================================
 28:         // Sự kiện khi người dùng bấm nút "Lưu thay đổi" / "Đổi mật khẩu" (btnChange)
 29:         // =====================================================================
 30:         private void btnChange_Click(object sender, RoutedEventArgs e)
 31:         {
 32:             // Bước 1: Xóa thông báo lỗi cũ
 33:             errOld.Text = string.Empty;
 34:             errNew.Text = string.Empty;
 35:             errConfirm.Text = string.Empty;
 36: 
 37:             // Bước 2: Lấy dữ liệu các ô mật khẩu
 38:             string oldPassword = txtOld.Password;
 39:             string newPassword = txtNew.Password;
 40:             string confirmPassword = txtConfirm.Password;
 41: 
 42:             // Bước 3: Tìm tài khoản trong Database để cập nhật
 43:             User? user = _db.Users.Find(Session.CurrentUser!.UserId);
 44:             if (user == null)
 45:             {
 46:                 MessageBox.Show("Không tìm thấy thông tin tài khoản.");
 47:                 return;
 48:             }
 49: 
 50:             bool isValid = true;
 51: 
 52:             // Bước 4: Kiểm tra Mật khẩu hiện tại (Old Password)
 53:             if (string.IsNullOrEmpty(oldPassword))
 54:             {
 55:                 errOld.Text = "Vui lòng nhập mật khẩu hiện tại.";
 56:                 isValid = false;
 57:             }
 58:             else if (user.PasswordHash != PasswordHelper.Hash(oldPassword))
 59:             {
 60:                 errOld.Text = "Mật khẩu hiện tại chưa đúng.";
 61:                 isValid = false;
 62:             }
 63: 
 64:             // Bước 5: Kiểm tra Mật khẩu mới (New Password)
 65:             if (string.IsNullOrEmpty(newPassword))
 66:             {
 67:                 errNew.Text = "Vui lòng nhập mật khẩu mới.";
 68:                 isValid = false;
 69:             }
 70:             else if (newPassword.Length < 6)
 71:             {
 72:                 errNew.Text = "Mật khẩu cần ít nhất 6 ký tự.";
 73:                 isValid = false;
 74:             }
 75:             else if (newPassword == oldPassword)
 76:             {
 77:                 errNew.Text = "Mật khẩu mới phải khác mật khẩu hiện tại.";
 78:                 isValid = false;
 79:             }
 80:             else if (newPassword.Equals(user.Username, StringComparison.OrdinalIgnoreCase))
 81:             {
 82:                 errNew.Text = "Mật khẩu không được trùng tên đăng nhập.";
 83:                 isValid = false;
 84:             }
 85: 
 86:             // Bước 6: Kiểm tra Xác nhận mật khẩu mới (Confirm Password)
 87:             if (string.IsNullOrEmpty(confirmPassword))
 88:             {
 89:                 errConfirm.Text = "Vui lòng nhập lại mật khẩu mới.";
 90:                 isValid = false;
 91:             }
 92:             else if (confirmPassword != newPassword)
 93:             {
 94:                 errConfirm.Text = "Hai mật khẩu chưa khớp.";
 95:                 isValid = false;
 96:             }
 97: 
 98:             // Nếu có lỗi thì dừng lại
 99:             if (!isValid)
100:             {
101:                 return;
102:             }
103: 
104:             // Bước 7: Thực hiện cập nhật mật khẩu mới vào Database
105:             try
106:             {
107:                 // Băm mật khẩu mới bằng SHA-256 trước khi lưu
108:                 user.PasswordHash = PasswordHelper.Hash(newPassword);
109:                 _db.SaveChanges(); // Lưu vào SQL Server
110: 
111:                 // Ghi lại lịch sử hoạt động đổi mật khẩu
112:                 AuditHelper.Log(_db, "ChangePassword", $"{user.Username} đổi mật khẩu", "Users", user.UserId);
113: 
114:                 MessageBox.Show("Đã đổi mật khẩu thành công. Lần đăng nhập sau hãy dùng mật khẩu mới.");
115:                 this.Close(); // Đóng cửa sổ đổi mật khẩu
116:             }
117:             catch (Exception ex)
118:             {
119:                 MessageBox.Show("Đổi mật khẩu thất bại: " + ex.Message);
120:             }
121:         }
122:     }
123: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 14**: `private SportsCenterContext _db;` – Khởi tạo DbContext.
- **Dòng 16-25 (Constructor)**: Nạp thông tin người dùng từ `Session.CurrentUser` hiển thị lên thanh `txtInfo`. Đã loại bỏ hoàn toàn dòng lambda `Loaded += ...` thừa thãi.
- **Dòng 30-120 (`btnChange_Click`)**:
  - Xóa lỗi cũ, lấy 3 ô mật khẩu: hiện tại, mới, xác nhận.
  - Tìm bản ghi tài khoản trong DB qua `_db.Users.Find(...)`.
  - Validate mật khẩu hiện tại (so khớp `PasswordHelper.Hash`).
  - Validate mật khẩu mới ($\ge 6$ ký tự, khác mật khẩu cũ, không trùng username).
  - Validate xác nhận mật khẩu khớp mật khẩu mới.
  - Cập nhật mã băm mới `user.PasswordHash = PasswordHelper.Hash(newPassword)`, lưu DB `_db.SaveChanges()`, ghi nhật ký kiểm toán, thông báo thành công và đóng cửa sổ.
  - Khối `catch` ngắn gọn: `MessageBox.Show("Đổi mật khẩu thất bại: " + ex.Message);`.

---

# 9. FILE: `Pages/UsersPage.xaml.cs` (Code-behind Quản lý Tài khoản - Manager)

### Đường dẫn: `SportsCenterApp/SportsCenter/Pages/UsersPage.xaml.cs`

### Toàn bộ mã nguồn:
```csharp
  1: using SportsCenter.Entities;
  2: using System;
  3: using System.Linq;
  4: using System.Windows;
  5: using System.Windows.Controls;
  6: 
  7: namespace SportsCenter
  8: {
  9:     /// <summary>
 10:     /// Interaction logic for UsersPage.xaml
 11:     /// Màn hình Quản lý tài khoản người dùng (Dành cho Quản lý / Manager)
 12:     /// Cấu trúc chuẩn theo mẫu bài thi Q2 PRN212
 13:     /// </summary>
 14:     public partial class UsersPage : UserControl
 15:     {
 16:         private SportsCenterContext _db;
 17:         private readonly string[] _roles = { "Manager", "Receptionist", "Coach", "Member" };
 18: 
 19:         private static string RoleName(string code) => Rules.RoleNames.GetValueOrDefault(code, code);
 20: 
 21:         public UsersPage()
 22:         {
 23:             InitializeComponent();
 24:             _db = new SportsCenterContext();
 25:             GridHelper.Setup(dgUsers);
 26:             pgUsers.PageChanged += (s, e) => LoadUsers();
 27: 
 28:             LoadRole();
 29:             LoadUsers();
 30:         }
 31: 
 32:         public void LoadUsers()
 33:         {
 34:             string keyword = txtSearch.Text.Trim();
 35:             int roleIndex = cbxRoleFilter.SelectedIndex;
 36: 
 37:             // 1. Cú pháp LINQ truy vấn bảng Users:
 38:             var query = _db.Users.AsQueryable();
 39: 
 40:             // 2. Lọc theo từ khóa tìm kiếm (Username hoặc FullName) nếu có nhập:
 41:             if (!string.IsNullOrEmpty(keyword))
 42:             {
 43:                 query = query.Where(u => u.Username.Contains(keyword) || u.FullName.Contains(keyword));
 44:             }
 45: 
 46:             // 3. Lọc theo Role nếu có chọn (khác All / Tất cả):
 47:             if (roleIndex > 0)
 48:             {
 49:                 string selectedRole = _roles[roleIndex - 1];
 50:                 query = query.Where(u => u.Role == selectedRole);
 51:             }
 52: 
 53:             // 4. Đổ dữ liệu vào DataGrid:
 54:             var userList = query
 55:                 .OrderBy(u => u.UserId)
 56:                 .Select(u => new
 57:                 {
 58:                     u.UserId,
 59:                     u.Username,
 60:                     u.FullName,
 61:                     Role = RoleName(u.Role),
 62:                     Status = u.IsActive ? "Đang hoạt động" : "Đã khóa"
 63:                 })
 64:                 .ToList();
 65: 
 66:             dgUsers.ItemsSource = pgUsers.Slice(userList);
 67:         }
 68: 
 69:         public void LoadRole()
 70:         {
 71:             // Xóa item cũ trước khi nạp
 72:             cbxRoleFilter.Items.Clear();
 73:             cbxRole.Items.Clear();
 74:             cbxNewRole.Items.Clear();
 75: 
 76:             // 1. Nạp ComboBox Filter: thêm "All" ở đầu tiên
 77:             cbxRoleFilter.Items.Add("All");
 78: 
 79:             // 2. Dùng foreach duyệt qua từng vai trò để nạp vào các ComboBox
 80:             foreach (var role in _roles)
 81:             {
 82:                 string name = RoleName(role);
 83:                 cbxRoleFilter.Items.Add(name); // Thêm vào bộ lọc
 84:                 cbxRole.Items.Add(name);       // Thêm vào ô Thêm mới
 85:                 cbxNewRole.Items.Add(name);    // Thêm vào ô Đổi vai trò
 86:             }
 87: 
 88:             // Chọn vị trí mặc định
 89:             cbxRoleFilter.SelectedIndex = 0; // Chọn "All"
 90:             cbxRole.SelectedIndex = 1;       // Chọn "Lễ tân"
 91:             cbxNewRole.SelectedIndex = 1;    // Chọn "Lễ tân"
 92:         }
 93: 
 94:         private void btnFilter_Click(object sender, RoutedEventArgs e)
 95:         {
 96:             pgUsers.Reset();
 97:             LoadUsers();
 98:         }
 99: 
100:         private void btnClearFilter_Click(object sender, RoutedEventArgs e)
101:         {
102:             // Reset cb về "All"
103:             txtSearch.Clear();
104:             cbxRoleFilter.SelectedIndex = 0;
105:             pgUsers.Reset();
106:             LoadUsers();
107:         }
108: 
109:         private void btnAddUser_Click(object sender, RoutedEventArgs e)
110:         {
111:             string fullName = txtFullName.Text.Trim();
112:             string username = txtUsername.Text.Trim();
113: 
114:             if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username))
115:             {
116:                 MessageBox.Show("Vui lòng nhập Họ tên và Tên đăng nhập.");
117:                 return;
118:             }
119: 
120:             if (AccountHelper.FullNameError(fullName) is string nameError)
121:             {
122:                 MessageBox.Show(nameError);
123:                 return;
124:             }
125: 
126:             if (AccountHelper.UsernameError(username) is string usernameError)
127:             {
128:                 MessageBox.Show(usernameError);
129:                 return;
130:             }
131: 
132:             if (_db.Users.Any(u => u.Username == username))
133:             {
134:                 MessageBox.Show("Tên đăng nhập đã tồn tại.");
135:                 return;
136:             }
137: 
138:             try
139:             {
140:                 var user = new User
141:                 {
142:                     Username = username,
143:                     PasswordHash = PasswordHelper.Hash("123456"),
144:                     FullName = fullName,
145:                     Role = _roles[cbxRole.SelectedIndex],
146:                     IsActive = true,
147:                     CreatedAt = DateTime.Now
148:                 };
149: 
150:                 _db.Users.Add(user);
151:                 EnsureProfile(user);
152:                 _db.SaveChanges();
153: 
154:                 AuditHelper.Log(_db, "CreateUser", $"Tạo tài khoản {username}", "Users", user.UserId);
155: 
156:                 // reset lại dg và form
157:                 LoadUsers();
158:                 ClearForm();
159: 
160:                 MessageBox.Show("Thêm tài khoản thành công. Mật khẩu mặc định là 123456.");
161:             }
162:             catch (Exception ex)
163:             {
164:                 MessageBox.Show("Thêm tài khoản thất bại: " + ex.Message);
165:             }
166:         }
167: 
168:         private void btnToggle_Click(object sender, RoutedEventArgs e)
169:         {
170:             var user = GetSelectedUser();
171:             if (user == null) return;
172: 
173:             try
174:             {
175:                 user.IsActive = !user.IsActive;
176:                 _db.SaveChanges();
177: 
178:                 AuditHelper.Log(_db, "ToggleUser", $"{(user.IsActive ? "Mở khóa" : "Khóa")} tài khoản {user.Username}", "Users", user.UserId);
179: 
180:                 // reset lại dg
181:                 LoadUsers();
182:                 GridHelper.SelectById(dgUsers, "UserId", user.UserId);
183: 
184:                 MessageBox.Show("Cập nhật trạng thái thành công.");
185:             }
186:             catch (Exception ex)
187:             {
188:                 MessageBox.Show("Cập nhật trạng thái thất bại: " + ex.Message);
189:             }
190:         }
191: 
192:         private void btnChangeRole_Click(object sender, RoutedEventArgs e)
193:         {
194:             var user = GetSelectedUser();
195:             if (user == null) return;
196: 
197:             string newRole = _roles[cbxNewRole.SelectedIndex];
198:             if (newRole == user.Role)
199:             {
200:                 MessageBox.Show("Tài khoản này đã có vai trò " + RoleName(newRole) + ".");
201:                 return;
202:             }
203: 
204:             try
205:             {
206:                 user.Role = newRole;
207:                 EnsureProfile(user);
208:                 _db.SaveChanges();
209: 
210:                 AuditHelper.Log(_db, "SetRole", $"Đổi vai trò {user.Username} thành {RoleName(newRole)}", "Users", user.UserId);
211: 
212:                 // reset lại dg
213:                 LoadUsers();
214:                 GridHelper.SelectById(dgUsers, "UserId", user.UserId);
215: 
216:                 MessageBox.Show("Đổi vai trò thành công.");
217:             }
218:             catch (Exception ex)
219:             {
220:                 MessageBox.Show("Đổi vai trò thất bại: " + ex.Message);
221:             }
222:         }
223: 
224:         private User? GetSelectedUser()
225:         {
226:             int? id = GridHelper.SelectedId(dgUsers, "UserId");
227:             if (id == null)
228:             {
229:                 MessageBox.Show("Vui lòng chọn một tài khoản trong danh sách.");
230:                 return null;
231:             }
232: 
233:             // Luật A3: Không thể tự khóa hoặc đổi quyền chính mình
234:             if (id == Session.CurrentUser!.UserId)
235:             {
236:                 MessageBox.Show("Không thể khóa hoặc đổi vai trò của chính tài khoản đang đăng nhập.");
237:                 return null;
238:             }
239: 
240:             return _db.Users.Find(id);
241:         }
242: 
243:         private void EnsureProfile(User user)
244:         {
245:             if (user.Role == "Coach" && !_db.Coaches.Any(c => c.UserId == user.UserId))
246:             {
247:                 _db.Coaches.Add(new Coach { User = user, FullName = user.FullName, Email = user.Username + "@center.vn", Phone = user.Phone });
248:             }
249:             if (user.Role == "Member" && !_db.Members.Any(m => m.UserId == user.UserId))
250:             {
251:                 _db.Members.Add(new Member { User = user, FullName = user.FullName, Phone = user.Phone, CreatedAt = DateTime.Now });
252:             }
253:         }
254: 
255:         // Tách hàm ClearForm() riêng biệt để tái sử dụng
256:         private void ClearForm()
257:         {
258:             txtFullName.Clear();
259:             txtUsername.Clear();
260:             cbxRole.SelectedIndex = 1;
261:         }
262:     }
263: }
```

### Giải thích chi tiết từng dòng (Chuẩn câu 2 đề thi PRN212):
- **Dòng 15**: `private SportsCenterContext _db;` – Khai báo DbContext kết nối CSDL.
- **Dòng 16**: Mảng `_roles = { "Manager", "Receptionist", "Coach", "Member" }`.
- **Dòng 18**: Hàm `RoleName(code)` chuyển mã tiếng Anh sang tên tiếng Việt hiển thị.
- **Dòng 20-29 (Constructor)**: Khởi tạo DB, thiết lập DataGrid qua `GridHelper.Setup`, đăng ký phân trang `pgUsers.PageChanged += (s, e) => LoadUsers();`, gọi `LoadRole()` và `LoadUsers()`.
- **Dòng 31-70 (`LoadUsers`)**:
  - Dòng 37 (Bước 1): Khởi tạo truy vấn LINQ: `var query = _db.Users.AsQueryable();`.
  - Dòng 40-43 (Bước 2): Lọc theo từ khóa tìm kiếm: `query = query.Where(u => u.Username.Contains(keyword) || u.FullName.Contains(keyword));`.
  - Dòng 46-50 (Bước 3): Lọc theo vai trò nếu ComboBox khác "All": `query = query.Where(u => u.Role == selectedRole);`.
  - Dòng 53-65 (Bước 4): Chiếu dữ liệu `.Select(...)` nạp các thuộc tính hiển thị (Mã, Username, Họ tên, Role tiếng Việt, Status) vào danh sách `userList` và gán `dgUsers.ItemsSource = pgUsers.Slice(userList)`.
- **Dòng 72-96 (`LoadRole` - Chuẩn PRN bằng vòng lặp foreach)**:
  - Xóa item cũ: `cbxRoleFilter.Items.Clear()`, `cbxRole.Items.Clear()`, `cbxNewRole.Items.Clear()`.
  - Thêm "All" ở đầu bộ lọc: `cbxRoleFilter.Items.Add("All");`.
  - Dùng 1 vòng lặp `foreach (var role in _roles)` nạp tên vai trò vào cả 3 ComboBox.
  - Chọn vị trí mặc định: `cbxRoleFilter.SelectedIndex = 0; cbxRole.SelectedIndex = 1; cbxNewRole.SelectedIndex = 1;` (đã chuẩn hóa thành 1 đồng bộ).
- **Dòng 98-102 (`btnFilter_Click`)**: Reset trang và gọi `LoadUsers()`.
- **Dòng 104-111 (`btnClearFilter_Click`)**: Xóa trắng ô tìm kiếm, đưa ComboBox lọc về "All" (`SelectedIndex = 0`), reset trang và tải lại toàn bộ danh sách.
- **Dòng 113-173 (`btnAddUser_Click`)**: Thêm tài khoản mới: Validate dữ liệu bắt buộc không rỗng, kiểm tra định dạng qua `AccountHelper`, kiểm tra trùng Username, tạo đối tượng `User` mới với mật khẩu mặc định "123456" (băm SHA-256), thêm vào DB, lưu DB, ghi log kiểm toán, reset DataGrid bằng `LoadUsers()`, reset form bằng hàm riêng `ClearForm()`, hiển thị thông báo thành công.
- **Dòng 175-198 (`btnToggle_Click`)**: Đảo ngược trạng thái `user.IsActive = !user.IsActive`, lưu DB, reset DataGrid và thông báo thành công.
- **Dòng 200-234 (`btnChangeRole_Click`)**: Cập nhật vai trò mới cho tài khoản, gọi `EnsureProfile`, lưu DB, reset DataGrid và thông báo thành công.
- **Dòng 236-252 (`GetSelectedUser`)**: Lấy tài khoản đang chọn từ DataGrid, áp dụng **Luật A3**: Không cho phép Quản lý tự khóa hoặc đổi quyền chính mình (`id == Session.CurrentUser!.UserId`).
- **Dòng 254-267 (`EnsureProfile`)**: Tự động thêm bản ghi vào `Coaches` hoặc `Members` nếu chưa có.
- **Dòng 269-275 (`ClearForm`)**: Hàm riêng biệt xóa trắng các ô nhập liệu và reset ComboBox về mặc định để tái sử dụng.
