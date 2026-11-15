# GIẢI THÍCH TOÀN BỘ CODE TỪNG DÒNG – PHẦN NGƯỜI 1: TÀI KHOẢN VÀ KHUNG ỨNG DỤNG
## DỰ ÁN SPORTS CENTER (PRN212 - WPF & EF CORE 8)
### *Phiên bản đã chuẩn hóa theo đề thi PRN212 với chú thích chi tiết từng dòng code*

> **Phạm vi Người 1 phụ trách gồm 9 file C#**:
> 1. `Entities/User.cs` – Mô hình dữ liệu tài khoản
> 2. `Helpers/PasswordHelper.cs` – Tiện ích băm mật khẩu SHA-256 (tách 4 bước tuần tự)
> 3. `Helpers/Session.cs` – Quản lý phiên làm việc toàn cục
> 4. `Helpers/AccountHelper.cs` – Chuẩn hóa chuỗi, kiểm tra định dạng & liên kết hồ sơ quầy
> 5. `LoginWindow.xaml.cs` – Màn hình Đăng nhập (Login)
> 6. `RegisterWindow.xaml.cs` – Màn hình Đăng ký tài khoản hội viên (Register)
> 7. `MainWindow.xaml.cs` – Khung giao diện chính & điều hướng theo Role (Shell)
> 8. `ChangePasswordWindow.xaml.cs` – Hộp thoại Đổi mật khẩu
> 9. `Pages/UsersPage.xaml.cs` – Quản lý người dùng cho Quản lý (chuẩn cấu trúc Q2 PRN212)

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
- **Dòng 1**: `namespace SportsCenter.Entities;` – Khai báo không gian tên chứa các lớp thực thể (Entity Models) đại diện cho các bảng trong CSDL SQL Server.
- **Dòng 3-7**: Khối ghi chú mô tả vai trò của thực thể `User`, tương ứng với bảng `Users` và do Người 1 phụ trách.
- **Dòng 8**: `public partial class User` – Định nghĩa lớp thực thể `User`. Từ khóa `partial` cho phép mở rộng lớp nếu cần.
- **Dòng 10-11**: `public int UserId { get; set; }` – Khóa chính (`Primary Key`), tự tăng (`IDENTITY(1,1)`).
- **Dòng 13-14**: `public string Username { get; set; } = null!;` – Tên đăng nhập duy nhất (`UNIQUE`), độ dài 3-30 ký tự. Toán tử `= null!;` báo cho compiler không báo cảnh báo null vì EF Core sẽ gán giá trị khi nạp từ DB.
- **Dòng 16-17**: `public string PasswordHash { get; set; } = null!;` – Chuỗi mã băm SHA-256 (64 ký tự hex in hoa), lưu trữ mật khẩu an toàn thay vì lưu mật khẩu thô.
- **Dòng 19-20**: `public string FullName { get; set; } = null!;` – Họ và tên đầy đủ của người dùng (2-100 ký tự).
- **Dòng 22-23**: `public string? Email { get; set; }` – Địa chỉ email, kiểu nullable (`NVARCHAR(150) NULL`) do hệ thống không bắt buộc nhập email.
- **Dòng 25-26**: `public string? Phone { get; set; }` – Số điện thoại liên lạc (10-11 chữ số, duy nhất trong hệ thống).
- **Dòng 28-29**: `public string Role { get; set; } = null!;` – Vai trò trong hệ thống, nhận 1 trong 4 giá trị theo ràng buộc CHECK constraint: `Manager` (Quản lý), `Receptionist` (Lễ tân), `Coach` (Huấn luyện viên), `Member` (Hội viên).
- **Dòng 31-32**: `public bool IsActive { get; set; }` – Cờ trạng thái: `true` là đang hoạt động, `false` là bị khóa (chặn đăng nhập).
- **Dòng 34-35**: `public DateTime CreatedAt { get; set; }` – Ngày giờ tạo tài khoản, mặc định `GETDATE()` từ SQL.
- **Dòng 37-38**: `public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();` – Thuộc tính điều hướng (Navigation Property) 1-N: Danh sách các nhật ký hoạt động do tài khoản này thực hiện.
- **Dòng 40-41**: `public virtual ICollection<Coach> Coaches { get; set; } = new List<Coach>();` – Quan hệ 1-1 liên kết sang bảng `Coaches` (nếu vai trò là Huấn luyện viên).
- **Dòng 43-44**: `public virtual ICollection<Member> Members { get; set; } = new List<Member>();` – Quan hệ 1-1 liên kết sang bảng `Members` (nếu vai trò là Hội viên).

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

### Giải thích chi tiết từng dòng:
- **Dòng 1-2**: `using System.Security.Cryptography; using System.Text;` – Import thư viện mật mã học và mã hóa văn bản của .NET.
- **Dòng 4**: `namespace SportsCenter` – Không gian tên ứng dụng.
- **Dòng 6-10**: Chú thích mô tả lớp tiện ích tĩnh băm mật khẩu theo chuẩn đề thi PRN212.
- **Dòng 11**: `public static class PasswordHelper` – Lớp tĩnh, không cần khởi tạo đối tượng `new PasswordHelper()`.
- **Dòng 13-15**: `public static string Hash(string password)` – Hàm băm nhận chuỗi mật khẩu gốc (plain text) và trả về chuỗi Hex in hoa.
- **Dòng 17-21**: **Bước 1** – Kiểm tra nếu chuỗi đầu vào rỗng hoặc null (`string.IsNullOrEmpty`) thì trả về `string.Empty` để tránh lỗi ngoại lệ.
- **Dòng 23-24**: **Bước 2** – `byte[] inputBytes = Encoding.UTF8.GetBytes(password);` chuyển chuỗi ký tự thành mảng byte chuẩn UTF-8.
- **Dòng 26-27**: **Bước 3** – `byte[] hashBytes = SHA256.HashData(inputBytes);` thực hiện giải thuật băm một chiều SHA-256 an toàn, sinh ra mảng băm cố định 32 byte.
- **Dòng 29-30**: **Bước 4** – `string hexString = Convert.ToHexString(hashBytes);` chuyển mảng 32 byte thành chuỗi 64 ký tự Hexadecimal in hoa (ví dụ: `BA3253876AED6...`).
- **Dòng 32-33**: **Bước 5** – Trả về chuỗi hex kết quả. Cách băm này hoàn toàn tương thích với hàm `HASHBYTES('SHA2_256', '123456')` trong SQL Server.

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
- **Dòng 1-2**: Import EF Core và không gian tên Entities.
- **Dòng 4-11**: Khai báo lớp static `Session` đóng vai trò lưu vết phiên làm việc toàn cục của người dùng đang đăng nhập trong suốt vòng đời ứng dụng.
- **Dòng 13-14**: `public static User? CurrentUser { get; set; }` – Lưu thông tin bản ghi `User` hiện tại đang đăng nhập. Dấu `?` cho biết có thể là `null` khi chưa ai đăng nhập.
- **Dòng 16-17**: `public static Member? CurrentMember { get; set; }` – Lưu thông tin hồ sơ hội viên nếu tài khoản đăng nhập có Role là `"Member"`.
- **Dòng 19-20**: `public static Coach? CurrentCoach { get; set; }` – Lưu thông tin hồ sơ huấn luyện viên nếu tài khoản đăng nhập có Role là `"Coach"`.
- **Dòng 22-34**: Thuộc tính `Role` với getter tường minh:
  - Nếu `CurrentUser != null` thì trả về `CurrentUser.Role` (ví dụ: `"Manager"`, `"Member"`...).
  - Ngược lại trả về chuỗi rỗng `string.Empty`.
