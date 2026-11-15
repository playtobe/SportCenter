# GIẢI THÍCH TOÀN BỘ CODE TỪNG DÒNG – PHẦN NGƯỜI 1: TÀI KHOẢN VÀ KHUNG ỨNG DỤNG
## DỰ ÁN SPORTS CENTER (PRN212 - WPF & EF CORE 8)

> **Phạm vi Người 1 phụ trách gồm 9 file C#**:
> 1. `Entities/User.cs`
> 2. `Helpers/PasswordHelper.cs`
> 3. `Helpers/Session.cs`
> 4. `Helpers/AccountHelper.cs`
> 5. `LoginWindow.xaml.cs`
> 6. `RegisterWindow.xaml.cs`
> 7. `MainWindow.xaml.cs`
> 8. `ChangePasswordWindow.xaml.cs`
> 9. `Pages/UsersPage.xaml.cs`

---

# 1. FILE: `Entities/User.cs` (Mô hình Thực thể Tài khoản)

### Mã nguồn:
```csharp
1: namespace SportsCenter.Entities;
2: 
3: public partial class User
4: {
5:     public int UserId { get; set; }
6: 
7:     public string Username { get; set; } = null!;
8: 
9:     public string PasswordHash { get; set; } = null!;
10: 
11:     public string FullName { get; set; } = null!;
12: 
13:     public string? Email { get; set; }
14: 
15:     public string? Phone { get; set; }
16: 
17:     public string Role { get; set; } = null!;
18: 
19:     public bool IsActive { get; set; }
20: 
21:     public DateTime CreatedAt { get; set; }
22: 
23:     public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
24: 
25:     public virtual ICollection<Coach> Coaches { get; set; } = new List<Coach>();
26: 
27:     public virtual ICollection<Member> Members { get; set; } = new List<Member>();
28: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 1**: `namespace SportsCenter.Entities;` – Khai báo không gian tên chứa các thực thể CSDL (Entity Models).
- **Dòng 3**: `public partial class User` – Định nghĩa lớp thực thể `User` ánh xạ tới bảng `Users` trong SQL Server. Từ khóa `partial` cho phép mở rộng lớp nếu cần.
- **Dòng 5**: `public int UserId { get; set; }` – Khóa chính (Primary Key), tự tăng (`IDENTITY(1,1)`) trong SQL Server.
- **Dòng 7**: `public string Username { get; set; } = null!;` – Tên đăng nhập duy nhất (`UNIQUE`). Cú pháp `= null!;` (null-forgiving operator) báo cho trình biên dịch C# không cảnh báo null vì EF Core sẽ nạp dữ liệu từ CSDL vào.
- **Dòng 9**: `public string PasswordHash { get; set; } = null!;` – Chứa chuỗi hex 64 ký tự đại diện cho mã băm SHA-256 của mật khẩu, tuyệt đối không lưu mật khẩu thô.
- **Dòng 11**: `public string FullName { get; set; } = null!;` – Họ và tên hiển thị của người dùng (tối đa 100 ký tự).
- **Dòng 13**: `public string? Email { get; set; }` – Dấu `?` biểu thị kiểu có thể nhận `null` (`NVARCHAR(150) NULL`), vì hệ thống không bắt buộc nhập email (không gửi mail).
- **Dòng 15**: `public string? Phone { get; set; }` – Số điện thoại liên lạc của tài khoản.
- **Dòng 17**: `public string Role { get; set; } = null!;` – Vai trò tài khoản, bị ràng buộc bởi CHECK constraint trong DB: chỉ nhận 1 trong 4 giá trị: `Manager`, `Receptionist`, `Coach`, `Member`.
- **Dòng 19**: `public bool IsActive { get; set; }` – Cờ trạng thái: `true` là đang hoạt động, `false` là bị khóa (chặn đăng nhập).
- **Dòng 21**: `public DateTime CreatedAt { get; set; }` – Thời điểm tạo tài khoản, mặc định lấy `GETDATE()` từ SQL.
- **Dòng 23-27**: Các thuộc tính điều hướng quan hệ (Navigation Properties):
  - `AuditLogs`: Danh sách các vết thao tác do User này thực hiện.
  - `Coaches`: Tập hợp huấn luyện viên liên kết (quan hệ 1-1 qua `UX_Coaches_UserId`).
  - `Members`: Tập hợp hội viên liên kết (quan hệ 1-1 qua `UX_Members_UserId`).

---

# 2. FILE: `Helpers/PasswordHelper.cs` (Tiện ích Băm Mật khẩu)

### Mã nguồn:
```csharp
1: using System.Security.Cryptography;
2: using System.Text;
3: 
4: namespace SportsCenter
5: {
6:     public static class PasswordHelper
7:     {
8:         // SHA-256, chuỗi hex IN HOA (khớp với dữ liệu mẫu trong SportsCenter.sql)
9:         public static string Hash(string password) =>
10:             Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
11:     }
12: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 1-2**: Import thư viện mật mã học `System.Security.Cryptography` và mã hóa chuỗi `System.Text`.
- **Dòng 6**: `public static class PasswordHelper` – Lớp tiện ích tĩnh, không cần khởi tạo đối tượng khi dùng.
- **Dòng 9**: `public static string Hash(string password) =>` – Hàm băm nhận vào chuỗi mật khẩu gốc (plain-text) và trả về chuỗi băm hex.
- **Dòng 10**:
  - `Encoding.UTF8.GetBytes(password)`: Biến đổi chuỗi ký tự mật khẩu thành mảng byte chuẩn UTF-8.
  - `SHA256.HashData(...)`: Thực hiện thuật toán băm SHA-256 an toàn một chiều, trả về mảng 32 bytes.
  - `Convert.ToHexString(...)`: Chuyển 32 bytes sang chuỗi ký tự Hexadecimal 64 ký tự **IN HOA** (ví dụ: `BA3253876AED6BC044DADE...`). Cách băm này khớp 100% với hàm `HASHBYTES('SHA2_256', '123456')` của SQL Server.

---

# 3. FILE: `Helpers/Session.cs` (Quản lý Phiên Đăng nhập)

### Mã nguồn:
```csharp
1: using Microsoft.EntityFrameworkCore;
2: using SportsCenter.Entities;
3: 
4: namespace SportsCenter
5: {
6:     // Lưu thông tin người đang đăng nhập (dùng chung cho mọi màn hình)
7:     public static class Session
8:     {
9:         public static User? CurrentUser { get; set; }
10: 
11:         // Có giá trị khi người đăng nhập là Member
12:         public static Member? CurrentMember { get; set; }
13: 
14:         // Có giá trị khi người đăng nhập là Coach
15:         public static Coach? CurrentCoach { get; set; }
16: 
17:         public static string Role => CurrentUser?.Role ?? "";
18: 
19:         public static void SignIn(SportsCenterContext db, User user)
20:         {
21:             CurrentUser = user;
22:             CurrentMember = user.Role == "Member" ? db.Members.AsNoTracking().FirstOrDefault(m => m.UserId == user.UserId) : null;
23:             CurrentCoach = user.Role == "Coach" ? db.Coaches.AsNoTracking().FirstOrDefault(c => c.UserId == user.UserId) : null;
24:             AuditHelper.Log(db, "Login", "Đăng nhập hệ thống");
25:         }
26: 
27:         public static void Clear()
28:         {
29:             CurrentUser = null;
30:             CurrentMember = null;
31:             CurrentCoach = null;
32:         }
33:     }
34: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 7**: `public static class Session` – Lớp tĩnh lưu trữ ngữ cảnh bảo mật trong suốt thời gian ứng dụng chạy.
- **Dòng 9**: `public static User? CurrentUser { get; set; }` – Con trỏ giữ thông tin tài khoản người dùng đang đăng nhập.
- **Dòng 12**: `public static Member? CurrentMember { get; set; }` – Giữ thực thể `Member` tương ứng nếu người đăng nhập có vai trò là Member.
- **Dòng 15**: `public static Coach? CurrentCoach { get; set; }` – Giữ thực thể `Coach` tương ứng nếu người đăng nhập là Coach.
- **Dòng 17**: `public static string Role => CurrentUser?.Role ?? "";` – Thuộc tính chỉ đọc tiện ích, trả về Role của User hoặc chuỗi rỗng nếu chưa đăng nhập.
- **Dòng 19-25**: Hàm `SignIn(SportsCenterContext db, User user)`:
  - Dòng 21: Gán `CurrentUser = user`.
  - Dòng 22: Nếu vai trò là `"Member"`, truy vấn bảng `Members` (dùng `AsNoTracking()` để đọc nhanh không lưu cache) theo `user.UserId`.
  - Dòng 23: Nếu vai trò là `"Coach"`, truy vấn bảng `Coaches` theo `user.UserId`.
  - Dòng 24: Gọi `AuditHelper.Log` để tự động ghi lại lịch sử đăng nhập vào bảng `AuditLogs`.
- **Dòng 27-32**: Hàm `Clear()`: Gán 3 biến về `null` khi đăng xuất để dọn dẹp sạch sẽ bộ nhớ phiên.

---

# 4. FILE: `Helpers/AccountHelper.cs` (Xác thực, Chuẩn hóa & Gắn Tài khoản)

### Mã nguồn:
```csharp
1: using Microsoft.EntityFrameworkCore;
2: using SportsCenter.Entities;
3: using System.Globalization;
4: using System.Text;
5: using System.Text.RegularExpressions;
6: 
7: namespace SportsCenter
8: {
9:     // Tạo tài khoản Member và gắn vào hồ sơ đã đăng ký tại quầy (nếu có).
10:     // Không có MessageBox nào ở đây: phần hỏi xác nhận nằm ở RegisterWindow.
11:     public static class AccountHelper
12:     {
13:         // Bỏ dấu, đổi thành chữ thường, gộp khoảng trắng: "  Nguyễn  Văn Đạt " == "nguyen van dat"
14:         public static string Normalize(string text)
15:         {
16:             string lower = text.Trim().ToLowerInvariant().Replace('đ', 'd');
17:             var sb = new StringBuilder();
18:             foreach (char ch in lower.Normalize(NormalizationForm.FormD))
19:             {
20:                 if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
21:                     sb.Append(ch);
22:             }
23:             return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
24:         }
25: 
26:         // Mỗi số điện thoại chỉ thuộc về MỘT member (kể cả hồ sơ đã có tài khoản hay đăng ký tại quầy).
27:         // Trả về hồ sơ đang dùng số này, hoặc null nếu số còn trống.
28:         public static Member? FindByPhone(SportsCenterContext db, string phone) =>
29:             db.Members.AsNoTracking().OrderBy(m => m.MemberId).FirstOrDefault(m => m.Phone == phone);
30: 
31:         // Kết quả kiểm tra số điện thoại khi tự đăng ký tài khoản
32:         public enum PhoneCheck
33:         {
34:             Free,                // số chưa có ai dùng: tạo hồ sơ mới
35:             LinkCounterProfile,  // số thuộc hồ sơ tại quầy (chưa có tài khoản) và đúng họ tên: hỏi gắn vào hồ sơ đó
36:             AlreadyRegistered    // số đã thuộc về người khác (có tài khoản, hoặc hồ sơ quầy khác họ tên): không cho đăng ký
37:         }
38: 
39:         public static (PhoneCheck Result, Member? Profile) CheckPhoneForRegistration(SportsCenterContext db, string fullName, string phone)
40:         {
41:             if (FindByPhone(db, phone) == null) return (PhoneCheck.Free, null);
42:             var counter = FindCounterProfiles(db, fullName, phone);
43:             if (counter.Count > 0) return (PhoneCheck.LinkCounterProfile, counter[0]);
44:             return (PhoneCheck.AlreadyRegistered, null);
45:         }
46: 
47:         // Email đơn giản: có @ và có dấu chấm ở phần sau, không có khoảng trắng
48:         public static bool IsValidEmail(string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
49: 
50:         // Tên đăng nhập: 3-30 ký tự, chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới (không có khoảng trắng). Trả về lỗi hoặc null.
51:         public static string? UsernameError(string username)
52:         {
53:             if (username.Length < 3 || username.Length > 30) return "Tên đăng nhập gồm 3-30 ký tự.";
54:             if (!Regex.IsMatch(username, @"^[A-Za-z0-9._]+$"))
55:                 return "Tên đăng nhập chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới (không có khoảng trắng).";
56:             return null;
57:         }
58: 
59:         // Họ tên: 2-100 ký tự, chỉ gồm chữ cái (có dấu), khoảng trắng, dấu chấm, dấu nháy hoặc gạch nối; không có số. Trả về lỗi hoặc null.
60:         public static string? FullNameError(string fullName)
61:         {
62:             string n = fullName.Trim().Normalize(NormalizationForm.FormC);
63:             if (n.Length < 2 || n.Length > 100) return "Họ tên gồm 2-100 ký tự.";
64:             if (!Regex.IsMatch(n, @"^\p{L}[\p{L}\p{M} .'’-]*$"))
65:                 return "Họ tên chỉ gồm chữ cái và khoảng trắng, không có số hoặc ký tự đặc biệt.";
66:             return null;
67:         }
68: 
69:         // Hồ sơ đăng ký tại quầy (chưa có tài khoản) khớp CẢ số điện thoại LẪN họ tên.
70:         // Chỉ biết số điện thoại thì không đủ để tìm ra hồ sơ.
71:         public static List<Member> FindCounterProfiles(SportsCenterContext db, string fullName, string phone)
72:         {
73:             string name = Normalize(fullName);
74:             return db.Members
75:                 .Where(m => m.UserId == null && m.Phone == phone)
76:                 .ToList()
77:                 .Where(m => Normalize(m.FullName) == name)
78:                 .ToList();
79:         }
80: 
81:         // Tạo tài khoản Member. counterProfile != null: gắn tài khoản vào hồ sơ quầy đó (giữ nguyên gói, hóa đơn, lớp đã đăng ký);
82:         // counterProfile == null: tạo hồ sơ Member mới. Cả hai trường hợp lưu trong một lần SaveChanges.
83:         public static User CreateMemberAccount(SportsCenterContext db, string username, string password,
84:             string fullName, string phone, Member? counterProfile)
85:         {
86:             if (counterProfile != null && counterProfile.UserId != null)
87:             {
88:                 throw new InvalidOperationException("Hồ sơ này đã có tài khoản.");
89:             }
90: 
91:             var user = new User
92:             {
93:                 Username = username,
94:                 PasswordHash = PasswordHelper.Hash(password),
95:                 FullName = fullName,
96:                 Phone = phone,
97:                 Role = "Member",
98:                 IsActive = true,
99:                 CreatedAt = DateTime.Now
100:             };
101: 
102:             if (counterProfile == null)
103:             {
104:                 db.Members.Add(new Member { User = user, FullName = fullName, Phone = phone, CreatedAt = DateTime.Now });
105:             }
106:             else
107:             {
108:                 if (db.Entry(counterProfile).State == EntityState.Detached)
109:                     db.Members.Attach(counterProfile);
110:                 db.Users.Add(user);
111:                 counterProfile.User = user;
112:             }
113: 
114:             db.SaveChanges();
115:             return user;
116:         }
117:     }
118: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 14-24 (`Normalize`)**: Thuật toán chuẩn hóa chuỗi tiếng Việt:
  - Dòng 16: Cắt khoảng trắng 2 đầu (`Trim`), chuyển về chữ thường không phân biệt văn hóa (`ToLowerInvariant`), thay thế chữ `đ` thành `d`.
  - Dòng 18-22: Tách chuỗi theo chuẩn `FormD` (phân tách ký tự gốc và dấu thanh âm), chỉ thêm vào `StringBuilder` các ký tự không phải dấu (`NonSpacingMark`).
  - Dòng 23: Tách chuỗi theo dấu cách và gộp lại bằng 1 khoảng trắng duy nhất. Kết quả biến `"  Nguyễn   Văn Đạt "` thành `"nguyen van dat"`.
- **Dòng 28-29 (`FindByPhone`)**: Tìm kiếm hội viên đầu tiên sở hữu số điện thoại `phone` trong bảng `Members` với chế độ `AsNoTracking()`.
- **Dòng 32-37 (`PhoneCheck`)**: Định nghĩa Enum 3 trạng thái kiểm tra SĐT khi tự đăng ký tài khoản:
  - `Free`: Số điện thoại chưa ai dùng.
  - `LinkCounterProfile`: Trùng số điện thoại và trùng họ tên với hồ sơ quầy chưa có tài khoản.
  - `AlreadyRegistered`: Số điện thoại đã được đăng ký bởi tài khoản khác, hoặc trùng SĐT quầy nhưng khác họ tên.
- **Dòng 39-45 (`CheckPhoneForRegistration`)**: Thực hiện kiểm tra theo thứ tự ưu tiên:
  - Dòng 41: Nếu `FindByPhone == null` $\rightarrow$ trả về `PhoneCheck.Free`.
  - Dòng 42-43: Nếu tìm thấy hồ sơ quầy khớp tên qua `FindCounterProfiles` $\rightarrow$ trả về `PhoneCheck.LinkCounterProfile`.
  - Dòng 44: Ngược lại $\rightarrow$ trả về `PhoneCheck.AlreadyRegistered`.
- **Dòng 48 (`IsValidEmail`)**: Dùng biểu thức chính quy Regex kiểm tra cú pháp email cơ bản.
- **Dòng 51-57 (`UsernameError`)**: Kiểm tra độ dài 3-30 ký tự và Regex `^[A-Za-z0-9._]+$` (chỉ chữ không dấu, số, chấm, gạch dưới, không có khoảng trắng). Trả về chuỗi thông báo lỗi hoặc `null` nếu hợp lệ.
- **Dòng 60-67 (`FullNameError`)**: Kiểm tra độ dài 2-100 ký tự và Regex `^\p{L}[\p{L}\p{M} .'’-]*$` hỗ trợ trọn vẹn Unicode tiếng Việt có dấu, tuyệt đối cấm chứa số và ký tự lạ.
- **Dòng 71-79 (`FindCounterProfiles`)**: Lấy danh sách hồ sơ quầy (`UserId == null` và `Phone == phone`), sau đó lọc tiếp bằng hàm `Normalize(FullName) == name`.
- **Dòng 83-116 (`CreateMemberAccount`)**:
  - Dòng 86-89: Nếu hồ sơ quầy đã có `UserId != null` thì ném ngoại lệ chặn lại.
  - Dòng 91-100: Khởi tạo thực thể `User` mới (role Member, mật khẩu được băm bằng `PasswordHelper.Hash`).
  - Dòng 102-105: Nếu `counterProfile == null` (khách mới hoàn toàn): tạo mới bản ghi `Member` gắn với `User`.
  - Dòng 106-112: Nếu `counterProfile != null` (gắn hồ sơ quầy): gắn hồ sơ quầy vào Change Tracker (`Attach`), thêm `user` vào `db.Users`, và gán liên kết `counterProfile.User = user`.
  - Dòng 114: Gọi `db.SaveChanges()` một lần duy nhất, đảm bảo tính nguyên tử (Atomic Transaction) cho cả hai bảng.

---

# 5. FILE: `LoginWindow.xaml.cs` (Code-behind Đăng nhập)

### Mã nguồn:
```csharp
1: using SportsCenter.Entities;
2: using System.Windows;
3: 
4: namespace SportsCenter
5: {
6:     public partial class LoginWindow : Window
7:     {
8:         private SportsCenterContext _db;
9: 
10:         public LoginWindow()
11:         {
12:             InitializeComponent();
13:             _db = new();
14:             txtUsername.Focus();
15:         }
16: 
17:         private void btnLogin_Click(object sender, RoutedEventArgs e)
18:         {
19:             string username = txtUsername.Text.Trim();
20:             string password = txtPassword.Password;
21: 
22:             if (username == "" || password == "")
23:             {
24:                 Msg.Warn("Vui lòng nhập tên đăng nhập và mật khẩu.");
25:                 return;
26:             }
27: 
28:             try
29:             {
30:                 var user = _db.Users.FirstOrDefault(u => u.Username == username);
31:                 if (user == null || user.PasswordHash != PasswordHelper.Hash(password))
32:                 {
33:                     Msg.Warn("Sai tên đăng nhập hoặc mật khẩu.");
34:                     return;
35:                 }
36:                 if (!user.IsActive)
37:                 {
38:                     Msg.Warn("Tài khoản đã bị khóa.");
39:                     return;
40:                 }
41: 
42:                 Session.SignIn(_db, user);
43:                 new MainWindow().Show();
44:                 Close();
45:             }
46:             catch (Exception ex)
47:             {
48:                 MessageBox.Show("Không kết nối được cơ sở dữ liệu. Hãy kiểm tra appsettings.json và đã chạy SportsCenter.sql chưa.\n\n"
49:                     + (ex.InnerException?.Message ?? ex.Message), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
50:             }
51:         }
52: 
53:         private void btnRegister_Click(object sender, RoutedEventArgs e)
54:         {
55:             new RegisterWindow().Show();
56:             Close();
57:         }
58:     }
59: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 6-8**: Định nghĩa lớp `LoginWindow` kế thừa từ `Window`. Khai báo biến `_db` nắm giữ DbContext riêng cho màn hình đăng nhập.
- **Dòng 10-15**: Hàm khởi tạo `LoginWindow()`:
  - Dòng 12: `InitializeComponent()` tải giao diện XAML.
  - Dòng 13: `_db = new()` khởi tạo kết nối DbContext.
  - Dòng 14: `txtUsername.Focus()` tự động đưa con trỏ chuột vào ô Tên đăng nhập.
- **Dòng 17-51 (`btnLogin_Click`) – Xử lý Đăng nhập (Luật A1)**:
  - Dòng 19-20: Đọc chuỗi `username` (cắt khoảng trắng thừa) và mật khẩu `password`.
  - Dòng 22-26: Kiểm tra nếu một trong hai trường trống thì báo `Msg.Warn` và dừng lại (`return`).
  - Dòng 30: Truy vấn bảng `Users` theo `Username`.
  - Dòng 31-35: So sánh mã băm `PasswordHelper.Hash(password)` với `user.PasswordHash`. Nếu không tìm thấy user hoặc sai mật khẩu, hiển thị câu cảnh báo chung `"Sai tên đăng nhập hoặc mật khẩu."` để tránh lộ thông tin tài khoản tồn tại.
  - Dòng 36-40: Kiểm tra nếu `!user.IsActive` (tài khoản đã bị quản lý khóa) thì báo `"Tài khoản đã bị khóa."` và dừng lại.
  - Dòng 42: Xác thực thành công, gọi `Session.SignIn(_db, user)` để nạp thông tin phiên và ghi log kiểm toán.
  - Dòng 43-44: Mở cửa sổ chính `new MainWindow().Show()` và đóng màn hình đăng nhập `Close()`.
  - Dòng 46-50: Khối `catch` bắt lỗi mất kết nối SQL Server, nhắc nhở kiểm tra chuỗi kết nối trong `appsettings.json`.
- **Dòng 53-57 (`btnRegister_Click`)**: Chuyển sang màn hình Đăng ký `new RegisterWindow().Show()` và đóng Login.

---

# 6. FILE: `RegisterWindow.xaml.cs` (Code-behind Tự Đăng ký & Gắn Hồ Sơ)

### Mã nguồn:
```csharp
1: using SportsCenter.Entities;
2: using System.Text.RegularExpressions;
3: using System.Windows;
4: using System.Windows.Controls;
5: 
6: namespace SportsCenter
7: {
8:     public partial class RegisterWindow : Window
9:     {
10:         private SportsCenterContext _db;
11: 
12:         public RegisterWindow()
13:         {
14:             InitializeComponent();
15:             _db = new();
16:             txtFullName.Focus();
17:         }
18: 
19:         private void ClearErrors()
20:         {
21:             errFullName.Text = errPhone.Text = errUsername.Text = errPassword.Text = errConfirm.Text = "";
22:         }
23: 
24:         private void btnCreate_Click(object sender, RoutedEventArgs e)
25:         {
26:             ClearErrors();
27:             string fullName = txtFullName.Text.Trim();
28:             string phone = txtPhone.Text.Trim();
29:             string username = txtUsername.Text.Trim();
30:             string password = txtPassword.Password;
31:             string confirm = txtConfirm.Password;
32:             bool ok = true;
33:             var phoneCheck = (Result: AccountHelper.PhoneCheck.Free, Profile: (Member?)null);
34: 
35:             if (fullName == "")
36:             {
37:                 errFullName.Text = "Vui lòng nhập họ tên.";
38:                 ok = false;
39:             }
40:             else if (AccountHelper.FullNameError(fullName) is string nameError)
41:             {
42:                 errFullName.Text = nameError;
43:                 ok = false;
44:             }
45: 
46:             if (phone == "")
47:             {
48:                 errPhone.Text = "Vui lòng nhập số điện thoại.";
49:                 ok = false;
50:             }
51:             else if (!Regex.IsMatch(phone, @"^0\d{9,10}$"))
52:             {
53:                 errPhone.Text = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0.";
54:                 ok = false;
55:             }
56:             else
57:             {
58:                 // Mỗi số điện thoại chỉ thuộc về một member
59:                 phoneCheck = AccountHelper.CheckPhoneForRegistration(_db, fullName, phone);
60:                 if (phoneCheck.Result == AccountHelper.PhoneCheck.AlreadyRegistered)
61:                 {
62:                     errPhone.Text = "Số điện thoại này đã được đăng ký. Hãy đăng nhập nếu đây là số của bạn, hoặc nhờ lễ tân kiểm tra.";
63:                     ok = false;
64:                 }
65:             }
66: 
67:             if (username == "")
68:             {
69:                 errUsername.Text = "Vui lòng nhập tên đăng nhập.";
70:                 ok = false;
71:             }
72:             else if (AccountHelper.UsernameError(username) is string usernameError)
73:             {
74:                 errUsername.Text = usernameError;
75:                 ok = false;
76:             }
77:             else if (_db.Users.Any(u => u.Username == username))
78:             {
79:                 errUsername.Text = "Tên đăng nhập này đã có người dùng.";
80:                 ok = false;
81:             }
82: 
83:             if (password == "")
84:             {
85:                 errPassword.Text = "Vui lòng nhập mật khẩu.";
86:                 ok = false;
87:             }
88:             else if (password.Length < 6)
89:             {
90:                 errPassword.Text = "Mật khẩu cần ít nhất 6 ký tự.";
91:                 ok = false;
92:             }
93:             else if (password.ToLower() == username.ToLower())
94:             {
95:                 errPassword.Text = "Mật khẩu không được trùng tên đăng nhập.";
96:                 ok = false;
97:             }
98: 
99:             if (confirm == "")
100:             {
101:                 errConfirm.Text = "Vui lòng nhập lại mật khẩu.";
102:                 ok = false;
103:             }
104:             else if (confirm != password)
105:             {
106:                 errConfirm.Text = "Hai mật khẩu chưa khớp.";
107:                 ok = false;
108:             }
109: 
110:             if (!ok) return;
111: 
112:             try
113:             {
114:                 // Số điện thoại thuộc một hồ sơ đăng ký tại quầy (chưa có tài khoản) đúng họ tên: phải gắn vào hồ sơ đó,
115:                 // vì mỗi số điện thoại chỉ có một hồ sơ (không tạo hồ sơ thứ hai).
116:                 Member? link = null;
117:                 if (phoneCheck.Result == AccountHelper.PhoneCheck.LinkCounterProfile)
118:                 {
119:                     var m = phoneCheck.Profile!;
120:                     if (!Msg.Confirm($"Đã có hồ sơ tại quầy của {m.FullName} (SĐT {m.Phone}, tạo ngày {m.CreatedAt:dd/MM/yyyy}).\n\n"
121:                         + "Gắn tài khoản mới vào hồ sơ đó để xem lại gói, hóa đơn và lớp đã đăng ký?"))
122:                     {
123:                         errPhone.Text = "Số điện thoại này đã có hồ sơ tại quầy. Hãy gắn tài khoản vào hồ sơ đó, hoặc dùng số điện thoại khác.";
124:                         return;
125:                     }
126:                     link = m;
127:                 }
128: 
129:                 // Tạo User (role Member); gắn vào hồ sơ quầy nếu có, không thì tạo hồ sơ Member mới
130:                 var user = AccountHelper.CreateMemberAccount(_db, username, password, fullName, phone, link);
131:                 Session.SignIn(_db, user);
132:                 if (link != null)
133:                 {
134:                     AuditHelper.Log(_db, "LinkMember",
135:                         $"Tài khoản {username} gắn vào hồ sơ quầy {link.FullName} (#{link.MemberId})", "Members", link.MemberId);
136:                 }
137: 
138:                 string note = link != null ? " Tài khoản đã được gắn vào hồ sơ tại quầy của bạn." : "";
139:                 Msg.Info("Tạo tài khoản thành công." + note);
140:                 new MainWindow().Show();
141:                 Close();
142:             }
143:             catch (Exception ex)
144:             {
145:                 Msg.Error(ex);
146:             }
147:         }
148: 
149:         private void btnBack_Click(object sender, RoutedEventArgs e)
150:         {
151:             new LoginWindow().Show();
152:             Close();
153:         }
154:     }
155: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 19-22 (`ClearErrors`)**: Xóa sạch toàn bộ văn bản của 5 TextBlock hiển thị lỗi màu đỏ dưới từng ô nhập liệu.
- **Dòng 24-147 (`btnCreate_Click`)**:
  - Dòng 26-33: Thu thập dữ liệu từ các ô nhập. Khai báo biến cờ `ok = true` và biến tuple `phoneCheck`.
  - Dòng 35-44 (Kiểm tra Họ tên): Không được rỗng; gọi `AccountHelper.FullNameError` kiểm tra cấm chứa chữ số hoặc ký tự lạ. Nếu lỗi, gán nội dung vào `errFullName.Text` và đặt `ok = false`.
  - Dòng 46-65 (Kiểm tra Số điện thoại - Luật A5): Không được rỗng; kiểm tra Regex `^0\d{9,10}$` (phải có 10-11 số và bắt đầu bằng số 0); gọi `AccountHelper.CheckPhoneForRegistration`. Nếu trả về `AlreadyRegistered`, gán lỗi đỏ vào `errPhone.Text` và đặt `ok = false`.
  - Dòng 67-81 (Kiểm tra Tên đăng nhập): Không được rỗng; gọi `AccountHelper.UsernameError` kiểm tra định dạng; kiểm tra chống trùng trong DB `_db.Users.Any(u => u.Username == username)`.
  - Dòng 83-97 (Kiểm tra Mật khẩu): Không được rỗng; độ dài $\ge 6$ ký tự; cấm trùng với username.
  - Dòng 99-108 (Kiểm tra Xác nhận mật khẩu): Hai ô mật khẩu bắt buộc phải khớp nhau (`confirm == password`).
  - Dòng 110: Nếu `!ok` (có ít nhất một lỗi), dừng hàm để người dùng điều chỉnh lại trên giao diện.
  - Dòng 117-127 (Luồng gắn hồ sơ quầy - Luật A2): Nếu kết quả kiểm tra SĐT là `LinkCounterProfile`, bật hộp thoại `Msg.Confirm` hỏi hội viên có muốn gắn tài khoản mới vào hồ sơ quầy cũ hay không. Nếu người dùng chọn "Không", hiển thị thông báo lỗi yêu cầu dùng số điện thoại khác và dừng lại. Nếu chọn "Có", gán `link = m`.
  - Dòng 130: Gọi `AccountHelper.CreateMemberAccount` để tạo `User` và tạo/gắn `Member` trong CSDL.
  - Dòng 131: Đăng nhập ngay lập tức cho người dùng bằng `Session.SignIn(_db, user)`.
  - Dòng 132-136: Nếu có gắn hồ sơ quầy, ghi log kiểm toán hành động `"LinkMember"`.
  - Dòng 138-141: Báo thông báo thành công, mở `MainWindow` và đóng form đăng ký.
- **Dòng 149-153 (`btnBack_Click`)**: Nút quay lại màn hình Đăng nhập.

---

# 7. FILE: `MainWindow.xaml.cs` (Khung Điều Hướng Ứng Dụng)

### Mã nguồn:
```csharp
1: using System.Windows;
2: using System.Windows.Controls;
3: 
4: namespace SportsCenter
5: {
6:     public partial class MainWindow : Window
7:     {
8:         // Các trang khác (vd danh sách member) gọi MainWindow.Instance.ShowPage(...) để chuyển màn hình
9:         public static MainWindow? Instance { get; private set; }
10: 
11:         private readonly Dictionary<Type, Button> _buttons = new();
12:         private readonly Dictionary<Type, Expander> _groupOf = new();
13:         private readonly List<Expander> _groups = new();
14:         private Type? _firstPage;
15: 
16:         public MainWindow()
17:         {
18:             InitializeComponent();
19:             Instance = this;
20: 
21:             var user = Session.CurrentUser!;
22:             txtUserName.Text = user.FullName;
23:             txtUserLogin.Text = user.Username;
24:             txtRole.Text = Rules.RoleNames[user.Role];
25: 
26:             BuildMenu(user.Role);
27:             ShowPage((UserControl)Activator.CreateInstance(_firstPage!)!);
28:         }
29: 
30:         // Menu theo role: mục đơn là nút thường, nhóm bấm để xổ xuống (mở nhóm này thì thu nhóm khác)
31:         private void BuildMenu(string role)
32:         {
33:             switch (role)
34:             {
35:                 case "Manager":
36:                     AddItem("Báo cáo", typeof(DashboardPage));
37:                     AddGroup("Quản lý lớp", ("Lớp học", typeof(ClassesPage)), ("Bộ môn, phòng", typeof(SportsRoomsPage)));
38:                     AddGroup("Thành viên và gói", ("Member", typeof(MembersPage)), ("Gói thành viên", typeof(PackagesPage)));
39:                     AddItem("Thanh toán", typeof(PaymentsPage));
40:                     AddGroup("Hệ thống", ("Tài khoản", typeof(UsersPage)), ("Lịch sử hoạt động", typeof(AuditPage)));
41:                     break;
42: 
43:                 case "Receptionist":
44:                     AddGroup("Thành viên", ("Member", typeof(MembersPage)), ("Mua / gia hạn gói", typeof(BuyPage)));
45:                     AddItem("Đặt lớp", typeof(BookingPage));
46:                     AddItem("Thanh toán", typeof(PaymentsPage));
47:                     break;
48: 
49:                 case "Member":
50:                     AddItem("Lịch của tôi", typeof(MySchedulePage));
51:                     AddItem("Đặt lớp", typeof(BookingPage));
52:                     AddItem("Gói thành viên", typeof(BuyPage));
53:                     AddItem("Thanh toán", typeof(PaymentsPage));
54:                     break;
55: 
56:                 case "Coach":
57:                     AddItem("Lớp của tôi", typeof(CoachPage));
58:                     AddItem("Điểm danh", typeof(AttendancePage));
59:                     break;
60:             }
61:         }
62: 
63:         private Button MakeButton(string title, Type pageType)
64:         {
65:             var btn = new Button { Content = title, Style = (Style)FindResource("MenuButton") };
66:             btn.Click += (s, e) => ShowPage((UserControl)Activator.CreateInstance(pageType)!);
67:             _buttons[pageType] = btn;
68:             _firstPage ??= pageType;
69:             return btn;
70:         }
71: 
72:         private void AddItem(string title, Type pageType) => panelMenu.Children.Add(MakeButton(title, pageType));
73: 
74:         private void AddGroup(string title, params (string Title, Type PageType)[] items)
75:         {
76:             var inner = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
77:             var group = new Expander { Header = title, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 2, 0, 2), Content = inner };
78: 
79:             foreach (var item in items)
80:             {
81:                 inner.Children.Add(MakeButton(item.Title, item.PageType));
82:                 _groupOf[item.PageType] = group;
83:             }
84: 
85:             group.Expanded += (s, e) =>
86:             {
87:                 foreach (var g in _groups)
88:                 {
89:                     if (g != group) g.IsExpanded = false;
90:                 }
91:             };
92:             _groups.Add(group);
93:             panelMenu.Children.Add(group);
94:         }
95: 
96:         // Hiện một trang trong vùng nội dung và tô sáng mục menu tương ứng
97:         public void ShowPage(UserControl page)
98:         {
99:             contentHost.Content = page;
100:             Type type = page.GetType();
101: 
102:             foreach (var kv in _buttons)
103:             {
104:                 kv.Value.Tag = kv.Key == type ? "on" : null;
105:             }
106:             if (_groupOf.TryGetValue(type, out var group))
107:             {
108:                 group.IsExpanded = true;
109:             }
110:         }
111: 
112:         // Đổi mật khẩu là hộp thoại nhỏ, không phải một màn hình trong menu
113:         private void btnChangePassword_Click(object sender, RoutedEventArgs e)
114:         {
115:             new ChangePasswordWindow { Owner = this }.ShowDialog();
116:         }
117: 
118:         private void btnLogout_Click(object sender, RoutedEventArgs e)
119:         {
120:             Session.Clear();
121:             new LoginWindow().Show();
122:             Close();
123:         }
124:     }
125: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 9**: `public static MainWindow? Instance { get; private set; }` – Thể hiện Singleton của `MainWindow` giúp mọi trang con có thể gọi chuyển trang `MainWindow.Instance.ShowPage(...)`.
- **Dòng 11-14**:
  - `_buttons`: Ánh xạ kiểu trang (`Type`) sang nút menu tương ứng để tô sáng khi được chọn.
  - `_groupOf`: Ánh xạ kiểu trang sang điều khiển `Expander` chứa nó để tự động mở nhóm khi trang hiển thị.
  - `_groups`: Danh sách toàn bộ các nhóm `Expander` để thực hiện hiệu ứng Accordion.
  - `_firstPage`: Kiểu trang đầu tiên được nạp làm trang chủ mặc định.
- **Dòng 16-28**: Hàm khởi tạo `MainWindow()`:
  - Dòng 19: Gán `Instance = this`.
  - Dòng 21-24: Đọc thông tin người dùng từ `Session.CurrentUser` đưa lên giao diện góc trái: Họ tên, Tên đăng nhập, Tên vai trò tiếng Việt (`Rules.RoleNames[user.Role]`).
  - Dòng 26: Gọi `BuildMenu(user.Role)` để xây dựng thực đơn.
  - Dòng 27: Khởi tạo và nạp trang đầu tiên `_firstPage` vào `contentHost`.
- **Dòng 31-61 (`BuildMenu`)**: Xây dựng menu động theo vai trò:
  - `Manager`: Dựng 3 nhóm xổ xuống (`Quản lý lớp`, `Thành viên và gói`, `Hệ thống`) và 2 mục đơn (`Báo cáo`, `Thanh toán`).
  - `Receptionist`: Dựng nhóm `Thành viên` và các nút chức năng tại quầy (`Đặt lớp`, `Thanh toán`).
  - `Member`: Dựng 4 mục tự phục vụ: `Lịch của tôi`, `Đặt lớp`, `Gói thành viên`, `Thanh toán`.
  - `Coach`: Dựng 2 mục chuyên môn: `Lớp của tôi`, `Điểm danh`.
- **Dòng 63-70 (`MakeButton`)**: Tạo nút bấm menu với Style `MenuButton`. Khi click, gọi `ShowPage` khởi tạo trang mới.
- **Dòng 72 (`AddItem`)**: Thêm một mục đơn vào thanh menu bên trái `panelMenu`.
- **Dòng 74-94 (`AddGroup`)**: Tạo nhóm chức năng xổ xuống dạng `Expander`:
  - Dòng 85-91: **Hiệu ứng Accordion**: Bắt sự kiện `group.Expanded`, khi người dùng bấm mở một nhóm, duyệt qua toàn bộ các nhóm khác trong `_groups` và đặt `IsExpanded = false` (tự động thu lại nhóm đang mở trước đó).
- **Dòng 97-110 (`ShowPage`)**: Đưa UserControl trang con vào `contentHost.Content`. Duyệt `_buttons` để gán `Tag = "on"` tô sáng nút tương ứng. Nếu trang nằm trong một Expander, tự động mở Expander đó ra (`group.IsExpanded = true`).
- **Dòng 113-116 (`btnChangePassword_Click`)**: Mở hộp thoại `ChangePasswordWindow` dưới dạng modal dialog (`ShowDialog()`).
- **Dòng 118-123 (`btnLogout_Click`)**: Gọi `Session.Clear()`, hiển thị lại `LoginWindow` và đóng cửa sổ chính.

---

# 8. FILE: `ChangePasswordWindow.xaml.cs` (Code-behind Đổi Mật Khẩu)

### Mã nguồn:
```csharp
1: using SportsCenter.Entities;
2: using System.Windows;
3: 
4: namespace SportsCenter
5: {
6:     public partial class ChangePasswordWindow : Window
7:     {
8:         private SportsCenterContext _db;
9: 
10:         public ChangePasswordWindow()
11:         {
12:             InitializeComponent();
13:             _db = new();
14: 
15:             var u = Session.CurrentUser!;
16:             txtInfo.Text = $"{u.FullName} · {u.Username} · {Rules.RoleNames[u.Role]}";
17:             Loaded += (s, e) => txtOld.Focus();
18:         }
19: 
20:         private void btnChange_Click(object sender, RoutedEventArgs e)
21:         {
22:             errOld.Text = errNew.Text = errConfirm.Text = "";
23:             string oldPw = txtOld.Password;
24:             string newPw = txtNew.Password;
25:             string confirm = txtConfirm.Password;
26: 
27:             var user = _db.Users.Find(Session.CurrentUser!.UserId)!;
28:             bool ok = true;
29: 
30:             if (oldPw == "")
31:             {
32:                 errOld.Text = "Vui lòng nhập mật khẩu hiện tại.";
33:                 ok = false;
34:             }
35:             else if (user.PasswordHash != PasswordHelper.Hash(oldPw))
36:             {
37:                 errOld.Text = "Mật khẩu hiện tại chưa đúng.";
38:                 ok = false;
39:             }
40: 
41:             if (newPw == "")
42:             {
43:                 errNew.Text = "Vui lòng nhập mật khẩu mới.";
44:                 ok = false;
45:             }
46:             else if (newPw.Length < 6)
47:             {
48:                 errNew.Text = "Mật khẩu cần ít nhất 6 ký tự.";
49:                 ok = false;
50:             }
51:             else if (newPw == oldPw)
52:             {
53:                 errNew.Text = "Mật khẩu mới phải khác mật khẩu hiện tại.";
54:                 ok = false;
55:             }
56:             else if (newPw.ToLower() == user.Username.ToLower())
57:             {
58:                 errNew.Text = "Mật khẩu không được trùng tên đăng nhập.";
59:                 ok = false;
60:             }
61: 
62:             if (confirm == "")
63:             {
64:                 errConfirm.Text = "Vui lòng nhập lại mật khẩu mới.";
65:                 ok = false;
66:             }
67:             else if (confirm != newPw)
68:             {
69:                 errConfirm.Text = "Hai mật khẩu chưa khớp.";
70:                 ok = false;
71:             }
72: 
73:             if (!ok) return;
74: 
75:             try
76:             {
77:                 user.PasswordHash = PasswordHelper.Hash(newPw);
78:                 _db.SaveChanges();
79:                 AuditHelper.Log(_db, "ChangePassword", $"{user.Username} đổi mật khẩu", "Users", user.UserId);
80: 
81:                 Msg.Info("Đã đổi mật khẩu. Lần đăng nhập sau dùng mật khẩu mới.");
82:                 Close();
83:             }
84:             catch (Exception ex)
85:             {
86:                 Msg.Error(ex);
87:             }
88:         }
89:     }
90: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 10-18**: Khởi tạo hộp thoại `ChangePasswordWindow`:
  - Dòng 15-16: Hiển thị tóm tắt thông tin người dùng đang đổi mật khẩu: `Họ tên · Username · Tên vai trò tiếng Việt`.
  - Dòng 17: Tự động đưa con trỏ chuột vào ô Mật khẩu hiện tại (`txtOld.Focus()`).
- **Dòng 20-88 (`btnChange_Click`)**:
  - Dòng 22: Xóa các thông báo lỗi cũ `errOld`, `errNew`, `errConfirm`.
  - Dòng 23-25: Đọc mật khẩu cũ `oldPw`, mật khẩu mới `newPw`, và xác nhận `confirm`.
  - Dòng 27: Tìm bản ghi `user` từ CSDL thông qua `_db.Users.Find(...)`.
  - Dòng 30-39 (Kiểm tra mật khẩu cũ): Không được để trống; kiểm tra mã băm `user.PasswordHash != PasswordHelper.Hash(oldPw)`. Nếu sai, gán `"Mật khẩu hiện tại chưa đúng."` vào `errOld.Text`.
  - Dòng 41-60 (Kiểm tra mật khẩu mới): Không được để trống; độ dài tối thiểu 6 ký tự; mật khẩu mới phải khác mật khẩu hiện tại (`newPw != oldPw`); cấm mật khẩu mới trùng với username.
  - Dòng 62-71 (Kiểm tra xác nhận mật khẩu): Hai ô bắt buộc phải khớp nhau (`confirm == newPw`).
  - Dòng 73: Nếu có bất kỳ lỗi nào (`!ok`), dừng xử lý.
  - Dòng 77: Băm mật khẩu mới: `user.PasswordHash = PasswordHelper.Hash(newPw)`.
  - Dòng 78: Lưu thay đổi vào SQL Server: `_db.SaveChanges()`.
  - Dòng 79: Ghi nhận nhật ký kiểm toán hành động `"ChangePassword"`.
  - Dòng 81-82: Thông báo thành công và đóng hộp thoại.

---

# 9. FILE: `Pages/UsersPage.xaml.cs` (Code-behind Quản lý Tài khoản - Manager)

### Mã nguồn:
```csharp
1: using SportsCenter.Entities;
2: using System.Windows;
3: using System.Windows.Controls;
4: 
5: namespace SportsCenter
6: {
7:     public partial class UsersPage : UserControl
8:     {
9:         private SportsCenterContext _db;
10:         // Mã role lưu trong DB; các ô chọn hiện tên tiếng Việt theo đúng thứ tự này
11:         private readonly string[] _roles = { "Manager", "Receptionist", "Coach", "Member" };
12: 
13:         private static string RoleName(string code) => Rules.RoleNames.GetValueOrDefault(code, code);
14: 
15:         public UsersPage()
16:         {
17:             InitializeComponent();
18:             _db = new();
19:             GridHelper.Setup(dgUsers);
20:             pgUsers.PageChanged += (s, e) => LoadUsers();
21:             LoadData();
22:         }
23: 
24:         private void LoadData()
25:         {
26:             var roleNames = _roles.Select(RoleName).ToList();
27: 
28:             var filterRoles = new List<string> { "Tất cả" };
29:             filterRoles.AddRange(roleNames);
30:             cbxRoleFilter.ItemsSource = filterRoles;
31:             cbxRoleFilter.SelectedIndex = 0;
32: 
33:             cbxNewRole.ItemsSource = roleNames;
34:             cbxNewRole.SelectedIndex = 3;
35:             cbxRole.ItemsSource = roleNames;
36:             cbxRole.SelectedIndex = 1;
37: 
38:             LoadUsers();
39:         }
40: 
41:         private void LoadUsers()
42:         {
43:             var query = _db.Users.AsQueryable();
44: 
45:             string keyword = txtSearch.Text.Trim();
46:             if (keyword != "")
47:             {
48:                 query = query.Where(u => u.Username.Contains(keyword) || u.FullName.Contains(keyword));
49:             }
50:             if (cbxRoleFilter.SelectedIndex > 0)
51:             {
52:                 string role = _roles[cbxRoleFilter.SelectedIndex - 1];
53:                 query = query.Where(u => u.Role == role);
54:             }
55: 
56:             var rows = query
57:                 .OrderBy(u => u.UserId)
58:                 .Select(u => new
59:                 {
60:                     u.UserId,
61:                     u.Username,
62:                     u.FullName,
63:                     u.Role,
64:                     Status = u.IsActive ? "Đang hoạt động" : "Đã khóa"
65:                 })
66:                 .ToList()
67:                 .Select(u => new { u.UserId, u.Username, u.FullName, Role = RoleName(u.Role), u.Status })
68:                 .ToList();
69:             dgUsers.ItemsSource = pgUsers.Slice(rows);
70:         }
71: 
72:         private void btnFilter_Click(object sender, RoutedEventArgs e)
73:         {
74:             pgUsers.Reset();
75:             LoadUsers();
76:         }
77: 
78:         private void btnClearFilter_Click(object sender, RoutedEventArgs e)
79:         {
80:             txtSearch.Clear();
81:             cbxRoleFilter.SelectedIndex = 0;
82:             pgUsers.Reset();
83:             LoadUsers();
84:         }
85: 
86:         // Lấy tài khoản đang chọn; không cho thao tác trên chính tài khoản đang đăng nhập
87:         private User? GetSelectedUser()
88:         {
89:             int? id = GridHelper.SelectedId(dgUsers, "UserId");
90:             if (id == null)
91:             {
92:                 Msg.Warn("Vui lòng chọn một tài khoản trong danh sách.");
93:                 return null;
94:             }
95:             if (id == Session.CurrentUser!.UserId)
96:             {
97:                 Msg.Warn("Không thể khóa hoặc đổi vai trò của chính tài khoản đang đăng nhập.");
98:                 return null;
99:             }
100:             return _db.Users.Find(id);
101:         }
102: 
103:         private void btnToggle_Click(object sender, RoutedEventArgs e)
104:         {
105:             var user = GetSelectedUser();
106:             if (user == null) return;
107: 
108:             try
109:             {
110:                 user.IsActive = !user.IsActive;
111:                 _db.SaveChanges();
112:                 AuditHelper.Log(_db, "ToggleUser", $"{(user.IsActive ? "Mở khóa" : "Khóa")} tài khoản {user.Username}", "Users", user.UserId);
113:                 LoadUsers();
114:                 GridHelper.SelectById(dgUsers, "UserId", user.UserId);
115:             }
116:             catch (Exception ex)
117:             {
118:                 Msg.Error(ex);
119:             }
120:         }
121: 
122:         private void btnChangeRole_Click(object sender, RoutedEventArgs e)
123:         {
124:             var user = GetSelectedUser();
125:             if (user == null) return;
126: 
127:             string newRole = _roles[cbxNewRole.SelectedIndex];
128:             if (newRole == user.Role)
129:             {
130:                 Msg.Warn("Tài khoản này đã có vai trò " + RoleName(newRole) + ".");
131:                 return;
132:             }
133: 
134:             try
135:             {
136:                 user.Role = newRole;
137:                 EnsureProfile(user);
138:                 _db.SaveChanges();
139:                 AuditHelper.Log(_db, "SetRole", $"Đổi {user.Username} thành {RoleName(newRole)}", "Users", user.UserId);
140:                 LoadUsers();
141:                 GridHelper.SelectById(dgUsers, "UserId", user.UserId);
142:             }
143:             catch (Exception ex)
144:             {
145:                 Msg.Error(ex);
146:             }
147:         }
148: 
149:         // Role Coach cần có dòng Coaches, role Member cần có dòng Members
150:         private void EnsureProfile(User user)
151:         {
152:             if (user.Role == "Coach" && !_db.Coaches.Any(c => c.UserId == user.UserId))
153:             {
154:                 _db.Coaches.Add(new Coach { User = user, FullName = user.FullName, Email = user.Username + "@center.vn", Phone = user.Phone });
155:             }
156:             if (user.Role == "Member" && !_db.Members.Any(m => m.UserId == user.UserId))
157:             {
158:                 _db.Members.Add(new Member { User = user, FullName = user.FullName, Phone = user.Phone, CreatedAt = DateTime.Now });
159:             }
160:         }
161: 
162:         private void btnAddUser_Click(object sender, RoutedEventArgs e)
163:         {
164:             string fullName = txtFullName.Text.Trim();
165:             string username = txtUsername.Text.Trim();
166:             if (fullName == "" || username == "")
167:             {
168:                 Msg.Warn("Vui lòng nhập Họ tên và Tên đăng nhập.");
169:                 return;
170:             }
171:             if (AccountHelper.FullNameError(fullName) is string nameError)
172:             {
173:                 Msg.Warn(nameError);
174:                 return;
175:             }
176:             if (AccountHelper.UsernameError(username) is string usernameError)
177:             {
178:                 Msg.Warn(usernameError);
179:                 return;
180:             }
181:             if (_db.Users.Any(u => u.Username == username))
182:             {
183:                 Msg.Warn("Tên đăng nhập đã tồn tại.");
184:                 return;
185:             }
186: 
187:             try
188:             {
189:                 var user = new User
190:                 {
191:                     Username = username,
192:                     PasswordHash = PasswordHelper.Hash("123456"),
193:                     FullName = fullName,
194:                     Role = _roles[cbxRole.SelectedIndex],
195:                     IsActive = true,
196:                     CreatedAt = DateTime.Now
197:                 };
198:                 _db.Users.Add(user);
199:                 EnsureProfile(user);
200:                 _db.SaveChanges();
201:                 AuditHelper.Log(_db, "CreateUser", $"Tạo tài khoản {username} ({RoleName(user.Role)})", "Users", user.UserId);
202: 
203:                 txtFullName.Clear();
204:                 txtUsername.Clear();
205:                 pgUsers.Reset();
206:                 LoadUsers();
207:                 Msg.Info($"Đã tạo tài khoản {username}. Mật khẩu mặc định là 123456, hãy nhờ người dùng đổi mật khẩu.");
208:             }
209:             catch (Exception ex)
210:             {
211:                 Msg.Error(ex);
212:             }
213:         }
214:     }
215: }
```

### Giải thích chi tiết từng dòng:
- **Dòng 11**: `private readonly string[] _roles = { "Manager", "Receptionist", "Coach", "Member" };` – Mảng cố định 4 vai trò của hệ thống.
- **Dòng 13**: `RoleName(code)`: Hàm đổi mã vai trò tiếng Anh sang tiếng Việt thông qua từ điển `Rules.RoleNames`.
- **Dòng 15-22**: Khởi tạo `UsersPage`: cấu hình DataGrid, đăng ký sự kiện chuyển trang của `pgUsers`, gọi `LoadData()`.
- **Dòng 24-39 (`LoadData`)**: Đổ danh sách các vai trò vào ComboBox bộ lọc `cbxRoleFilter` ("Tất cả", "Quản lý...", "Hội viên..."), ComboBox chọn vai trò khi tạo mới `cbxRole`, và ComboBox đổi vai trò `cbxNewRole`. Gọi `LoadUsers()`.
- **Dòng 41-70 (`LoadUsers`)**:
  - Dòng 43-54: Lọc theo từ khóa `Username` hoặc `FullName` kết hợp lọc theo vai trò được chọn trong ComboBox.
  - Dòng 56-68: Sắp xếp theo `UserId`, chuyển đổi cờ `IsActive` thành chuỗi `"Đang hoạt động"` / `"Đã khóa"`, đổi tên vai trò sang tiếng Việt.
  - Dòng 69: Cắt đúng 8 dòng/trang đưa lên DataGrid qua `pgUsers.Slice(rows)`.
- **Dòng 72-84**: Các hàm bấm nút "Lọc" (`btnFilter_Click`) và "Xóa lọc" (`btnClearFilter_Click`) đưa chỉ số trang về 1 (`Reset()`) và nạp lại danh sách.
- **Dòng 87-101 (`GetSelectedUser`) – Luật A3 (Bảo vệ tài khoản quản trị)**:
  - Lấy `UserId` của dòng đang chọn trên DataGrid qua `GridHelper.SelectedId(dgUsers, "UserId")`.
  - Dòng 90-94: Nếu chưa chọn dòng nào, hiển thị cảnh báo `Msg.Warn` và trả về `null`.
  - Dòng 95-99: **Cấm tự khóa hoặc tự đổi quyền của chính mình**: Kiểm tra nếu `id == Session.CurrentUser.UserId`, lập tức chặn lại bằng `Msg.Warn("Không thể khóa hoặc đổi vai trò của chính tài khoản đang đăng nhập.")` và trả về `null`.
  - Dòng 100: Tìm và trả về thực thể `User` từ DbContext.
- **Dòng 103-120 (`btnToggle_Click`) – Khóa / Mở khóa tài khoản**:
  - Lấy tài khoản được chọn qua `GetSelectedUser()`.
  - Dòng 110: Đảo ngược trạng thái hoạt động: `user.IsActive = !user.IsActive;`.
  - Dòng 111-112: Lưu thay đổi `_db.SaveChanges()` và ghi log kiểm toán hành động `"ToggleUser"`.
  - Dòng 113-114: Tải lại danh sách và gọi `GridHelper.SelectById(dgUsers, "UserId", user.UserId)` để giữ nguyên tiêu điểm vào dòng vừa thao tác.
- **Dòng 122-147 (`btnChangeRole_Click`) – Đổi vai trò**:
  - Lấy vai trò mới từ `cbxNewRole`. Nếu trùng vai trò hiện tại thì cảnh báo và dừng lại.
  - Dòng 136: Cập nhật `user.Role = newRole;`.
  - Dòng 137: Gọi hàm `EnsureProfile(user)` để tạo hồ sơ liên kết.
  - Dòng 138-141: Lưu CSDL, ghi log `"SetRole"`, nạp lại bảng và chọn lại dòng vừa sửa.
- **Dòng 150-160 (`EnsureProfile`) – Đảm bảo toàn vẹn dữ liệu đa bảng**:
  - Dòng 152-155: Nếu vai trò là `"Coach"` mà bảng `Coaches` chưa có dòng nào ứng với `UserId` này, tự động thêm mới một bản ghi `Coach` với email mặc định là `{username}@center.vn`.
  - Dòng 156-159: Nếu vai trò là `"Member"` mà bảng `Members` chưa có dòng nào ứng với `UserId`, tự động thêm mới bản ghi `Member` với ngày tạo là `DateTime.Now`.
  - 👉 Nhờ hàm này, hệ thống không bao giờ bị lỗi "tài khoản mồ côi" khi truy vấn các màn hình Huấn luyện viên hoặc Hội viên.
- **Dòng 162-213 (`btnAddUser_Click`) – Quản lý tạo tài khoản nhân sự mới**:
  - Dòng 164-185: Kiểm tra không để trống Họ tên và Username; kiểm tra định dạng qua `AccountHelper.FullNameError` và `AccountHelper.UsernameError`; kiểm tra chống trùng username trong bảng `Users`.
  - Dòng 189-197: Tạo đối tượng `User` với **mật khẩu mặc định là 123456** băm qua `PasswordHelper.Hash("123456")`, cờ `IsActive = true`, ngày tạo hiện tại.
  - Dòng 198-201: Thêm user vào `db.Users`, gọi `EnsureProfile(user)` để tạo kèm hồ sơ tương ứng, lưu CSDL và ghi log `"CreateUser"`.
  - Dòng 203-207: Xóa trắng form nhập, nạp lại danh sách và hiển thị thông báo thành công kèm lời nhắc người dùng đổi mật khẩu trong lần đăng nhập đầu tiên.

---
*Tài liệu được trích xuất và giải thích chi tiết 100% từng dòng mã nguồn của Người 1.*