- **Dòng 39**: `public static void SignIn(SportsCenterContext db, User user)` – Hàm đăng nhập thiết lập phiên làm việc:
  - **Dòng 42**: Bước 1 – Gán `CurrentUser = user`.
  - **Dòng 44-52**: Bước 2 – Nếu `user.Role == "Member"`, truy vấn bảng `Members` không theo dõi trạng thái (`AsNoTracking()`) tìm bản ghi có `UserId == user.UserId` gán vào `CurrentMember`. Ngược lại gán `null`.
  - **Dòng 54-62**: Bước 3 – Nếu `user.Role == "Coach"`, truy vấn bảng `Coaches` với `AsNoTracking()` tìm bản ghi có `UserId == user.UserId` gán vào `CurrentCoach`. Ngược lại gán `null`.
  - **Dòng 64-65**: Bước 4 – Tự động gọi `AuditHelper.Log(db, "Login", "Đăng nhập hệ thống")` để lưu vết lịch sử đăng nhập vào bảng `AuditLogs`.
- **Dòng 71-77**: `public static void Clear()` – Hàm đăng xuất dọn sạch phiên làm việc: gán cả 3 biến static về `null`.

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
- **Dòng 1-5**: Import các thư viện xử lý EF Core, Unicode, chuỗi và Regex.
- **Dòng 7-18**: Khai báo lớp `AccountHelper` xử lý toàn bộ nghiệp vụ tài khoản: chuẩn hóa họ tên, xác thực email/tên đăng nhập/họ tên, kiểm tra số điện thoại và thuật toán gắn tài khoản vào hồ sơ quầy.
- **Dòng 24-52**: Phương thức `Normalize(string text)` – Chuẩn hóa chuỗi họ tên:
  - Dòng 26-29: Kiểm tra chuỗi rỗng bằng `string.IsNullOrWhiteSpace(text)`.
  - Dòng 32: Cắt khoảng trắng đầu cuối (`Trim()`), chuyển chữ thường (`ToLowerInvariant()`), thay thế chữ `đ` thành `d`.
  - Dòng 35: Phân tách dấu tiếng Việt bằng `Normalize(NormalizationForm.FormD)`.
  - Dòng 38-46: Lặp qua từng ký tự, lọc bỏ các dấu thanh (`UnicodeCategory.NonSpacingMark`), chỉ giữ lại ký tự chữ cái thuần túy.
  - Dòng 49-51: Tách từ bằng khoảng trắng (`Split(' ', StringSplitOptions.RemoveEmptyEntries)`) và gộp lại bằng 1 khoảng trắng đơn `string.Join(" ", words)`. Kết quả giúp so sánh họ tên không phân biệt hoa/thường, không phân biệt dấu và khoảng trắng thừa (ví dụ: `"  Nguyễn   Văn Đạt "` $ightarrow$ `"nguyen van dat"`).
- **Dòng 58-73**: Phương thức `FindByPhone(SportsCenterContext db, string phone)`:
  - Dòng 60-63: Kiểm tra SĐT rỗng.
  - Dòng 66-70: Truy vấn LINQ bảng `Members` với `AsNoTracking()`, sắp xếp theo `MemberId` và tìm bản ghi đầu tiên khớp số điện thoại (`FirstOrDefault(m => m.Phone == phone)`).
- **Dòng 78-83**: Định nghĩa `enum PhoneCheck` đại diện cho 3 trạng thái kiểm tra số điện thoại:
  - `Free`: Số mới hoàn toàn, chưa ai dùng.
  - `LinkCounterProfile`: Số thuộc về hồ sơ tạo tại quầy (chưa có tài khoản) và khớp họ tên.
  - `AlreadyRegistered`: Số đã được tài khoản khác sử dụng hoặc khác họ tên.
- **Dòng 88-111**: Phương thức `CheckPhoneForRegistration`:
  - Dòng 91-97: Tìm theo số điện thoại, nếu chưa có ai dùng (`existingMember == null`) thì trả về `(PhoneCheck.Free, null)`.
  - Dòng 100-106: Nếu số đã có, tìm hồ sơ quầy qua `FindCounterProfiles`. Nếu tìm thấy hồ sơ khớp họ tên thì trả về `(PhoneCheck.LinkCounterProfile, counterProfiles[0])`.
  - Dòng 109-110: Nếu không khớp họ tên hoặc đã có tài khoản thì trả về `(PhoneCheck.AlreadyRegistered, null)`.
- **Dòng 116-126**: `IsValidEmail(string email)` – Kiểm tra định dạng email bằng Regex `^[^@\s]+@[^@\s]+\.[^@\s]+$`.
- **Dòng 132-148**: `UsernameError(string username)`:
  - Dòng 135-138: Kiểm tra độ dài 3-30 ký tự.
  - Dòng 141-145: Kiểm tra chỉ gồm chữ cái không dấu, số, dấu chấm hoặc gạch dưới qua Regex `^[A-Za-z0-9._]+$`.
- **Dòng 154-171**: `FullNameError(string fullName)`:
  - Dòng 157: Chuẩn hóa FormC.
  - Dòng 160-163: Kiểm tra độ dài 2-100 ký tự.
  - Dòng 166-170: Kiểm tra chỉ gồm chữ cái tiếng Việt và khoảng trắng, không chứa số qua Regex `^\p{L}[\p{L}\p{M} .'’-]*$`.
- **Dòng 176-192**: `FindCounterProfiles(SportsCenterContext db, string fullName, string phone)`:
  - Dòng 179: Chuẩn hóa họ tên đầu vào `Normalize(fullName)`.
  - Dòng 182-185: Lọc các Member có `UserId == null` (tạo tại quầy) và trùng SĐT.
  - Dòng 188-191: Lọc tiếp trên bộ nhớ các hồ sơ có họ tên chuẩn hóa khớp hoàn toàn.
- **Dòng 199-250**: `CreateMemberAccount(...)` – Tạo tài khoản hội viên:
  - Dòng 205-208: Ném ngoại lệ nếu hồ sơ quầy đã có tài khoản rồi.
  - Dòng 211-220: Tạo đối tượng `User` mới với role `"Member"`, băm mật khẩu `PasswordHelper.Hash(password)`.
  - Dòng 223-234: Trường hợp 1 (`counterProfile == null`): Tạo thêm bản ghi `Member` mới gắn với `User`.
  - Dòng 235-244: Trường hợp 2 (`counterProfile != null`): Attach hồ sơ quầy vào `db` nếu detached, gán `counterProfile.User = newUser` để liên kết.
  - Dòng 247: Lưu toàn bộ thay đổi vào CSDL chỉ trong 1 lần gọi `db.SaveChanges()`.

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
 14:         // 1. Khởi tạo DbContext kết nối Database (tương tự như trong bài thi PRN)
 15:         private SportsCenterContext _db;
 16: 
 17:         public LoginWindow()
 18:         {
 19:             InitializeComponent();
 20:             _db = new SportsCenterContext(); // Khởi tạo kết nối DB
 21:             txtUsername.Focus();             // Đặt con trỏ chuột vào ô Tên đăng nhập khi vừa mở
 22:         }
 23: 
 24:         // =====================================================================
 25:         // Sự kiện khi người dùng bấm nút "Đăng nhập" (btnLogin)
 26:         // =====================================================================
 27:         private void btnLogin_Click(object sender, RoutedEventArgs e)
 28:         {
 29:             // Bước 1: Lấy chuỗi Tên đăng nhập và Mật khẩu từ giao diện
 30:             string username = txtUsername.Text.Trim();
 31:             string password = txtPassword.Password;
 32: 
 33:             // Bước 2: Kiểm tra dữ liệu rỗng (Validation)
 34:             if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
 35:             {
 36:                 MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
 37:                 return;
 38:             }
 39: 
 40:             try
 41:             {
 42:                 // Bước 3: Tìm tài khoản trong Database khớp với Username vừa nhập
 43:                 User? user = _db.Users.FirstOrDefault(u => u.Username == username);
 44: 
 45:                 // Bước 4: Kiểm tra nếu tài khoản không tồn tại HOẶC mật khẩu băm không trùng khớp
 46:                 string hashedPassword = PasswordHelper.Hash(password);
 47:                 if (user == null || user.PasswordHash != hashedPassword)
 48:                 {
 49:                     MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
 50:                     return;
 51:                 }
 52: 
 53:                 // Bước 5: Kiểm tra nếu tài khoản đã bị Quản lý khóa (IsActive == false)
 54:                 if (!user.IsActive)
 55:                 {
 56:                     MessageBox.Show("Tài khoản đã bị khóa. Vui lòng liên hệ ban quản lý.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
 57:                     return;
 58:                 }
 59: 
 60:                 // Bước 6: Đăng nhập thành công -> Lưu thông tin phiên làm việc vào Session
 61:                 Session.SignIn(_db, user);
 62: 
 63:                 // Bước 7: Mở màn hình chính (MainWindow) và đóng màn hình Đăng nhập
 64:                 MainWindow main = new MainWindow();
 65:                 main.Show();
 66:                 this.Close();
 67:             }
 68:             catch (Exception ex)
 69:             {
 70:                 // Xử lý khi không kết nối được cơ sở dữ liệu
 71:                 string errorMsg = "Không kết nối được cơ sở dữ liệu. Hãy kiểm tra file appsettings.json và cơ sở dữ liệu SQL Server.\n\n"
 72:                                   + (ex.InnerException?.Message ?? ex.Message);
 73:                 MessageBox.Show(errorMsg, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
 74:             }
 75:         }
 76: 
 77:         // =====================================================================
 78:         // Sự kiện khi người dùng bấm nút "Đăng ký tài khoản" (btnRegister)
 79:         // =====================================================================
 80:         private void btnRegister_Click(object sender, RoutedEventArgs e)
 81:         {
 82:             // Mở cửa sổ Đăng ký mới (RegisterWindow)
 83:             RegisterWindow registerWindow = new RegisterWindow();
 84:             registerWindow.Show();
 85: 
 86:             // Đóng cửa sổ Đăng nhập hiện tại
 87:             this.Close();
 88:         }
 89:     }
 90: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 1-4**: Import các thư viện WPF và Entities.
- **Dòng 9**: `public partial class LoginWindow : Window` – Lớp tương tác của cửa sổ đăng nhập.
- **Dòng 14**: `private SportsCenterContext _db;` – Khởi tạo đối tượng DbContext (tương tự như trong bài thi PRN212).
- **Dòng 16-21**: Hàm tạo `LoginWindow()`:
  - `InitializeComponent()` nạp các thành phần giao diện XAML.
  - `_db = new SportsCenterContext();` khởi tạo kết nối CSDL.
  - `txtUsername.Focus();` đặt con trỏ chuột vào ô nhập tên đăng nhập.
- **Dòng 26-66**: Sự kiện `btnLogin_Click` khi người dùng bấm nút "Đăng nhập":
  - Dòng 29-30: Lấy `username` (cắt khoảng trắng) và `password` từ giao diện.
  - Dòng 33-37: Validate dữ liệu bắt buộc nhập, nếu rỗng thì hiện `MessageBox.Show` cảnh báo và `return`.
  - Dòng 39-65: Khối `try-catch`:
    - Dòng 42: Tìm tài khoản trong DB bằng LINQ `_db.Users.FirstOrDefault(u => u.Username == username)`.
    - Dòng 45-50: So khớp mật khẩu băm `PasswordHelper.Hash(password)`. Nếu không thấy tài khoản hoặc mật khẩu không khớp thì hiện thông báo "Sai tên đăng nhập hoặc mật khẩu."
    - Dòng 53-57: Kiểm tra trạng thái `IsActive`. Nếu tài khoản bị khóa (`!user.IsActive`) thì báo "Tài khoản đã bị khóa" và dừng lại.
    - Dòng 60: Đăng nhập thành công -> gọi `Session.SignIn(_db, user)` để thiết lập phiên làm việc.
    - Dòng 63-65: Mở cửa sổ chính `new MainWindow().Show()` và đóng cửa sổ đăng nhập `this.Close()`.
  - Dòng 67-73: Khối `catch (Exception ex)` bắt lỗi kết nối CSDL và hiển thị thông báo chi tiết cho người dùng.
- **Dòng 79-86**: Sự kiện `btnRegister_Click` khi bấm nút "Đăng ký": Mở `new RegisterWindow().Show()` và đóng cửa sổ đăng nhập hiện tại.

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
 15:         // 1. Khởi tạo đối tượng DbContext để làm việc với CSDL
 16:         private SportsCenterContext _db;
 17: 
 18:         public RegisterWindow()
 19:         {
 20:             InitializeComponent();
 21:             _db = new SportsCenterContext(); // Khởi tạo kết nối DB
 22:             txtFullName.Focus();             // Đặt tiêu điểm chuột vào ô Họ tên khi mở cửa sổ
 23:         }
 24: 
 25:         // =====================================================================
 26:         // Hàm xóa sạch các dòng chữ báo lỗi màu đỏ trên form trước khi kiểm tra
 27:         // =====================================================================
 28:         private void ClearErrors()
 29:         {
 30:             errFullName.Text = string.Empty;
 31:             errPhone.Text = string.Empty;
 32:             errUsername.Text = string.Empty;
 33:             errPassword.Text = string.Empty;
 34:             errConfirm.Text = string.Empty;
 35:         }
 36: 
 37:         // =====================================================================
 38:         // Sự kiện khi người dùng nhấn nút "Tạo tài khoản" (btnCreate)
 39:         // =====================================================================
 40:         private void btnCreate_Click(object sender, RoutedEventArgs e)
 41:         {
 42:             // Bước 1: Xóa thông báo lỗi cũ và lấy dữ liệu người dùng nhập
 43:             ClearErrors();
 44:             string fullName = txtFullName.Text.Trim();
 45:             string phone = txtPhone.Text.Trim();
 46:             string username = txtUsername.Text.Trim();
 47:             string password = txtPassword.Password;
 48:             string confirm = txtConfirm.Password;
 49: 
 50:             // Biến cờ theo dõi xem toàn bộ dữ liệu có hợp lệ không
 51:             bool isValid = true;
 52:             var phoneCheck = (Result: AccountHelper.PhoneCheck.Free, Profile: (Member?)null);
 53: 
 54:             // Bước 2: Kiểm tra Họ và tên (FullName)
 55:             if (string.IsNullOrEmpty(fullName))
 56:             {
 57:                 errFullName.Text = "Vui lòng nhập họ tên.";
 58:                 isValid = false;
 59:             }
 60:             else
 61:             {
 62:                 string? nameError = AccountHelper.FullNameError(fullName);
 63:                 if (nameError != null)
 64:                 {
 65:                     errFullName.Text = nameError;
 66:                     isValid = false;
 67:                 }
 68:             }
 69: 
 70:             // Bước 3: Kiểm tra Số điện thoại (Phone)
 71:             if (string.IsNullOrEmpty(phone))
 72:             {
 73:                 errPhone.Text = "Vui lòng nhập số điện thoại.";
 74:                 isValid = false;
 75:             }
 76:             else if (!Regex.IsMatch(phone, @"^0\d{9,10}$"))
 77:             {
 78:                 errPhone.Text = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0.";
 79:                 isValid = false;
 80:             }
 81:             else
 82:             {
 83:                 // Quy tắc A5: Mỗi SĐT chỉ thuộc về một hội viên duy nhất
 84:                 phoneCheck = AccountHelper.CheckPhoneForRegistration(_db, fullName, phone);
 85:                 if (phoneCheck.Result == AccountHelper.PhoneCheck.AlreadyRegistered)
 86:                 {
 87:                     errPhone.Text = "Số điện thoại này đã được đăng ký. Hãy đăng nhập nếu là số của bạn.";
 88:                     isValid = false;
 89:                 }
 90:             }
 91: 
 92:             // Bước 4: Kiểm tra Tên đăng nhập (Username)
 93:             if (string.IsNullOrEmpty(username))
 94:             {
 95:                 errUsername.Text = "Vui lòng nhập tên đăng nhập.";
 96:                 isValid = false;
 97:             }
 98:             else
 99:             {
100:                 string? usernameError = AccountHelper.UsernameError(username);
101:                 if (usernameError != null)
102:                 {
103:                     errUsername.Text = usernameError;
104:                     isValid = false;
105:                 }
106:                 else if (_db.Users.Any(u => u.Username == username))
107:                 {
108:                     // Kiểm tra trùng tên đăng nhập trong Database
109:                     errUsername.Text = "Tên đăng nhập này đã có người dùng.";
110:                     isValid = false;
111:                 }
112:             }
113: 
114:             // Bước 5: Kiểm tra Mật khẩu (Password)
115:             if (string.IsNullOrEmpty(password))
116:             {
117:                 errPassword.Text = "Vui lòng nhập mật khẩu.";
118:                 isValid = false;
119:             }
120:             else if (password.Length < 6)
121:             {
122:                 errPassword.Text = "Mật khẩu cần ít nhất 6 ký tự.";
123:                 isValid = false;
124:             }
125:             else if (password.Equals(username, StringComparison.OrdinalIgnoreCase))
126:             {
127:                 errPassword.Text = "Mật khẩu không được trùng tên đăng nhập.";
128:                 isValid = false;
129:             }
130: 
131:             // Bước 6: Kiểm tra Xác nhận mật khẩu (Confirm Password)
132:             if (string.IsNullOrEmpty(confirm))
133:             {
134:                 errConfirm.Text = "Vui lòng nhập lại mật khẩu.";
135:                 isValid = false;
136:             }
137:             else if (confirm != password)
138:             {
139:                 errConfirm.Text = "Hai mật khẩu chưa khớp.";
140:                 isValid = false;
141:             }
142: 
143:             // Nếu có bất kỳ trường dữ liệu nào sai thì dừng lại không xử lý tiếp
144:             if (!isValid)
145:             {
146:                 return;
147:             }
148: 
149:             // Bước 7: Thực hiện lưu tài khoản vào Database
150:             try
151:             {
152:                 Member? linkProfile = null;
153: 
154:                 // Quy tắc A2: Nếu SĐT trùng với hồ sơ tạo tại quầy (chưa có tài khoản) và khớp họ tên
155:                 if (phoneCheck.Result == AccountHelper.PhoneCheck.LinkCounterProfile)
156:                 {
157:                     Member counterMember = phoneCheck.Profile!;
158:                     string confirmPrompt = $"Đã có hồ sơ tại quầy của {counterMember.FullName} (SĐT: {counterMember.Phone}).\n\n"
159:                                          + "Bạn có muốn gắn tài khoản mới này vào hồ sơ cũ để giữ lại các gói tập và lịch sử không?";
160:                     
161:                     MessageBoxResult answer = MessageBox.Show(confirmPrompt, "Xác nhận liên kết", MessageBoxButton.YesNo, MessageBoxImage.Question);
162:                     if (answer != MessageBoxResult.Yes)
163:                     {
164:                         errPhone.Text = "Số điện thoại này đã có hồ sơ tại quầy. Hãy gắn tài khoản hoặc dùng số khác.";
165:                         return;
166:                     }
167:                     linkProfile = counterMember;
168:                 }
169: 
170:                 // Gọi helper tạo User vai trò Member (và liên kết hồ sơ quầy nếu có)
171:                 User createdUser = AccountHelper.CreateMemberAccount(_db, username, password, fullName, phone, linkProfile);
172: 
173:                 // Tự động đăng nhập người dùng vừa tạo
174:                 Session.SignIn(_db, createdUser);
175: 
176:                 // Ghi nhận Audit Log nếu có thao tác liên kết tài khoản
177:                 if (linkProfile != null)
178:                 {
179:                     AuditHelper.Log(_db, "LinkMember", $"Tài khoản {username} gắn vào hồ sơ quầy #{linkProfile.MemberId}", "Members", linkProfile.MemberId);
180:                 }
181: 
182:                 // Thông báo thành công và chuyển sang màn hình chính
183:                 string successNote = linkProfile != null ? " Tài khoản đã được gắn vào hồ sơ tại quầy của bạn." : "";
184:                 MessageBox.Show("Tạo tài khoản thành công!" + successNote, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
185: 
186:                 MainWindow main = new MainWindow();
187:                 main.Show();
188:                 this.Close();
189:             }
190:             catch (Exception ex)
191:             {
192:                 MessageBox.Show("Đã xảy ra lỗi khi tạo tài khoản: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
193:             }
194:         }
195: 
196:         // =====================================================================
197:         // Sự kiện khi người dùng bấm nút "Đã có tài khoản? Đăng nhập" (btnBack)
198:         // =====================================================================
199:         private void btnBack_Click(object sender, RoutedEventArgs e)
200:         {
201:             LoginWindow loginWindow = new LoginWindow();
202:             loginWindow.Show();
203:             this.Close();
204:         }
205:     }
206: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 1-5**: Import thư viện xử lý Regex, Entities và WPF.
- **Dòng 10**: `public partial class RegisterWindow : Window` – Màn hình đăng ký hội viên.
- **Dòng 15**: `private SportsCenterContext _db;` – Khởi tạo DbContext.
- **Dòng 17-22**: Constructor: khởi tạo DB và đặt focus vào ô `txtFullName`.
- **Dòng 27-34**: `ClearErrors()`: Xóa sạch các thông báo lỗi màu đỏ trên form trước khi kiểm tra lại.
- **Dòng 39-192**: Sự kiện `btnCreate_Click` khi nhấn "Tạo tài khoản":
  - Dòng 42-47: Xóa lỗi cũ, lấy dữ liệu từ 5 ô nhập liệu (`fullName`, `phone`, `username`, `password`, `confirm`).
  - Dòng 50-51: Biến cờ `isValid = true` và biến tuple `phoneCheck`.
  - Dòng 54-68: Validate Họ và tên (không rỗng, kiểm tra định dạng qua `AccountHelper.FullNameError`).
  - Dòng 71-90: Validate Số điện thoại (10-11 số, bắt đầu bằng 0 qua Regex `^0\d{9,10}$`, kiểm tra qua `AccountHelper.CheckPhoneForRegistration`). Nếu đã đăng ký thì báo lỗi.
  - Dòng 93-112: Validate Tên đăng nhập (độ dài, ký tự hợp lệ, kiểm tra trùng bằng `_db.Users.Any(u => u.Username == username)`).
  - Dòng 115-129: Validate Mật khẩu (ít nhất 6 ký tự, không trùng tên đăng nhập).
  - Dòng 132-142: Validate Xác nhận mật khẩu (phải khớp mật khẩu).
  - Dòng 145-148: Nếu `!isValid` thì dừng lại, không thực hiện lưu.
  - Dòng 151-186: Khối `try-catch` lưu dữ liệu:
    - Dòng 156-170: Nếu phát hiện số điện thoại có hồ sơ quầy đúng họ tên (`phoneCheck.Result == LinkCounterProfile`), hiện `MessageBox.Show` với lựa chọn `YesNo` hỏi người dùng có muốn liên kết không. Nếu người dùng từ chối thì dừng lại.
    - Dòng 173: Gọi `AccountHelper.CreateMemberAccount` để tạo `User` và lưu vào DB.
    - Dòng 176: Tự động đăng nhập qua `Session.SignIn`.
    - Dòng 179-182: Nếu có liên kết hồ sơ quầy, ghi log `AuditHelper.Log`.
    - Dòng 185-189: Hiển thị thông báo thành công và chuyển sang `MainWindow`.
- **Dòng 198-203**: Sự kiện `btnBack_Click` khi bấm nút "Đã có tài khoản? Đăng nhập": Mở lại `LoginWindow` và đóng màn hình đăng ký.

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
- **Dòng 1-4**: Import các thư viện điều hướng và điều khiển WPF.
- **Dòng 9**: `public partial class MainWindow : Window` – Khung giao diện chính (Shell).
- **Dòng 14**: `public static MainWindow? Instance { get; private set; }` – Thể hiện tĩnh của cửa sổ chính, giúp bất kỳ trang con nào (như `MembersPage`) có thể gọi `MainWindow.Instance?.ShowPage(...)` để chuyển màn hình.
- **Dòng 17-26**: Các cấu trúc dữ liệu quản lý menu: `_buttons` (nút ứng với trang), `_groupOf` (nhóm chứa trang), `_groups` (danh sách nhóm), `_firstPage` (trang mở đầu tiên).
- **Dòng 28-48**: Constructor `MainWindow()`:
  - Dòng 31: Lưu `Instance = this`.
  - Dòng 34-39: Đọc thông tin từ `Session.CurrentUser` để hiển thị họ tên, username, tên vai trò tiếng Việt (`Rules.RoleNames`).
  - Dòng 42: Gọi `BuildMenu(user.Role)` để tạo menu tương ứng với vai trò.
  - Dòng 45-48: Mở trang đầu tiên mặc định vào vùng nội dung chính.
- **Dòng 53-83**: `BuildMenu(string role)` – Tạo menu động theo vai trò (Role-based Navigation):
  - `Manager`: Toàn quyền gồm Báo cáo, Lớp học, Bộ môn phòng, Member, Gói thành viên, Thanh toán, Tài khoản, Lịch sử hoạt động.
  - `Receptionist`: Quản lý hội viên, Bán gói, Đặt lớp, Thanh toán.
  - `Member`: Lịch của tôi, Đặt lớp, Gói thành viên, Thanh toán.
  - `Coach`: Lớp của tôi, Điểm danh.
- **Dòng 88-111**: `MakeButton(string title, Type pageType)` – Hàm tạo nút bấm menu: gán style `MenuButton`, bắt sự kiện Click để tạo thể hiện của trang bằng `Activator.CreateInstance(pageType)` và gọi `ShowPage`.
- **Dòng 116-120**: `AddItem(...)` – Thêm một mục menu đơn lẻ.
- **Dòng 125-163**: `AddGroup(...)` – Thêm nhóm menu dạng xổ xuống (`Expander`), hỗ trợ hiệu ứng đóng các nhóm khác khi mở một nhóm (Accordion).
- **Dòng 170-188**: `ShowPage(UserControl page)` – Hàm hiển thị một trang con vào `contentHost`, đồng thời đánh dấu trạng thái `Tag = "on"` để tô sáng nút menu tương ứng.
- **Dòng 193-199**: `btnChangePassword_Click` – Mở hộp thoại đổi mật khẩu dạng Modal Dialog (`ShowDialog()`).
- **Dòng 204-215**: `btnLogout_Click` – Xóa phiên đăng nhập qua `Session.Clear()`, mở lại `LoginWindow` và đóng cửa sổ chính.

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
 13:         // 1. Khởi tạo đối tượng DbContext để làm việc với CSDL
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
 25: 
 26:             // Đặt con trỏ chuột vào ô Mật khẩu hiện tại khi mở cửa sổ
 27:             Loaded += (s, e) => txtOld.Focus();
 28:         }
 29: 
 30:         // =====================================================================
 31:         // Sự kiện khi người dùng bấm nút "Lưu thay đổi" / "Đổi mật khẩu" (btnChange)
 32:         // =====================================================================
 33:         private void btnChange_Click(object sender, RoutedEventArgs e)
 34:         {
 35:             // Bước 1: Xóa thông báo lỗi cũ
 36:             errOld.Text = string.Empty;
 37:             errNew.Text = string.Empty;
 38:             errConfirm.Text = string.Empty;
 39: 
 40:             // Bước 2: Lấy dữ liệu các ô mật khẩu
 41:             string oldPassword = txtOld.Password;
 42:             string newPassword = txtNew.Password;
 43:             string confirmPassword = txtConfirm.Password;
 44: 
 45:             // Bước 3: Tìm tài khoản trong Database để cập nhật
 46:             User? user = _db.Users.Find(Session.CurrentUser!.UserId);
 47:             if (user == null)
 48:             {
 49:                 MessageBox.Show("Không tìm thấy thông tin tài khoản.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
 50:                 return;
 51:             }
 52: 
 53:             bool isValid = true;
 54: 
 55:             // Bước 4: Kiểm tra Mật khẩu hiện tại (Old Password)
 56:             if (string.IsNullOrEmpty(oldPassword))
 57:             {
 58:                 errOld.Text = "Vui lòng nhập mật khẩu hiện tại.";
 59:                 isValid = false;
 60:             }
 61:             else if (user.PasswordHash != PasswordHelper.Hash(oldPassword))
 62:             {
 63:                 errOld.Text = "Mật khẩu hiện tại chưa đúng.";
 64:                 isValid = false;
 65:             }
 66: 
 67:             // Bước 5: Kiểm tra Mật khẩu mới (New Password)
 68:             if (string.IsNullOrEmpty(newPassword))
 69:             {
 70:                 errNew.Text = "Vui lòng nhập mật khẩu mới.";
 71:                 isValid = false;
 72:             }
 73:             else if (newPassword.Length < 6)
 74:             {
 75:                 errNew.Text = "Mật khẩu cần ít nhất 6 ký tự.";
 76:                 isValid = false;
 77:             }
 78:             else if (newPassword == oldPassword)
 79:             {
 80:                 errNew.Text = "Mật khẩu mới phải khác mật khẩu hiện tại.";
 81:                 isValid = false;
 82:             }
 83:             else if (newPassword.Equals(user.Username, StringComparison.OrdinalIgnoreCase))
 84:             {
 85:                 errNew.Text = "Mật khẩu không được trùng tên đăng nhập.";
 86:                 isValid = false;
 87:             }
 88: 
 89:             // Bước 6: Kiểm tra Xác nhận mật khẩu mới (Confirm Password)
 90:             if (string.IsNullOrEmpty(confirmPassword))
 91:             {
 92:                 errConfirm.Text = "Vui lòng nhập lại mật khẩu mới.";
 93:                 isValid = false;
 94:             }
 95:             else if (confirmPassword != newPassword)
 96:             {
 97:                 errConfirm.Text = "Hai mật khẩu chưa khớp.";
 98:                 isValid = false;
 99:             }
100: 
101:             // Nếu có lỗi thì dừng lại
102:             if (!isValid)
103:             {
104:                 return;
105:             }
106: 
107:             // Bước 7: Thực hiện cập nhật mật khẩu mới vào Database
108:             try
109:             {
110:                 // Băm mật khẩu mới bằng SHA-256 trước khi lưu
111:                 user.PasswordHash = PasswordHelper.Hash(newPassword);
112:                 _db.SaveChanges(); // Lưu vào SQL Server
113: 
114:                 // Ghi lại lịch sử hoạt động đổi mật khẩu
115:                 AuditHelper.Log(_db, "ChangePassword", $"{user.Username} đổi mật khẩu", "Users", user.UserId);
116: 
117:                 MessageBox.Show("Đã đổi mật khẩu thành công. Lần đăng nhập sau hãy dùng mật khẩu mới.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
118:                 this.Close(); // Đóng cửa sổ đổi mật khẩu
119:             }
120:             catch (Exception ex)
121:             {
122:                 MessageBox.Show("Lỗi khi lưu mật khẩu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
123:             }
124:         }
125:     }
126: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 1-3**: Import Entities và WPF.
- **Dòng 8**: `public partial class ChangePasswordWindow : Window` – Hộp thoại đổi mật khẩu.
- **Dòng 13**: `private SportsCenterContext _db;` – Khởi tạo DbContext.
- **Dòng 15-28**: Constructor: Khởi tạo kết nối DB, nạp thông tin người dùng từ `Session.CurrentUser` hiển thị lên thanh `txtInfo`, và đặt focus vào ô mật khẩu cũ.
- **Dòng 33-131**: Sự kiện `btnChange_Click` khi nhấn nút "Lưu thay đổi":
  - Dòng 36-39: Xóa các thông báo lỗi cũ trên giao diện.
  - Dòng 42-44: Lấy giá trị 3 ô mật khẩu: `oldPassword`, `newPassword`, `confirmPassword`.
  - Dòng 47-52: Tìm bản ghi người dùng trong CSDL bằng `_db.Users.Find(...)`.
  - Dòng 57-67: Kiểm tra mật khẩu hiện tại (không rỗng, băm bằng `PasswordHelper.Hash` phải trùng với `user.PasswordHash`).
  - Dòng 70-93: Kiểm tra mật khẩu mới (không rỗng, độ dài $\ge 6$, khác mật khẩu cũ, không trùng tên đăng nhập).
  - Dòng 96-106: Kiểm tra xác nhận mật khẩu mới (phải khớp mật khẩu mới).
  - Dòng 109-112: Nếu có lỗi thì dừng lại.
  - Dòng 115-130: Khối `try-catch` cập nhật mật khẩu:
    - Băm mật khẩu mới: `user.PasswordHash = PasswordHelper.Hash(newPassword)`.
    - Lưu vào DB: `_db.SaveChanges()`.
    - Ghi nhận lịch sử: `AuditHelper.Log(_db, "ChangePassword", ...)`.
    - Hiển thị `MessageBox.Show` thông báo thành công và đóng cửa sổ.

---

# 9. FILE: `Pages/UsersPage.xaml.cs` (Code-behind Quản lý Tài khoản - Manager)

### Đường dẫn: `SportsCenterApp/SportsCenter/Pages/UsersPage.xaml.cs`

### Toàn bộ mã nguồn:
```csharp
  1: using SportsCenter.Entities;
  2: using System;
  3: using System.Collections.Generic;
  4: using System.Linq;
  5: using System.Windows;
  6: using System.Windows.Controls;
  7: 
  8: namespace SportsCenter
  9: {
 10:     /// <summary>
 11:     /// Interaction logic for UsersPage.xaml
 12:     /// Màn hình Quản lý tài khoản người dùng (Dành cho Quản lý / Manager)
 13:     /// Cấu trúc thiết kế chuẩn theo mẫu bài thi PRN212:
 14:     /// - Khởi tạo DbContext
 15:     /// - Load ComboBox bộ lọc và form thêm mới
 16:     /// - Load dữ liệu lên DataGrid bằng LINQ (Filter theo từ khóa và ComboBox)
 17:     /// - Các chức năng: Lọc, Xóa bộ lọc (Clear Filter), Khóa/Mở tài khoản, Đổi vai trò, Thêm tài khoản mới
 18:     /// </summary>
 19:     public partial class UsersPage : UserControl
 20:     {
 21:         // 1. Khai báo DbContext kết nối Database (chuẩn PRN212)
 22:         private SportsCenterContext _db;
 23: 
 24:         // 2. Danh sách các mã vai trò lưu trong CSDL
 25:         private readonly string[] _roles = { "Manager", "Receptionist", "Coach", "Member" };
 26: 
 27:         // 3. Hàm chuyển đổi mã Role trong DB sang tên hiển thị tiếng Việt
 28:         private static string RoleName(string code)
 29:         {
 30:             if (Rules.RoleNames.ContainsKey(code))
 31:             {
 32:                 return Rules.RoleNames[code];
 33:             }
 34:             return code;
 35:         }
 36: 
 37:         public UsersPage()
 38:         {
 39:             InitializeComponent();
 40:             _db = new SportsCenterContext(); // Khởi tạo kết nối CSDL
 41: 
 42:             // Tái sử dụng GridHelper để tự động căn chỉnh và định dạng cột DataGrid
 43:             GridHelper.Setup(dgUsers);
 44: 
 45:             // Gán sự kiện khi bấm chuyển trang trên PagerControl thì nạp lại danh sách
 46:             pgUsers.PageChanged += (s, e) => LoadUsers();
 47: 
 48:             // Nạp dữ liệu ban đầu cho các ComboBox và DataGrid
 49:             LoadData();
 50:         }
 51: 
 52:         // =====================================================================
 53:         // Bước 1: Nạp danh sách vai trò vào các ComboBox trên giao diện
 54:         // =====================================================================
 55:         private void LoadData()
 56:         {
 57:             // Chuyển danh sách mã vai trò sang tên tiếng Việt
 58:             List<string> roleNames = _roles.Select(RoleName).ToList();
 59: 
 60:             // 1. Nạp dữ liệu cho ComboBox Lọc (cbxRoleFilter): Thêm lựa chọn "Tất cả" ở đầu danh sách (chuẩn PRN)
 61:             List<string> filterRoles = new List<string> { "Tất cả" };
 62:             filterRoles.AddRange(roleNames);
 63:             cbxRoleFilter.ItemsSource = filterRoles;
 64:             cbxRoleFilter.SelectedIndex = 0; // Mặc định chọn "Tất cả"
 65: 
 66:             // 2. Nạp dữ liệu cho ComboBox Đổi vai trò (cbxNewRole)
 67:             cbxNewRole.ItemsSource = roleNames;
 68:             cbxNewRole.SelectedIndex = 3; // Mặc định chọn "Hội viên"
 69: 
 70:             // 3. Nạp dữ liệu cho ComboBox Thêm tài khoản mới (cbxRole)
 71:             cbxRole.ItemsSource = roleNames;
 72:             cbxRole.SelectedIndex = 1; // Mặc định chọn "Lễ tân"
 73: 
 74:             // 4. Nạp danh sách người dùng lên DataGrid
 75:             LoadUsers();
 76:         }
 77: 
 78:         // =====================================================================
 79:         // Bước 2: Nạp và lọc danh sách tài khoản lên DataGrid bằng LINQ
 80:         // (Tương tự phương thức LoadEmployee / LoadCourse trong bài PRN212)
 81:         // =====================================================================
 82:         private void LoadUsers()
 83:         {
 84:             // Khởi tạo truy vấn LINQ từ bảng Users
 85:             var query = _db.Users.AsQueryable();
 86: 
 87:             // 1. Lọc theo từ khóa tìm kiếm (Tên đăng nhập hoặc Họ tên) nếu có nhập
 88:             string keyword = txtSearch.Text.Trim();
 89:             if (!string.IsNullOrEmpty(keyword))
 90:             {
 91:                 query = query.Where(u => u.Username.Contains(keyword) || u.FullName.Contains(keyword));
 92:             }
 93: 
 94:             // 2. Lọc theo Vai trò nếu ComboBox khác "Tất cả" (SelectedIndex > 0)
 95:             if (cbxRoleFilter.SelectedIndex > 0)
 96:             {
 97:                 string selectedRole = _roles[cbxRoleFilter.SelectedIndex - 1];
 98:                 query = query.Where(u => u.Role == selectedRole);
 99:             }
100: 
101:             // 3. Sắp xếp theo UserId tăng dần và chọn các cột cần hiển thị
102:             var rows = query
103:                 .OrderBy(u => u.UserId)
104:                 .Select(u => new
105:                 {
106:                     u.UserId,
107:                     u.Username,
108:                     u.FullName,
109:                     Role = RoleName(u.Role),
110:                     Status = u.IsActive ? "Đang hoạt động" : "Đã khóa"
111:                 })
112:                 .ToList();
113: 
114:             // 4. Đổ dữ liệu vào DataGrid (kết hợp phân trang qua pgUsers)
115:             dgUsers.ItemsSource = pgUsers.Slice(rows);
116:         }
117: 
118:         // =====================================================================
119:         // Sự kiện khi bấm nút "Lọc" (btnFilter)
120:         // =====================================================================
121:         private void btnFilter_Click(object sender, RoutedEventArgs e)
122:         {
123:             pgUsers.Reset(); // Quay về trang 1
124:             LoadUsers();     // Nạp lại danh sách theo điều kiện lọc
125:         }
126: 
127:         // =====================================================================
128:         // Sự kiện khi bấm nút "Bỏ lọc" (btnClearFilter) - Chuẩn bài thi PRN
129:         // =====================================================================
130:         private void btnClearFilter_Click(object sender, RoutedEventArgs e)
131:         {
132:             txtSearch.Clear();               // Xóa trắng ô tìm kiếm
133:             cbxRoleFilter.SelectedIndex = 0; // Đặt lại ComboBox lọc về "Tất cả"
134:             pgUsers.Reset();                 // Quay về trang 1
135:             LoadUsers();                     // Nạp lại toàn bộ danh sách ban đầu
136:         }
137: 
138:         // =====================================================================
139:         // Lấy thông tin tài khoản đang được chọn trong DataGrid
140:         // =====================================================================
141:         private User? GetSelectedUser()
142:         {
143:             // Lấy ID từ dòng đang chọn trên DataGrid
144:             int? id = GridHelper.SelectedId(dgUsers, "UserId");
145:             if (id == null)
146:             {
147:                 MessageBox.Show("Vui lòng chọn một tài khoản trong danh sách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
148:                 return null;
149:             }
150: 
151:             // Quy tắc A3: Không cho phép tự khóa hoặc đổi quyền của chính tài khoản đang đăng nhập
152:             if (id == Session.CurrentUser!.UserId)
153:             {
154:                 MessageBox.Show("Không thể khóa hoặc đổi vai trò của chính tài khoản đang đăng nhập.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
155:                 return null;
156:             }
157: 
158:             // Tìm và trả về đối tượng User từ Database
159:             return _db.Users.Find(id);
160:         }
161: 
162:         // =====================================================================
163:         // Sự kiện khi bấm nút "Khóa / Mở khóa tài khoản" (btnToggle)
164:         // =====================================================================
165:         private void btnToggle_Click(object sender, RoutedEventArgs e)
166:         {
167:             // Bước 1: Lấy tài khoản đang chọn
168:             User? user = GetSelectedUser();
169:             if (user == null) return;
170: 
171:             try
172:             {
173:                 // Bước 2: Đảo ngược trạng thái hoạt động (Active <-> Inactive)
174:                 user.IsActive = !user.IsActive;
175:                 _db.SaveChanges(); // Lưu vào Database
176: 
177:                 // Bước 3: Ghi lịch sử hoạt động (Audit Log)
178:                 string actionText = user.IsActive ? "Mở khóa" : "Khóa";
179:                 AuditHelper.Log(_db, "ToggleUser", $"{actionText} tài khoản {user.Username}", "Users", user.UserId);
180: 
181:                 // Bước 4: Nạp lại danh sách và giữ dòng đang chọn
182:                 LoadUsers();
183:                 GridHelper.SelectById(dgUsers, "UserId", user.UserId);
184: 
185:                 MessageBox.Show($"Đã {actionText.ToLower()} tài khoản {user.Username} thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
186:             }
187:             catch (Exception ex)
188:             {
189:                 MessageBox.Show("Lỗi khi cập nhật trạng thái: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
190:             }
191:         }
192: 
193:         // =====================================================================
194:         // Sự kiện khi bấm nút "Đổi vai trò" (btnChangeRole)
195:         // =====================================================================
196:         private void btnChangeRole_Click(object sender, RoutedEventArgs e)
197:         {
198:             // Bước 1: Lấy tài khoản đang chọn
199:             User? user = GetSelectedUser();
200:             if (user == null) return;
201: 
202:             // Bước 2: Lấy vai trò mới từ ComboBox
203:             string newRole = _roles[cbxNewRole.SelectedIndex];
204:             if (newRole == user.Role)
205:             {
206:                 MessageBox.Show("Tài khoản này đã có vai trò " + RoleName(newRole) + ".", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
207:                 return;
208:             }
209: 
210:             try
211:             {
212:                 // Bước 3: Cập nhật vai trò mới cho User
213:                 user.Role = newRole;
214: 
215:                 // Đảm bảo tạo hồ sơ tương ứng trong bảng Coaches hoặc Members nếu chưa có
216:                 EnsureProfile(user);
217: 
218:                 _db.SaveChanges(); // Lưu vào Database
219: 
220:                 // Bước 4: Ghi nhận Audit Log
221:                 AuditHelper.Log(_db, "SetRole", $"Đổi vai trò {user.Username} thành {RoleName(newRole)}", "Users", user.UserId);
222: 
223:                 // Bước 5: Nạp lại dữ liệu hiển thị
224:                 LoadUsers();
225:                 GridHelper.SelectById(dgUsers, "UserId", user.UserId);
226: 
227:                 MessageBox.Show($"Đã đổi vai trò của {user.Username} thành {RoleName(newRole)}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
228:             }
229:             catch (Exception ex)
230:             {
231:                 MessageBox.Show("Lỗi khi đổi vai trò: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
232:             }
233:         }
234: 
235:         // =====================================================================
236:         // Hàm đồng bộ hồ sơ: Nếu User có role Coach thì phải có dòng trong bảng Coaches,
237:         // nếu có role Member thì phải có dòng trong bảng Members
238:         // =====================================================================
239:         private void EnsureProfile(User user)
240:         {
241:             // Nếu là Coach và chưa có hồ sơ huấn luyện viên -> Tự động thêm hồ sơ Coach
242:             if (user.Role == "Coach" && !_db.Coaches.Any(c => c.UserId == user.UserId))
243:             {
244:                 Coach newCoach = new Coach
245:                 {
246:                     User = user,
247:                     FullName = user.FullName,
248:                     Email = user.Username + "@center.vn",
249:                     Phone = user.Phone
250:                 };
251:                 _db.Coaches.Add(newCoach);
252:             }
253: 
254:             // Nếu là Member và chưa có hồ sơ hội viên -> Tự động thêm hồ sơ Member
255:             if (user.Role == "Member" && !_db.Members.Any(m => m.UserId == user.UserId))
256:             {
257:                 Member newMember = new Member
258:                 {
259:                     User = user,
260:                     FullName = user.FullName,
261:                     Phone = user.Phone,
262:                     CreatedAt = DateTime.Now
263:                 };
264:                 _db.Members.Add(newMember);
265:             }
266:         }
267: 
268:         // =====================================================================
269:         // Sự kiện khi bấm nút "Thêm tài khoản mới" (btnAddUser)
270:         // (Tương tự phương thức btnAddEmp_Click / btnAddCourse_Click trong bài PRN212)
271:         // =====================================================================
272:         private void btnAddUser_Click(object sender, RoutedEventArgs e)
273:         {
274:             // Bước 1: Lấy dữ liệu từ các ô nhập liệu
275:             string fullName = txtFullName.Text.Trim();
276:             string username = txtUsername.Text.Trim();
277: 
278:             // Bước 2: Kiểm tra dữ liệu rỗng
279:             if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username))
280:             {
281:                 MessageBox.Show("Vui lòng nhập Họ tên và Tên đăng nhập.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
282:                 return;
283:             }
284: 
285:             // Bước 3: Kiểm tra định dạng Họ tên hợp lệ
286:             string? nameError = AccountHelper.FullNameError(fullName);
287:             if (nameError != null)
288:             {
289:                 MessageBox.Show(nameError, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
290:                 return;
291:             }
292: 
293:             // Bước 4: Kiểm tra định dạng Tên đăng nhập hợp lệ
294:             string? usernameError = AccountHelper.UsernameError(username);
295:             if (usernameError != null)
296:             {
297:                 MessageBox.Show(usernameError, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
298:                 return;
299:             }
300: 
301:             // Bước 5: Kiểm tra xem Tên đăng nhập đã tồn tại trong CSDL chưa
302:             if (_db.Users.Any(u => u.Username == username))
303:             {
304:                 MessageBox.Show("Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
305:                 return;
306:             }
307: 
308:             // Bước 6: Thêm tài khoản mới vào CSDL
309:             try
310:             {
311:                 // Khởi tạo đối tượng User mới với mật khẩu mặc định "123456"
312:                 User newUser = new User
313:                 {
314:                     Username = username,
315:                     PasswordHash = PasswordHelper.Hash("123456"), // Băm SHA-256
316:                     FullName = fullName,
317:                     Role = _roles[cbxRole.SelectedIndex],         // Lấy vai trò được chọn từ ComboBox
318:                     IsActive = true,                              // Mặc định kích hoạt
319:                     CreatedAt = DateTime.Now
320:                 };
321: 
322:                 _db.Users.Add(newUser);
323:                 EnsureProfile(newUser); // Tạo hồ sơ Coach/Member tương ứng nếu có
324:                 _db.SaveChanges();      // Lưu vào Database
325: 
326:                 // Ghi lịch sử hoạt động tạo tài khoản
327:                 AuditHelper.Log(_db, "CreateUser", $"Tạo tài khoản {username} ({RoleName(newUser.Role)})", "Users", newUser.UserId);
328: 
329:                 // Xóa trắng form nhập liệu sau khi thêm thành công (giống ClearForm trong PRN)
330:                 txtFullName.Clear();
331:                 txtUsername.Clear();
332: 
333:                 // Nạp lại danh sách hiển thị
334:                 pgUsers.Reset();
335:                 LoadUsers();
336: 
337:                 MessageBox.Show($"Đã tạo tài khoản {username} thành công! Mật khẩu mặc định là 123456.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
338:             }
339:             catch (Exception ex)
340:             {
341:                 MessageBox.Show("Lỗi khi tạo tài khoản: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
342:             }
343:         }
344:     }
345: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 1-5**: Import Entities, LINQ, Collections và các điều khiển WPF.
- **Dòng 18**: `public partial class UsersPage : UserControl` – Trang quản lý tài khoản người dùng, thiết kế theo chuẩn câu 2 đề thi PRN212.
- **Dòng 21**: `private SportsCenterContext _db;` – Khởi tạo DbContext riêng cho trang.
- **Dòng 24**: Mảng hằng số `_roles` chứa 4 mã vai trò trong DB: `Manager`, `Receptionist`, `Coach`, `Member`.
- **Dòng 27-34**: Hàm tĩnh `RoleName(code)` ánh xạ mã vai trò tiếng Anh sang tên tiếng Việt hiển thị (Quản lý, Lễ tân, Huấn luyện viên, Hội viên).
- **Dòng 36-48**: Constructor `UsersPage()`:
  - Khởi tạo `_db = new SportsCenterContext()`.
  - Gọi `GridHelper.Setup(dgUsers)` tự động sinh cột và căn chỉnh DataGrid.
  - Đăng ký sự kiện chuyển trang `pgUsers.PageChanged += (s, e) => LoadUsers()`.
  - Gọi `LoadData()` nạp dữ liệu ban đầu.
- **Dòng 53-73**: Phương thức `LoadData()`:
  - Dòng 56: Lấy danh sách tên vai trò tiếng Việt.
  - Dòng 59-63: Nạp ComboBox bộ lọc `cbxRoleFilter` với phần tử `"Tất cả"` chèn ở vị trí index 0 (`filterRoles.Insert(0, ...)`), mặc định chọn `SelectedIndex = 0` (chuẩn đề thi PRN212).
  - Dòng 66-70: Nạp dữ liệu cho ComboBox đổi vai trò (`cbxNewRole`) và thêm tài khoản (`cbxRole`).
  - Dòng 73: Gọi `LoadUsers()` nạp danh sách lên DataGrid.
- **Dòng 79-114**: Phương thức `LoadUsers()` (Tương tự `LoadEmployee` / `LoadCourse` trong PRN):
  - Dòng 82: Khởi tạo câu truy vấn LINQ: `var query = _db.Users.AsQueryable();`.
  - Dòng 85-89: Lọc theo từ khóa tìm kiếm nếu có nhập: `query = query.Where(u => u.Username.Contains(keyword) || u.FullName.Contains(keyword));`.
  - Dòng 92-96: Lọc theo vai trò nếu ComboBox chọn khác `"Tất cả"`: `query = query.Where(u => u.Role == selectedRole);`.
  - Dòng 99-110: Sắp xếp theo `UserId`, dùng phép chiếu `.Select(...)` nạp các thuộc tính hiển thị (mã, tên đăng nhập, họ tên, vai trò tiếng Việt, trạng thái hoạt động).
  - Dòng 113: Đổ dữ liệu vào `dgUsers.ItemsSource = pgUsers.Slice(rows)`.
- **Dòng 119-123**: `btnFilter_Click` – Sự kiện bấm nút "Lọc": Reset trang về 1 và gọi `LoadUsers()`.
- **Dòng 128-134**: `btnClearFilter_Click` – Sự kiện bấm nút "Bỏ lọc": Xóa trắng ô tìm kiếm, đưa `cbxRoleFilter.SelectedIndex = 0`, reset trang và gọi `LoadUsers()` (chuẩn PRN212).
- **Dòng 139-158**: `GetSelectedUser()` – Lấy tài khoản đang chọn trên DataGrid:
  - Lấy Id qua `GridHelper.SelectedId(dgUsers, "UserId")`.
  - Quy tắc A3: Chặn không cho Quản lý tự khóa hoặc tự đổi quyền của chính tài khoản đang đăng nhập (`id == Session.CurrentUser!.UserId`).
  - Tìm và trả về `_db.Users.Find(id)`.
- **Dòng 163-191**: `btnToggle_Click` – Khóa / Mở khóa tài khoản:
  - Lấy tài khoản đang chọn qua `GetSelectedUser()`.
  - Đảo ngược trạng thái hoạt động: `user.IsActive = !user.IsActive`.
  - Lưu vào DB: `_db.SaveChanges()`.
  - Ghi lịch sử: `AuditHelper.Log(_db, "ToggleUser", ...)`.
  - Nạp lại danh sách `LoadUsers()` và chọn lại đúng dòng đó `GridHelper.SelectById`.
- **Dòng 196-234**: `btnChangeRole_Click` – Đổi vai trò tài khoản:
  - Lấy tài khoản đang chọn.
  - Lấy vai trò mới từ `cbxNewRole`, kiểm tra nếu trùng vai trò hiện tại thì cảnh báo.
  - Cập nhật `user.Role = newRole`, gọi `EnsureProfile(user)` để tạo hồ sơ tương ứng nếu chưa có.
  - Lưu vào DB `_db.SaveChanges()`, ghi nhật ký `AuditHelper.Log`, và nạp lại danh sách.
- **Dòng 240-268**: `EnsureProfile(User user)` – Đảm bảo tính toàn vẹn hồ sơ:
  - Nếu Role chuyển sang `"Coach"` và chưa có trong bảng `Coaches` thì tự động thêm mới bản ghi `Coach`.
  - Nếu Role chuyển sang `"Member"` và chưa có trong bảng `Members` thì tự động thêm mới bản ghi `Member`.
- **Dòng 274-340**: `btnAddUser_Click` – Thêm tài khoản mới (chuẩn `btnAdd..._Click` trong bài PRN212):
  - Lấy `fullName` và `username`, validate không được rỗng.
  - Kiểm tra tính hợp lệ qua `AccountHelper.FullNameError` và `AccountHelper.UsernameError`.
  - Kiểm tra trùng tên đăng nhập bằng `_db.Users.Any(u => u.Username == username)`.
  - Tạo đối tượng `User` mới với mật khẩu mặc định `"123456"` (được băm bằng `PasswordHelper.Hash`).
  - Thêm vào DB: `_db.Users.Add(newUser); EnsureProfile(newUser); _db.SaveChanges();`.
  - Ghi nhật ký: `AuditHelper.Log(_db, "CreateUser", ...)`.
  - Xóa trắng form nhập liệu, nạp lại danh sách và thông báo thành công qua `MessageBox.Show`.

---
*Tài liệu được cập nhật tự động và đồng bộ 100% với mã nguồn mới nhất trên nhánh main.*
