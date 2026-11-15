# Giải thích code phần Người 2 (từng dòng)

Phần của Người 2: **Member, Gói thành viên, Mua / gia hạn gói, Lịch sử hoạt động**. File này được sinh từ code thật trong project (số dòng ghi bên trái mỗi đoạn code khớp file). Cùng nội dung với trang Claude "Đọc code Người 2".

---

## Cách đọc trang này

Phần của Người 2 gồm 4 màn: **Member**, **Gói thành viên**, **Mua / gia hạn gói** và **Lịch sử hoạt động**. Trang này chứa toàn bộ code liên quan (lấy thẳng từ file thật, số dòng khớp với file) kèm giải thích từng đoạn. Bạn chỉ cần đọc từ trên xuống.

Thứ tự đọc (từ dễ đến khó): 1. Bốn bảng SQL → 2. Công cụ dùng chung → 3. PackagesPage → 4. AuditHelper + AuditPage → 5. Rules.Packages → 6. MembersPage → 7. BuyPage → 8. Ôn tập.

```
Members ───────────┐
                   ├── Subscriptions ─── Payments
MembershipPackages ┘   (mỗi lần mua = 1 dòng)  (hóa đơn)

AuditLogs  (độc lập, chỉ trỏ tới Users)
```
Màn Payments do Người 5 làm, nhưng `BuyPage` của bạn là nơi tạo ra dòng Payments khi bán gói.

### Khuôn chung của 4 màn (nhớ phần này là đọc được cả 4 file)

**Khuôn chung của 4 màn:**
1. Hàm tạo: `InitializeComponent()` → `_db = new()` → `GridHelper.Setup(dg)` → `LoadData()`.
2. `LoadData()` gom các hàm `LoadX()`; mỗi hàm là một câu LINQ (`Where`, `Select`) → `ToList()` → `dg.ItemsSource = ...`.
3. Nút bấm: kiểm tra ô nhập (sai thì `MessageBox.Show` + `return`) → `try { ghi + SaveChanges + Log + làm mới } catch`; xóa ô nhập bằng `ClearForm()`.

### Từ vựng C# / EF Core gặp trong mọi file

| Thấy gì | Nghĩa là gì |
|---|---|
| `using X;` | Dùng sẵn đồ nghề trong thư viện X (giống `import`). |
| `partial class PackagesPage : UserControl` | Class của một màn hình. `partial` vì nửa còn lại do file `.xaml` sinh ra (giao diện). `: UserControl` nghĩa là "là một control WPF". |
| `_db = new();` | Tạo `SportsCenterContext` (đối tượng EF Core, cầu nối tới SQL Server). Mỗi màn có một cái. |
| `_db.Members` | Bảng `Members`. Bạn viết truy vấn bằng LINQ thay vì SQL. |
| `.Where(x => điều kiện)` | `WHERE`. Mũi tên `=>` đọc là "với mỗi x thì...". |
| `.OrderBy / .OrderByDescending` | `ORDER BY` tăng / giảm. |
| `.Select(x => new { ... })` | Chọn cột hoặc tạo cột tính sẵn. `new { }` là object ẩn danh (không cần khai báo class). |
| `.ToList()` | **Lúc này SQL mới thật sự chạy**, kết quả về bộ nhớ. Trước đó chỉ là "đang soạn câu hỏi". |
| `.FirstOrDefault()` | Lấy dòng đầu; không có thì trả `null` (không báo lỗi). |
| `.Find(id)` | Lấy một dòng theo khóa chính. |
| `.Any(...)`, `.Count()` | Có dòng nào thỏa không (true/false) / đếm dòng. |
| `.Include(s => s.Package)` | Kéo luôn dòng liên quan (JOIN) để dùng `s.Package.PackageName`. |
| `.AsNoTracking()` | Chỉ đọc, EF không theo dõi thay đổi. Nhanh hơn và không lỡ lưu nhầm. |
| `_db.X.Add(obj)` | Đánh dấu "sắp thêm". **Chưa vào DB.** |
| `_db.SaveChanges()` | Gửi mọi thay đổi đang chờ xuống DB trong **một transaction** (hoặc lưu hết, hoặc không lưu gì). |
| `int?`, `DateOnly?` | Kiểu "có thể null". |
| `a ?? b` | Lấy `a`; nếu `a` null thì lấy `b`. |
| `a?.B` | Nếu `a` null thì cả biểu thức là null (không sập). |
| `x!` | Nói với trình biên dịch "chắc chắn không null". |
| `if (x is not int id) {...}` | Nếu `x` **không** phải số nguyên thì vào khối `{...}` (thường là báo lỗi rồi `return`); ngược lại, sau khối `if`, biến `id` đã có giá trị số nguyên để dùng. Kiểu viết của template Q2 (`is not Department department`). |
| `if (x is string s)` | Nếu `x` đúng là chuỗi thì gán vào `s` rồi dùng luôn. |
| `MessageBox.Show("...")` | Hộp thoại thông báo của Windows. |
| `catch (Exception ex)` | Có lỗi bất ngờ thì rơi vào đây, app không sập. |

---

## 1. Bốn bảng SQL

Tên cột khớp từng chữ với tên thuộc tính trong code C# (cột `PackageName` ↔ `p.PackageName`). File gốc: `SportsCenter.sql`.

#### Members: hồ sơ hội viên
```sql
  66 | CREATE TABLE Members (
  67 |     MemberId    INT IDENTITY(1,1) PRIMARY KEY,
  68 |     UserId      INT NULL,
  69 |     FullName    NVARCHAR(100) NOT NULL,
  70 |     Phone       NVARCHAR(20)  NULL,
  71 |     Email       NVARCHAR(150) NULL,
  72 |     DateOfBirth DATE          NULL,
  73 |     Gender      NVARCHAR(10)  NULL,
  74 |     CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE(),
  75 |     CONSTRAINT FK_Members_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
  76 | );
  77 | CREATE UNIQUE INDEX UX_Members_UserId ON Members(UserId) WHERE UserId IS NOT NULL;
  78 | -- Moi so dien thoai chi thuoc ve MOT member (app cung kiem tra truoc, day la lop chan cuoi cung o database)
  79 | CREATE UNIQUE INDEX UX_Members_Phone ON Members(Phone) WHERE Phone IS NOT NULL;
  80 | GO
```
- **66-76:** `MemberId INT IDENTITY(1,1) PRIMARY KEY`: số thứ tự **do SQL Server tự cấp** (1, 2, 3...). Code C# không tự đặt. `UserId NULL`: member có tài khoản thì có số này, đăng ký tại quầy thì `NULL` (màn Member hiện "Có / Chưa có"). `FullName` bắt buộc, `Phone` và `Email` có thể null.
- **77:** Mỗi tài khoản chỉ gắn với một member (chỉ áp dụng khi `UserId` không null).
- **78-79:** **`UX_Members_Phone`: mỗi số điện thoại chỉ thuộc một member.** Đây là lớp chặn cuối cùng ở DB; app cũng kiểm tra trước (xem `btnAddMember_Click`).

#### MembershipPackages và Subscriptions
```sql
  94 | CREATE TABLE MembershipPackages (
  95 |     PackageId    INT IDENTITY(1,1) PRIMARY KEY,
  96 |     PackageName  NVARCHAR(100)  NOT NULL,
  97 |     DurationDays INT            NOT NULL CHECK (DurationDays > 0),
  98 |     Price        DECIMAL(18,2)  NOT NULL CHECK (Price >= 0),
  99 |     Description  NVARCHAR(300)  NULL,
 100 |     IsActive     BIT            NOT NULL DEFAULT 1
 101 | );
 102 | GO
 103 | CREATE TABLE Subscriptions (
 104 |     SubscriptionId INT IDENTITY(1,1) PRIMARY KEY,
 105 |     MemberId       INT NOT NULL,
 106 |     PackageId      INT NOT NULL,
 107 |     StartDate      DATE NOT NULL,
 108 |     EndDate        DATE NOT NULL,
 109 |     Status         NVARCHAR(20) NOT NULL DEFAULT N'Active'
 110 |                    CONSTRAINT CK_Subscriptions_Status CHECK (Status IN (N'Active', N'Expired', N'Cancelled')),
 111 |     CONSTRAINT CK_Subscriptions_Dates CHECK (EndDate >= StartDate),
 112 |     CONSTRAINT FK_Subscriptions_Members  FOREIGN KEY (MemberId)  REFERENCES Members(MemberId),
 113 |     CONSTRAINT FK_Subscriptions_Packages FOREIGN KEY (PackageId) REFERENCES MembershipPackages(PackageId)
 114 | );
 115 | GO
```
- **94-101:** `MembershipPackages`: gói bán ra. `CHECK (DurationDays > 0)` và `CHECK (Price >= 0)`: DB từ chối số ngày hay giá sai. `IsActive BIT`: 1 = đang bán, 0 = ngừng bán. **Không bao giờ xóa gói**, chỉ đổi cột này.
- **103-109:** `Subscriptions`: mỗi lần member mua một gói là một dòng. `MemberId` và `PackageId` là khóa ngoại. `StartDate`, `EndDate` kiểu `DATE`. `Status` mặc định `Active`.
- **110-113:** `Status` chỉ nhận `Active / Expired / Cancelled`. `CHECK (EndDate >= StartDate)`. Hai `FOREIGN KEY` bắt buộc member và gói phải tồn tại. App **không dựa vào Status để biết còn hạn hay không**, mà so ngày với hôm nay (xem `SubscriptionText`).

#### Payments: bảng của Người 5 nhưng BuyPage của bạn ghi vào
```sql
 181 | CREATE TABLE Payments (
 182 |     PaymentId      INT IDENTITY(1,1) PRIMARY KEY,
 183 |     InvoiceNo      NVARCHAR(30)  NOT NULL UNIQUE,
 184 |     MemberId       INT NOT NULL,
 185 |     SubscriptionId INT NULL,
 186 |     Amount         DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
 187 |     PaymentMethod  NVARCHAR(20)  NOT NULL
 188 |                    CONSTRAINT CK_Payments_Method CHECK (PaymentMethod IN (N'Cash', N'BankTransfer', N'Card')),
 189 |     PaidAt         DATETIME NOT NULL DEFAULT GETDATE(),
 190 |     ReceivedBy     INT NULL,   -- UserId nhan vien ghi nhan
 191 |     Status         NVARCHAR(20) NOT NULL DEFAULT N'Paid'
 192 |                    CONSTRAINT CK_Payments_Status CHECK (Status IN (N'Paid', N'Refunded')),
 193 |     Note           NVARCHAR(200) NULL,
 194 |     CONSTRAINT FK_Payments_Members       FOREIGN KEY (MemberId)       REFERENCES Members(MemberId),
 195 |     CONSTRAINT FK_Payments_Subscriptions FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(SubscriptionId),
 196 |     CONSTRAINT FK_Payments_Users         FOREIGN KEY (ReceivedBy)     REFERENCES Users(UserId)
 197 | );
 198 | GO
```
- **183:** `InvoiceNo UNIQUE`: mã hóa đơn không được trùng (INV-0001...).
- **185:** `SubscriptionId NULL`: hóa đơn trỏ tới gói vừa mua. Khi mua gói, EF điền số này giúp bạn (xem `BuyPage`).
- **186-188:** `Amount > 0`; `PaymentMethod` chỉ nhận `Cash / BankTransfer / Card`.
- **190:** `ReceivedBy`: UserId của nhân viên thu tiền.

#### AuditLogs: lịch sử hoạt động
```sql
 214 | CREATE TABLE AuditLogs (
 215 |     LogId      INT IDENTITY(1,1) PRIMARY KEY,
 216 |     UserId     INT NULL,
 217 |     Action     NVARCHAR(50)  NOT NULL,   -- Login, CreateClass, RegisterClass, Payment, ...
 218 |     EntityName NVARCHAR(50)  NULL,
 219 |     EntityId   INT NULL,
 220 |     Details    NVARCHAR(300) NULL,
 221 |     CreatedAt  DATETIME NOT NULL DEFAULT GETDATE(),
 222 |     CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
 223 | );
 224 | GO
```
- **215-221:** `Action` là **mã** hành động (`Payment`, `CreateMember`...), `EntityName` + `EntityId` cho biết tác động lên bảng nào, dòng nào, `Details` là câu mô tả, `CreatedAt` là giờ, `UserId` là người làm.

---

## 2. Công cụ dùng chung

Các file này của Người 1 và Người 5. Bạn **không cần hiểu bên trong**, chỉ cần biết hàm làm gì và gọi thế nào. Bốn màn của bạn báo bằng `MessageBox.Show` thẳng và không phân trang, nên không dùng `Msg` hay `PagerControl`.

#### GridHelper.Setup: tiêu đề tiếng Việt và ẩn cột Id
```csharp
  62 |         public static void Setup(DataGrid dg, bool editable = false)
  63 |         {
  64 |             dg.AutoGenerateColumns = true;
  65 |             dg.IsReadOnly = !editable;
  66 |             dg.AutoGeneratingColumn += (s, e) =>
  67 |             {
  68 |                 string name = e.PropertyName;
  69 | 
  70 |                 // Ẩn các cột khóa (ClassId, MemberId...) nhưng vẫn giữ giá trị để lấy khi chọn dòng
  71 |                 if (name.Length > 2 && name.EndsWith("Id"))
  72 |                 {
  73 |                     e.Cancel = true;
  74 |                     return;
  75 |                 }
  76 | 
  77 |                 string header = Headers.TryGetValue(name, out var h) ? h : name;
  78 |                 Type type = Nullable.GetUnderlyingType(e.PropertyType) ?? e.PropertyType;
```
- **62-65:** `GridHelper.Setup(dg)`: cài DataGrid. `AutoGenerateColumns = true` nghĩa là bảng tự tạo cột từ các thuộc tính của `new { ... }` trong `Select`.
- **70-75:** **Ẩn mọi cột có tên kết thúc bằng `Id`** (`PackageId`, `MemberId`...) nhưng vẫn giữ giá trị trong dòng. Vì vậy `Select` của bạn có `PackageId` mà trên màn hình không thấy cột này.
- **77:** Tiêu đề cột lấy từ từ điển `Headers` (`PackageName` → "Gói", `DurationDays` → "Thời hạn (ngày)").

#### GridHelper: định dạng, lấy / chọn dòng
```csharp
 107 |                 if (e.Column is DataGridTextColumn col && col.Binding is Binding b)
 108 |                 {
 109 |                     if (type == typeof(decimal))
 110 |                         b.StringFormat = "N0";
 111 |                     else if (type == typeof(DateTime))
 112 |                         b.StringFormat = "dd/MM/yyyy HH:mm";
 113 |                     else if (type == typeof(DateOnly))
 114 |                         b.StringFormat = "dd/MM/yyyy";
 115 |                 }
 116 |             };
 117 |         }
 118 | 
 119 |         // Chọn lại dòng có Id này (dùng sau khi tải lại danh sách, để người dùng không phải chọn lại)
 120 |         public static void SelectById(DataGrid dg, string propertyName, int id)
 121 |         {
 122 |             foreach (var item in dg.Items)
 123 |             {
 124 |                 if (item.GetType().GetProperty(propertyName)?.GetValue(item) as int? == id)
 125 |                 {
 126 |                     dg.SelectedItem = item;
 127 |                     return;
 128 |                 }
 129 |             }
 130 |         }
 131 | 
 132 |         // Lấy giá trị cột Id (vd "ClassId") của dòng đang chọn; không chọn gì thì trả về null
 133 |         public static int? SelectedId(DataGrid dg, string propertyName)
 134 |         {
 135 |             object? item = dg.SelectedItem;
 136 |             if (item == null) return null;
 137 |             return item.GetType().GetProperty(propertyName)?.GetValue(item) as int?;
 138 |         }
```
- **107-115:** **Đây là chỗ có `Binding b`.** Mỗi cột chữ có một `Binding` (dây nối: ô đọc thuộc tính nào, hiển thị ra sao); `b.StringFormat` đặt cách hiển thị: tiền `decimal` thành `N0` (1.500.000), ngày thành `dd/MM/yyyy`. `col.Binding is Binding b` vừa kiểm tra kiểu vừa gán biến `b`.
- **120-130:** `SelectById`: sau khi tải lại bảng, chọn lại đúng dòng có Id này để người dùng không phải bấm lại.
- **133-138:** `SelectedId(dg, "PackageId")`: đọc cột `PackageId` của **dòng đang chọn**; chưa chọn dòng nào thì trả `null`. Mọi nút bấm của bạn bắt đầu bằng hàm này, thường viết `... is not int id`.

#### Session: ai đang đăng nhập
```csharp
   6 |     // Lưu thông tin người đang đăng nhập (dùng chung cho mọi màn hình)
   7 |     public static class Session
   8 |     {
   9 |         public static User? CurrentUser { get; set; }
  10 | 
  11 |         // Có giá trị khi người đăng nhập là Member
  12 |         public static Member? CurrentMember { get; set; }
  13 | 
  14 |         // Có giá trị khi người đăng nhập là Coach
  15 |         public static Coach? CurrentCoach { get; set; }
  16 | 
  17 |         public static string Role => CurrentUser?.Role ?? "";
  18 | 
  19 |         public static void SignIn(SportsCenterContext db, User user)
  20 |         {
  21 |             CurrentUser = user;
  22 |             CurrentMember = user.Role == "Member" ? db.Members.AsNoTracking().FirstOrDefault(m => m.UserId == user.UserId) : null;
  23 |             CurrentCoach = user.Role == "Coach" ? db.Coaches.AsNoTracking().FirstOrDefault(c => c.UserId == user.UserId) : null;
  24 |             AuditHelper.Log(db, "Login", "Đăng nhập hệ thống");
  25 |         }
  26 | 
  27 |         public static void Clear()
  28 |         {
  29 |             CurrentUser = null;
  30 |             CurrentMember = null;
  31 |             CurrentCoach = null;
  32 |         }
  33 |     }
```
- **9-15:** Ba biến tĩnh lưu người đang đăng nhập: `CurrentUser`, `CurrentMember` (có khi là Member), `CurrentCoach`.
- **17:** `Session.Role` = vai trò ("Manager", "Receptionist", "Coach", "Member"); chưa đăng nhập thì chuỗi rỗng.
- **19-25:** `SignIn`: gán các biến trên rồi ghi log `Login`.

#### Rules.cs: phương thức thanh toán
```csharp
  26 |         // Phương thức thanh toán: khóa lưu trong DB -> tên hiển thị
  27 |         public static readonly Dictionary<string, string> MethodNames = new()
  28 |         {
  29 |             ["Cash"] = "Tiền mặt",
  30 |             ["BankTransfer"] = "Chuyển khoản",
  31 |             ["Card"] = "Thẻ"
  32 |         };
  33 | 
  34 |         public static readonly List<object> MethodList =
  35 |             MethodNames.Select(kv => (object)new { Key = kv.Key, Name = kv.Value }).ToList();
```
- **27-32:** Từ điển mã → tên hiển thị của phương thức thanh toán.
- **34-35:** `MethodList`: biến từ điển thành danh sách `{ Key, Name }` cho ComboBox (`BuyPage` dùng).

#### Rules.cs: tên hành động
```csharp
  37 |         // Hành động ghi trong Lịch sử hoạt động: mã lưu trong DB -> tên hiển thị
  38 |         // (thêm hành động mới thì thêm một dòng vào đúng nhóm của mình, nhóm theo thứ tự Người 1 → 5)
  39 |         public static readonly Dictionary<string, string> ActionNames = new()
  40 |         {
  41 |             // Người 1: tài khoản
  42 |             ["Login"] = "Đăng nhập",
  43 |             ["ChangePassword"] = "Đổi mật khẩu",
  44 |             ["CreateUser"] = "Tạo tài khoản",
  45 |             ["SetRole"] = "Đổi vai trò",
  46 |             ["ToggleUser"] = "Khóa/Mở tài khoản",
  47 |             ["LinkMember"] = "Liên kết hồ sơ",
  48 |             // Người 2: member, gói
  49 |             ["CreateMember"] = "Đăng ký tại quầy",
  50 |             ["CreatePackage"] = "Tạo gói",
  51 |             ["TogglePackage"] = "Mở/Ngừng bán gói",
  52 |             // Người 3: lớp học
  53 |             ["CreateSport"] = "Thêm bộ môn",
  54 |             ["DeleteSport"] = "Xóa bộ môn",
  55 |             ["CreateRoom"] = "Thêm phòng",
  56 |             ["DeleteRoom"] = "Xóa phòng",
  57 |             ["CreateClass"] = "Tạo lớp",
  58 |             // Người 4: đặt lớp, điểm danh
  59 |             ["CancelClass"] = "Hủy lớp/đăng ký",
  60 |             ["RegisterClass"] = "Đăng ký lớp",
  61 |             ["Attendance"] = "Điểm danh",
  62 |             // Người 5: thanh toán
  63 |             ["Payment"] = "Thanh toán"
  64 |         };
  65 | 
  66 |         public static string ActionName(string code) =>
  67 |             ActionNames.TryGetValue(code, out var name) ? name : code;
```
- **39-64:** `ActionNames`: mã hành động lưu trong DB → tên tiếng Việt hiển thị. Nhóm "Người 2" gồm `CreateMember`, `CreatePackage`, `TogglePackage`.
- **66-67:** `ActionName(code)`: tra tên, không thấy thì trả lại chính mã. `AuditPage` dùng hàm này.

#### Rules.cs: mã hóa đơn
```csharp
  80 |         // Mã hóa đơn tiếp theo dạng INV-0001 (Y1)
  81 |         public static string NextInvoiceNo(SportsCenterContext db)
  82 |         {
  83 |             int n = db.Payments.Count() + 1;
  84 |             string no;
  85 |             do
  86 |             {
  87 |                 no = "INV-" + n.ToString("D4");
  88 |                 n++;
  89 |             } while (db.Payments.Any(p => p.InvoiceNo == no));
  90 |             return no;
  91 |         }
```
- **81-91:** `NextInvoiceNo`: lấy số hóa đơn hiện có + 1, định dạng `INV-0004` (`"D4"` = 4 chữ số, thêm số 0 phía trước). Vòng `do ... while` tăng tiếp nếu mã này đã có, nên không bao giờ trùng.

#### AccountHelper.Normalize: chuẩn hóa chữ
```csharp
  13 |         // Bỏ dấu, đổi thành chữ thường, gộp khoảng trắng: "  Nguyễn  Văn Đạt " == "nguyen van dat"
  14 |         public static string Normalize(string text)
  15 |         {
  16 |             string lower = text.Trim().ToLowerInvariant().Replace('đ', 'd');
  17 |             var sb = new StringBuilder();
  18 |             foreach (char ch in lower.Normalize(NormalizationForm.FormD))
  19 |             {
  20 |                 if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
  21 |                     sb.Append(ch);
  22 |             }
  23 |             return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
  24 |         }
```
- **14-24:** `Normalize`: bỏ dấu, chữ thường, gộp khoảng trắng. "Gói 3 Tháng" và "goi 3 thang" cho cùng kết quả. Dùng để chống trùng tên gói.

#### AccountHelper: kiểm tra email và họ tên
```csharp
  47 |         // Email đơn giản: có @ và có dấu chấm ở phần sau, không có khoảng trắng
  48 |         public static bool IsValidEmail(string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
  49 | 
  50 |         // Tên đăng nhập: 3-30 ký tự, chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới (không có khoảng trắng). Trả về lỗi hoặc null.
  51 |         public static string? UsernameError(string username)
  52 |         {
  53 |             if (username.Length < 3 || username.Length > 30) return "Tên đăng nhập gồm 3-30 ký tự.";
  54 |             if (!Regex.IsMatch(username, @"^[A-Za-z0-9._]+$"))
  55 |                 return "Tên đăng nhập chỉ gồm chữ không dấu, số, dấu chấm hoặc gạch dưới (không có khoảng trắng).";
  56 |             return null;
  57 |         }
  58 | 
  59 |         // Họ tên: 2-100 ký tự, chỉ gồm chữ cái (có dấu), khoảng trắng, dấu chấm, dấu nháy hoặc gạch nối; không có số. Trả về lỗi hoặc null.
  60 |         public static string? FullNameError(string fullName)
  61 |         {
  62 |             string n = fullName.Trim().Normalize(NormalizationForm.FormC);
  63 |             if (n.Length < 2 || n.Length > 100) return "Họ tên gồm 2-100 ký tự.";
  64 |             if (!Regex.IsMatch(n, @"^\p{L}[\p{L}\p{M} .'’-]*$"))
  65 |                 return "Họ tên chỉ gồm chữ cái và khoảng trắng, không có số hoặc ký tự đặc biệt.";
  66 |             return null;
  67 |         }
```
- **48:** `IsValidEmail`: có `@`, có dấu chấm phía sau, không có khoảng trắng.
- **60-67:** `FullNameError`: trả về câu lỗi nếu họ tên sai (2-100 ký tự, chỉ chữ cái có dấu, khoảng trắng, dấu chấm, nháy, gạch nối), hoặc `null` nếu hợp lệ.

---

## 3. PackagesPage

**`Pages/PackagesPage.xaml.cs`** (126 dòng): Màn **Gói thành viên**: xem danh sách, thêm gói, ngừng / mở bán lại. Chỉ Manager vào được.

#### Hàm tạo và LoadData: chạy một lần khi mở màn
```csharp
   1 | using Microsoft.EntityFrameworkCore;
   2 | using SportsCenter.Entities;
   3 | using System.Windows;
   4 | using System.Windows.Controls;
   5 | 
   6 | namespace SportsCenter
   7 | {
   8 |     public partial class PackagesPage : UserControl
   9 |     {
  10 |         private SportsCenterContext _db;
  11 | 
  12 |         public PackagesPage()
  13 |         {
  14 |             InitializeComponent();
  15 |             _db = new();
  16 |             GridHelper.Setup(dgPackages);   // cài bảng: tiêu đề tiếng Việt, ẩn cột Id (của Người 5)
  17 |             LoadData();
  18 |         }
  19 | 
  20 |         private void LoadData()
  21 |         {
  22 |             LoadPackages();
  23 |         }
```
- **1-4:** `using`: dùng các thư viện (EF Core, entity của dự án, WPF).
- **6:** `namespace SportsCenter`: tên "ngăn kéo" chứa class.
- **8:** Class màn hình. `partial` + `: UserControl` (xem bảng từ vựng).
- **10:** `_db`: biến lưu "cầu nối DB", dùng cho cả class.
- **14:** `InitializeComponent()`: dựng giao diện từ file `.xaml`. **Bắt buộc là dòng đầu.**
- **15:** Mở kết nối DB.
- **16:** Cài bảng `dgPackages` (tiêu đề tiếng Việt, ẩn cột Id).
- **17:** Gọi `LoadData()` để hiện dữ liệu lần đầu.
- **20-23:** `LoadData()` gom các hàm Load của màn. Màn này chỉ có một hàm Load, nhưng viết vậy để 4 màn có cùng một dạng hàm tạo.

#### LoadPackages = một câu SELECT viết bằng LINQ
```csharp
  25 |         // Hiện danh sách gói lên bảng
  26 |         private void LoadPackages()
  27 |         {
  28 |             dgPackages.ItemsSource = _db.MembershipPackages
  29 |                 .OrderBy(p => p.PackageId)
  30 |                 .Select(p => new
  31 |                 {
  32 |                     p.PackageId,
  33 |                     p.PackageName,
  34 |                     p.DurationDays,
  35 |                     p.Price,
  36 |                     Status = p.IsActive ? "Đang bán" : "Ngừng bán"
  37 |                 }).ToList();
  38 |         }
```
- **28-29:** `FROM MembershipPackages ... ORDER BY PackageId`.
- **30-37:** `Select` chọn các cột. `Status = p.IsActive ? "Đang bán" : "Ngừng bán"` là **cột tự tính**: `true` thì chữ này, `false` thì chữ kia. `PackageId` có trong `Select` nhưng không hiện (bị `GridHelper` ẩn); nó ở đó để nút bấm biết đang chọn gói nào. `ToList()` ở dòng 37: **SQL chạy tại đây**, kết quả đổ thẳng vào bảng.

#### btnTogglePackage_Click: Ngừng bán / Mở bán lại
```csharp
  40 |         // Gói đã có người mua thì không xóa, chỉ ngừng bán / mở bán lại
  41 |         private void btnTogglePackage_Click(object sender, RoutedEventArgs e)
  42 |         {
  43 |             if (GridHelper.SelectedId(dgPackages, "PackageId") is not int id)
  44 |             {
  45 |                 MessageBox.Show("Vui lòng chọn một gói trong danh sách.");
  46 |                 return;
  47 |             }
  48 | 
  49 |             try
  50 |             {
  51 |                 var pkg = _db.MembershipPackages.Find(id)!;
  52 |                 pkg.IsActive = !pkg.IsActive;
  53 |                 _db.SaveChanges();
  54 |                 AuditHelper.Log(_db, "TogglePackage", (pkg.IsActive ? "Mở bán " : "Ngừng bán ") + pkg.PackageName, "MembershipPackages", pkg.PackageId);
  55 |                 LoadPackages();
  56 |                 GridHelper.SelectById(dgPackages, "PackageId", pkg.PackageId);
  57 |             }
  58 |             catch (Exception ex)
  59 |             {
  60 |                 MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
  61 |             }
  62 |         }
```
- **40:** **Vì sao không xóa gói:** gói đã bán thì `Subscriptions` còn trỏ tới nó (khóa ngoại); xóa sẽ mất lịch sử. Nên chỉ ngừng bán.
- **43-47:** `GridHelper.SelectedId(...) is not int id`: lấy `PackageId` của dòng đang chọn. **Không phải số** (chưa chọn dòng nào, hàm trả `null`) thì cảnh báo rồi `return`. Qua khỏi khối `if` thì `id` chắc chắn là số nguyên.
- **51:** `Find(id)` lấy gói theo khóa chính; `!` nói "chắc chắn có".
- **52:** **Đảo đúng/sai**: đang bán thành ngừng bán và ngược lại.
- **53:** `SaveChanges()`: EF nhận ra `IsActive` đổi và tự viết lệnh `UPDATE`.
- **54:** Ghi lịch sử, chuỗi "Ngừng bán Gói 1 tháng" hoặc "Mở bán ...".
- **55-56:** Tải lại bảng rồi chọn lại đúng dòng vừa bấm.
- **58-61:** `catch`: có lỗi bất ngờ thì hiện hộp lỗi (`ex.InnerException?.Message ?? ex.Message` lấy lỗi gốc của SQL nếu có).

#### btnAddPackage_Click, chặng 1: kiểm tra
```csharp
  64 |         private void btnAddPackage_Click(object sender, RoutedEventArgs e)
  65 |         {
  66 |             // 1. Kiểm tra dữ liệu nhập
  67 |             string name = txtPackageName.Text.Trim();
  68 |             if (name == "" || txtDays.Text.Trim() == "" || txtPrice.Text.Trim() == "")
  69 |             {
  70 |                 MessageBox.Show("Vui lòng nhập Tên gói, Số ngày và Giá.");
  71 |                 return;
  72 |             }
  73 |             if (!int.TryParse(txtDays.Text.Trim(), out int days) || days <= 0)
  74 |             {
  75 |                 MessageBox.Show("Số ngày phải là số lớn hơn 0.");
  76 |                 return;
  77 |             }
  78 |             if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price) || price < 0)
  79 |             {
  80 |                 MessageBox.Show("Giá phải là số không âm.");
  81 |                 return;
  82 |             }
  83 |             // Tên gói không được trùng (không phân biệt hoa thường và dấu: "Gói 3 tháng" == "goi 3 thang")
  84 |             foreach (var p in _db.MembershipPackages.ToList())
  85 |             {
  86 |                 if (AccountHelper.Normalize(p.PackageName) == AccountHelper.Normalize(name))
  87 |                 {
  88 |                     MessageBox.Show("Đã có gói tên \"" + name + "\".");
  89 |                     return;
  90 |                 }
  91 |             }
```
- **66:** **Chặng 1: kiểm tra.** Sai thì báo và `return`, không chạy tiếp.
- **67:** `Text.Trim()`: lấy chữ trong ô, cắt khoảng trắng đầu và cuối.
- **68-72:** Thiếu ô nào thì cảnh báo và thoát.
- **73:** `int.TryParse(chuoi, out int days)`: thử đổi chuỗi thành số. Được thì trả `true` và số nằm trong `days`; không được thì trả `false` mà không làm app sập (khác `int.Parse`).
- **78:** Tương tự cho giá (`decimal`), không cho số âm.
- **83-91:** Chống trùng tên: duyệt từng gói đang có, chuẩn hóa cả hai tên (bỏ dấu, chữ thường) rồi so. Có tên giống thì báo trùng. Dùng vòng `foreach` thay vì `Any(...)` vì `Normalize` là hàm C#, SQL không hiểu được.

#### btnAddPackage_Click, chặng 2 và 3: ghi dữ liệu, làm mới, ClearForm
```csharp
  93 |             // 2. Thêm gói
  94 |             try
  95 |             {
  96 |                 var pkg = new MembershipPackage
  97 |                 {
  98 |                     PackageName = name,
  99 |                     DurationDays = days,
 100 |                     Price = price,
 101 |                     IsActive = true
 102 |                 };
 103 |                 _db.MembershipPackages.Add(pkg);
 104 |                 _db.SaveChanges();
 105 |                 AuditHelper.Log(_db, "CreatePackage", "Thêm gói " + name, "MembershipPackages", pkg.PackageId);
 106 | 
 107 |                 // 3. Làm mới màn hình
 108 |                 LoadData();
 109 |                 ClearForm();
 110 |                 MessageBox.Show("Đã thêm gói " + name + ".");
 111 |             }
 112 |             catch (Exception ex)
 113 |             {
 114 |                 MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
 115 |             }
 116 |         }
 117 | 
 118 |         // Trả các ô nhập về giá trị mặc định
 119 |         private void ClearForm()
 120 |         {
 121 |             txtPackageName.Clear();
 122 |             txtDays.Text = "30";
 123 |             txtPrice.Text = "500000";
 124 |         }
 125 |     }
 126 | }
```
- **96-102:** Tạo object gói mới. Chưa có `PackageId`.
- **103:** `Add`: đánh dấu "sắp thêm", **chưa vào DB**.
- **104:** `SaveChanges()`: lúc này mới `INSERT`. SQL cấp `PackageId` (cột IDENTITY) và EF **gán ngược** vào `pkg.PackageId`.
- **105:** Ghi log, cần `pkg.PackageId` nên **phải đặt sau `SaveChanges`**. Nếu đặt trước thì `PackageId` vẫn bằng 0.
- **107-110:** **Chặng 3: làm mới.** `LoadData()` tải lại bảng, `ClearForm()` xóa ô nhập, rồi báo thành công.
- **119-124:** `ClearForm()`: trả 3 ô về mặc định (tên rỗng, 30 ngày, 500000). Tách thành hàm riêng như template Q2.

---

## 4. AuditHelper và AuditPage

**`Helpers/AuditHelper.cs`** (22 dòng): Hàm ghi **một dòng** lịch sử. Mọi màn trong app gọi nó sau khi làm xong việc.

#### AuditHelper.Log
```csharp
   1 | using SportsCenter.Entities;
   2 | 
   3 | namespace SportsCenter
   4 | {
   5 |     // Ghi lịch sử hoạt động (bảng AuditLogs)
   6 |     public static class AuditHelper
   7 |     {
   8 |         public static void Log(SportsCenterContext db, string action, string details, string? entityName = null, int? entityId = null)
   9 |         {
  10 |             db.AuditLogs.Add(new AuditLog
  11 |             {
  12 |                 UserId = Session.CurrentUser?.UserId,
  13 |                 Action = action,
  14 |                 EntityName = entityName,
  15 |                 EntityId = entityId,
  16 |                 Details = details,
  17 |                 CreatedAt = DateTime.Now
  18 |             });
  19 |             db.SaveChanges();
  20 |         }
  21 |     }
  22 | }
```
- **6:** `static class`: gọi thẳng `AuditHelper.Log(...)`, không cần `new`.
- **8:** Nhận sẵn `db` của màn đang gọi, mã hành động, câu mô tả. `string? entityName = null, int? entityId = null` là **tham số tùy chọn**: không truyền thì là `null`.
- **10-18:** Tạo dòng `AuditLog`. `Session.CurrentUser?.UserId` là người đang đăng nhập (chưa đăng nhập thì `null`). `CreatedAt = DateTime.Now` là giờ hiện tại.
- **19:** **`Log` tự gọi `SaveChanges()`**, nên mỗi lần ghi log là một lần lưu riêng, nằm ngoài transaction của việc chính.

**`Pages/AuditPage.xaml.cs`** (109 dòng): Màn **Lịch sử hoạt động**: xem log, lọc theo từ khóa, người dùng, hành động, khoảng ngày. Chỉ Manager vào được.

#### Hàm tạo, LoadData, LoadUsers, LoadActions
```csharp
  12 |         public AuditPage()
  13 |         {
  14 |             InitializeComponent();
  15 |             _db = new();
  16 |             GridHelper.Setup(dgAudit);
  17 |             LoadData();
  18 |         }
  19 | 
  20 |         private void LoadData()
  21 |         {
  22 |             LoadUsers();
  23 |             LoadActions();
  24 |             LoadLogs();
  25 |         }
  26 | 
  27 |         // Ô chọn người dùng (có mục "Tất cả")
  28 |         private void LoadUsers()
  29 |         {
  30 |             var users = _db.Users.OrderBy(u => u.Username).Select(u => u.Username).ToList();
  31 |             users.Insert(0, "Tất cả");
  32 |             cbxUser.ItemsSource = users;
  33 |             cbxUser.SelectedIndex = 0;
  34 |         }
  35 | 
  36 |         // Ô chọn hành động (có mục "Tất cả")
  37 |         private void LoadActions()
  38 |         {
  39 |             // DB lưu mã hành động (vd "Payment"), ô chọn hiện tên tiếng Việt (vd "Thanh toán")
  40 |             var actions = new List<string>();
  41 |             foreach (string code in _db.AuditLogs.Select(a => a.Action).Distinct().ToList())
  42 |             {
  43 |                 actions.Add(Rules.ActionName(code));
  44 |             }
  45 |             actions.Sort();
  46 |             actions.Insert(0, "Tất cả");
  47 |             cbxAction.ItemsSource = actions;
  48 |             cbxAction.SelectedIndex = 0;
  49 |         }
```
- **12-18:** Hàm tạo đúng khuôn, gọi `LoadData()`.
- **20-25:** `LoadData()` gom 3 hàm Load: ô người dùng, ô hành động, rồi bảng log.
- **30-33:** `LoadUsers`: lấy tên đăng nhập của mọi tài khoản, sắp A-Z, chèn "Tất cả" lên đầu (`Insert(0, ...)`) rồi đổ vào combo.
- **40-44:** DB lưu **mã** hành động (`Payment`), ô chọn hiện **tên tiếng Việt** (`Thanh toán`). Lấy các mã khác nhau có trong log (`Distinct`), đổi từng mã sang tên bằng `Rules.ActionName`.
- **45-48:** Sắp xếp, chèn "Tất cả" lên đầu, đổ vào combo.

#### LoadLogs, phần 1: lọc cộng dồn
```csharp
  51 |         // Hiện lịch sử hoạt động (mới nhất ở trên), có lọc
  52 |         private void LoadLogs()
  53 |         {
  54 |             var query = _db.AuditLogs.Include(a => a.User).AsQueryable();
  55 | 
  56 |             string keyword = txtKeyword.Text.Trim();
  57 |             if (keyword != "")
  58 |             {
  59 |                 query = query.Where(a => (a.Details ?? "").Contains(keyword)
  60 |                     || (a.User != null && a.User.Username.Contains(keyword)));
  61 |             }
  62 |             if (cbxUser.SelectedItem is string user && user != "Tất cả")
  63 |             {
  64 |                 query = query.Where(a => a.User != null && a.User.Username == user);
  65 |             }
  66 |             if (dpFrom.SelectedDate is DateTime from)
  67 |             {
  68 |                 query = query.Where(a => a.CreatedAt >= from.Date);
  69 |             }
  70 |             if (dpTo.SelectedDate is DateTime to)
  71 |             {
  72 |                 // "Đến ngày 15" nghĩa là lấy hết ngày 15, nên so với 0 giờ ngày 16
  73 |                 query = query.Where(a => a.CreatedAt < to.Date.AddDays(1));
  74 |             }
```
- **54:** `AsQueryable()`: bắt đầu với "toàn bộ bảng" (kèm `Include` để lấy luôn người dùng), **chưa chạy SQL**.
- **56-61:** Có gõ từ khóa thì thêm điều kiện: tìm trong `Details` **hoặc** tên người dùng. `(a.Details ?? "")` phòng `Details` null.
- **62-65:** Có chọn người dùng cụ thể (khác "Tất cả") thì thêm điều kiện theo tên.
- **66-69:** Từ ngày: `dpFrom.SelectedDate is DateTime from` (có chọn ngày thì gán vào `from`), lọc `CreatedAt >= from.Date` (từ 00:00 ngày đó).
- **70-74:** Đến ngày: `CreatedAt < to.Date.AddDays(1)`. `CreatedAt` có cả giờ. Chọn "đến 15/11" phải lấy cả 23:59 ngày 15, nên so với 00:00 ngày 16 và dùng dấu `<`.
- **mẫu chung:** Mỗi `query = query.Where(...)` chỉ thêm một điều kiện `AND`. Điều kiện nào người dùng không chọn thì bỏ qua. Đây là **lọc cộng dồn**.

#### LoadLogs, phần 2: chọn cột, đổi tên hành động, các nút
```csharp
  76 |             var rows = query.OrderByDescending(a => a.CreatedAt).ToList()
  77 |                 .Select(a => new
  78 |                 {
  79 |                     At = a.CreatedAt,
  80 |                     User = a.User != null ? a.User.Username : "-",
  81 |                     Action = Rules.ActionName(a.Action),
  82 |                     a.Details
  83 |                 }).ToList();
  84 | 
  85 |             // Lọc theo hành động: so sánh tên tiếng Việt, nên lọc sau khi đã đổi tên
  86 |             if (cbxAction.SelectedItem is string action && action != "Tất cả")
  87 |             {
  88 |                 rows = rows.Where(r => r.Action == action).ToList();
  89 |             }
  90 | 
  91 |             dgAudit.ItemsSource = rows;
  92 |         }
  93 | 
  94 |         private void btnFilter_Click(object sender, RoutedEventArgs e)
  95 |         {
  96 |             LoadLogs();
  97 |         }
  98 | 
  99 |         private void btnClearFilter_Click(object sender, RoutedEventArgs e)
 100 |         {
 101 |             txtKeyword.Clear();
 102 |             cbxUser.SelectedIndex = 0;
 103 |             cbxAction.SelectedIndex = 0;
 104 |             dpFrom.SelectedDate = null;
 105 |             dpTo.SelectedDate = null;
 106 |             LoadLogs();
 107 |         }
 108 |     }
```
- **76:** Mới nhất lên đầu; `ToList()` chạy SQL **ở đây**, từ dòng sau là xử lý trong bộ nhớ.
- **77-83:** `Select`: chọn cột; đổi mã hành động sang tên tiếng Việt bằng `Rules.ActionName` (hàm C#, nên phải làm sau `ToList`); `User` là `-` nếu log không có người làm.
- **85-89:** Lọc theo hành động: ô chọn là **tên tiếng Việt** nên so với cột `Action` đã đổi tên, vì vậy lọc **sau** khi đổi tên.
- **91:** Đổ danh sách vào bảng.
- **94-97:** `btnFilter_Click`: chỉ gọi lại `LoadLogs()` (các ô lọc đã có giá trị).
- **99-107:** `btnClearFilter_Click`: xóa hết ô lọc rồi tải lại.

---

## 5. Rules.Packages

**`Helpers/Rules.Packages.cs`** (72 dòng): Các **luật về gói**: gói nào đang dùng, gói mới bắt đầu khi nào (P3), gói nào đã mua nối tiếp. Đây là phần khó nhất.

`Rules` là một `static partial class` chia thành nhiều file (mỗi người một file). Bạn gọi `Rules.TênHàm(...)` từ bất kỳ màn nào. `Today` (định nghĩa trong `Rules.cs`) là ngày hôm nay kiểu `DateOnly`.

#### ActiveSubscription: gói đang dùng của member
```csharp
   9 |         // Gói đang dùng: chưa hủy, đã bắt đầu và chưa hết hạn. Không có thì trả null.
  10 |         public static Subscription? ActiveSubscription(SportsCenterContext db, int memberId)
  11 |         {
  12 |             var today = Today;
  13 |             return db.Subscriptions
  14 |                 .AsNoTracking()
  15 |                 .Where(s => s.MemberId == memberId && s.Status != "Cancelled" && s.StartDate <= today && s.EndDate >= today)
  16 |                 .OrderByDescending(s => s.EndDate)
  17 |                 .FirstOrDefault();
  18 |         }
```
- **10:** Kiểu trả về `Subscription?`: có thể là `null` (không có gói đang dùng).
- **12:** Lấy hôm nay một lần cho cả hàm.
- **15:** Gói của member này, **chưa hủy**, **đã bắt đầu** (`StartDate <= hôm nay`) và **chưa hết hạn** (`EndDate >= hôm nay`).
- **16-17:** Nếu có nhiều gói thỏa thì lấy gói hết hạn **xa nhất**; không có thì `null`.

#### CurrentOrLatest và SubscriptionText: trạng thái hiện trong danh sách Member
```csharp
  20 |         // Gói còn hạn nếu có, không thì gói gần nhất (để hiện trạng thái trong danh sách Member)
  21 |         public static Subscription? CurrentOrLatest(IEnumerable<Subscription> mine)
  22 |         {
  23 |             var today = Today;
  24 |             var current = mine
  25 |                 .Where(s => s.Status != "Cancelled" && s.StartDate <= today && s.EndDate >= today)
  26 |                 .OrderByDescending(s => s.EndDate)
  27 |                 .FirstOrDefault();
  28 |             if (current != null)
  29 |             {
  30 |                 return current;
  31 |             }
  32 |             return mine.OrderByDescending(s => s.EndDate).FirstOrDefault();
  33 |         }
  34 | 
  35 |         public static string SubscriptionText(Subscription? s)
  36 |         {
  37 |             if (s == null) return "Chưa có gói";
  38 |             if (s.Status == "Cancelled") return "Đã hủy";
  39 |             if (s.EndDate < Today) return "Hết hạn";
  40 |             return "Còn hạn";
  41 |         }
```
- **21:** Nhận **danh sách gói đã có trong bộ nhớ** (`IEnumerable`), không truy vấn DB.
- **24-27:** Tìm gói đang còn hạn, lấy cái hết hạn xa nhất.
- **28-31:** Có gói còn hạn thì trả luôn gói đó.
- **32:** Không có thì trả gói có `EndDate` xa nhất (tức gói đã hết hạn gần nhất). Nhờ vậy mỗi member luôn hiện đúng một trạng thái trong danh sách.
- **35-41:** `SubscriptionText` ra chữ: `null` → "Chưa có gói"; `Cancelled` → "Đã hủy"; `EndDate < hôm nay` → "Hết hạn"; còn lại → "Còn hạn". Chú ý: dựa vào **ngày**, không dựa vào cột `Status = Expired`.

#### NextStartDate (luật P3): gói mới bắt đầu khi nào
```csharp
  43 |         // P3: ngày bắt đầu của gói mới = ngày sau khi gói XA NHẤT chưa hết hạn kết thúc
  44 |         // (tính cả gói đã mua nối tiếp). Không còn gói nào thì bắt đầu từ hôm nay.
  45 |         public static DateOnly NextStartDate(SportsCenterContext db, int memberId)
  46 |         {
  47 |             var today = Today;
  48 |             var last = db.Subscriptions
  49 |                 .AsNoTracking()
  50 |                 .Where(s => s.MemberId == memberId && s.Status != "Cancelled" && s.EndDate >= today)
  51 |                 .OrderByDescending(s => s.EndDate)
  52 |                 .FirstOrDefault();
  53 |             if (last == null)
  54 |             {
  55 |                 return today;
  56 |             }
  57 |             return last.EndDate.AddDays(1);
  58 |         }
```
- **48-52:** Lấy gói của member, chưa hủy, **chưa hết hạn** (`EndDate >= hôm nay`), sắp theo `EndDate` giảm dần rồi lấy gói đầu tiên: gói hết hạn **xa nhất**. Gói đã mua nối tiếp mà chưa tới ngày bắt đầu cũng nằm trong tập này.
- **53-56:** Không có gói nào (`last == null`) thì gói mới **bắt đầu hôm nay**.
- **57:** Có thì **ngày bắt đầu = ngày kết thúc của gói xa nhất + 1**.
- **ví dụ:** Hôm nay 15/11, member còn gói đến 20/11. Mua gói mới: bắt đầu 21/11. Mua thêm lần nữa: gói xa nhất lúc này là gói vừa mua, nên gói thứ ba nối tiếp sau nó. Các gói xếp nối đuôi nhau, **không chồng lên nhau**.

#### QueuedSubscriptions: các gói đã mua nối tiếp
```csharp
  60 |         // Các gói đã mua nối tiếp nhưng chưa tới ngày bắt đầu
  61 |         public static List<Subscription> QueuedSubscriptions(SportsCenterContext db, int memberId)
  62 |         {
  63 |             var today = Today;
  64 |             return db.Subscriptions
  65 |                 .AsNoTracking()
  66 |                 .Include(s => s.Package)
  67 |                 .Where(s => s.MemberId == memberId && s.Status != "Cancelled" && s.StartDate > today)
  68 |                 .OrderBy(s => s.StartDate)
  69 |                 .ToList();
  70 |         }
```
- **64-69:** Các gói chưa hủy mà **`StartDate > hôm nay`** (đã mua nhưng chưa tới ngày dùng), xếp theo ngày bắt đầu. `Include(s => s.Package)` kéo luôn gói để có tên. `BuyPage` dùng để hiện dòng "Đã mua nối tiếp".

---

## 6. MembersPage

**`Pages/MembersPage.xaml.cs`** (180 dòng): Màn **Member**: danh sách, tìm kiếm, lọc theo trạng thái gói, đăng ký member tại quầy, chuyển sang màn mua gói.

#### Hàm tạo, phân quyền Manager, LoadData
```csharp
  13 |         public MembersPage()
  14 |         {
  15 |             InitializeComponent();
  16 |             _db = new();
  17 |             GridHelper.Setup(dgMembers);
  18 | 
  19 |             // Manager chỉ xem: ẩn nút mua gói và khung đăng ký member
  20 |             if (Session.Role == "Manager")
  21 |             {
  22 |                 btnBuy.Visibility = Visibility.Collapsed;
  23 |                 grpAdd.Visibility = Visibility.Collapsed;
  24 |             }
  25 | 
  26 |             LoadData();
  27 |         }
  28 | 
  29 |         private void LoadData()
  30 |         {
  31 |             LoadStatus();
  32 |             LoadMembers();
  33 |         }
  34 | 
  35 |         // Ô lọc trạng thái gói
  36 |         private void LoadStatus()
  37 |         {
  38 |             cbxStatus.ItemsSource = new List<string> { "Tất cả", "Còn hạn", "Hết hạn", "Chưa có gói", "Đã hủy" };
  39 |             cbxStatus.SelectedIndex = 0;
  40 |         }
```
- **15-17:** Khuôn hàm tạo như các màn khác.
- **19-24:** `Session.Role == "Manager"`: Manager **chỉ xem**, nên ẩn nút "Mua / gia hạn gói" và khung đăng ký member. Bán gói và đăng ký tại quầy là việc của lễ tân.
- **26:** Gọi `LoadData()`.
- **29-33:** `LoadData()` gom 2 hàm: nạp ô lọc trạng thái, rồi nạp danh sách.
- **36-40:** `LoadStatus`: ô lọc trạng thái gói có 5 lựa chọn, mặc định "Tất cả".

#### LoadMembers, phần 1: lọc từ khóa, lấy dữ liệu
```csharp
  42 |         // Hiện danh sách member (có lọc theo từ khóa và trạng thái gói)
  43 |         private void LoadMembers()
  44 |         {
  45 |             // 1. Lọc theo từ khóa
  46 |             var query = _db.Members.AsQueryable();
  47 |             string keyword = txtSearch.Text.Trim();
  48 |             if (keyword != "")
  49 |             {
  50 |                 query = query.Where(m => m.FullName.Contains(keyword)
  51 |                     || (m.Phone ?? "").Contains(keyword)
  52 |                     || (m.Email ?? "").Contains(keyword));
  53 |             }
  54 |             var members = query.OrderBy(m => m.MemberId).ToList();
  55 |             var subs = _db.Subscriptions.Include(s => s.Package).ToList();
```
- **46-47:** Bắt đầu `AsQueryable()` rồi lấy từ khóa.
- **48-53:** Có từ khóa thì tìm trong họ tên, số điện thoại **hoặc** email. `Contains` thành `LIKE %keyword%` trong SQL. `(m.Phone ?? "")` vì Phone, Email có thể null.
- **54:** Chạy SQL, lấy danh sách member theo thứ tự `MemberId`.
- **55:** Lấy **tất cả gói** kèm tên gói (một lần). Sau đó ghép với từng member trong bộ nhớ để khỏi hỏi DB cho mỗi member.

#### LoadMembers, phần 2: ghép trạng thái gói
```csharp
  57 |             // 2. Với mỗi member: lấy gói hiện tại (hoặc gần nhất) để hiện trạng thái
  58 |             var rows = members.Select(m =>
  59 |             {
  60 |                 var sub = Rules.CurrentOrLatest(subs.Where(x => x.MemberId == m.MemberId));
  61 |                 return new
  62 |                 {
  63 |                     m.MemberId,
  64 |                     m.FullName,
  65 |                     m.Phone,
  66 |                     HasAccount = m.UserId != null ? "Có" : "Chưa có",
  67 |                     PackageName = sub?.Package.PackageName ?? "—",
  68 |                     EndDate = sub?.EndDate,
  69 |                     Status = Rules.SubscriptionText(sub)
  70 |                 };
  71 |             }).ToList();
  72 | 
  73 |             // 3. Lọc theo trạng thái (phải lọc sau vì trạng thái do code tính, không phải cột trong DB)
  74 |             if (cbxStatus.SelectedItem is string status && status != "Tất cả")
  75 |             {
  76 |                 rows = rows.Where(r => r.Status == status).ToList();
  77 |             }
  78 | 
  79 |             dgMembers.ItemsSource = rows;
  80 |         }
```
- **60:** Với mỗi member, lấy các gói của riêng họ rồi `CurrentOrLatest` chọn một gói đại diện (còn hạn, không thì gần nhất).
- **66:** `m.UserId != null ? "Có" : "Chưa có"`: có tài khoản đăng nhập chưa.
- **67-68:** `sub?.Package.PackageName ?? "—"`: không có gói thì hiện dấu "—". `sub?.EndDate` là null thì ô trống.
- **69:** `SubscriptionText(sub)` ra "Còn hạn / Hết hạn / Chưa có gói / Đã hủy".
- **73-77:** Lọc theo trạng thái được chọn. **Lọc sau `ToList()`** vì `Status` do hàm C# tính ra, không phải cột trong DB nên SQL không lọc được (khác với từ khóa lọc ở dòng 50-52).
- **79:** Đổ danh sách vào bảng.

#### Tìm kiếm, xóa lọc, chuyển sang mua gói
```csharp
  82 |         private void btnSearch_Click(object sender, RoutedEventArgs e)
  83 |         {
  84 |             LoadMembers();
  85 |         }
  86 | 
  87 |         private void btnClearFilter_Click(object sender, RoutedEventArgs e)
  88 |         {
  89 |             txtSearch.Clear();
  90 |             cbxStatus.SelectedIndex = 0;
  91 |             LoadMembers();
  92 |         }
  93 | 
  94 |         // Chuyển sang màn Mua / gia hạn gói với member đang chọn
  95 |         private void btnBuyPackage_Click(object sender, RoutedEventArgs e)
  96 |         {
  97 |             if (GridHelper.SelectedId(dgMembers, "MemberId") is not int memberId)
  98 |             {
  99 |                 MessageBox.Show("Vui lòng chọn một member trong danh sách.");
 100 |                 return;
 101 |             }
 102 |             MainWindow.Instance?.ShowPage(new BuyPage(memberId));
 103 |         }
```
- **82-85:** `btnSearch_Click`: gọi lại `LoadMembers()` (ô từ khóa và ô trạng thái đã có giá trị).
- **87-92:** `btnClearFilter_Click`: xóa ô lọc rồi tải lại.
- **97-101:** `GridHelper.SelectedId(...) is not int memberId`: chưa chọn member thì cảnh báo và thoát; qua khỏi `if` thì `memberId` là số nguyên.
- **102:** `MainWindow.Instance?.ShowPage(new BuyPage(memberId))`: đổi sang màn Mua gói và **truyền sẵn member** để màn đó chọn sẵn. `MainWindow.Instance` là cửa sổ chính; `?.` phòng khi nó null.

#### btnAddMember_Click, chặng 1: kiểm tra
```csharp
 105 |         // Đăng ký member tại quầy
 106 |         private void btnAddMember_Click(object sender, RoutedEventArgs e)
 107 |         {
 108 |             // 1. Kiểm tra dữ liệu nhập
 109 |             string fullName = txtFullName.Text.Trim();
 110 |             string phone = txtPhone.Text.Trim();
 111 |             string email = txtEmail.Text.Trim();
 112 | 
 113 |             if (fullName == "" || phone == "")
 114 |             {
 115 |                 MessageBox.Show("Vui lòng nhập Họ tên và Điện thoại.");
 116 |                 return;
 117 |             }
 118 |             string? nameError = AccountHelper.FullNameError(fullName);
 119 |             if (nameError != null)
 120 |             {
 121 |                 MessageBox.Show(nameError);
 122 |                 return;
 123 |             }
 124 |             if (!Regex.IsMatch(phone, @"^0\d{9,10}$"))
 125 |             {
 126 |                 MessageBox.Show("Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0.");
 127 |                 return;
 128 |             }
 129 |             if (email != "" && !AccountHelper.IsValidEmail(email))
 130 |             {
 131 |                 MessageBox.Show("Email chưa đúng định dạng (ví dụ: ten@mail.com). Có thể để trống.");
 132 |                 return;
 133 |             }
```
- **109-111:** Lấy 3 ô nhập, cắt khoảng trắng.
- **113-117:** Họ tên và SĐT là bắt buộc.
- **118-123:** `FullNameError` trả về câu lỗi hoặc `null`. Có lỗi thì báo.
- **124-128:** Regex `^0\d{9,10}$`: `^` đầu chuỗi, số `0`, rồi 9 đến 10 chữ số, `$` hết chuỗi (tổng 10-11 chữ số).
- **129-133:** Email có thể để trống, nhưng nếu nhập phải đúng dạng.

#### btnAddMember_Click, chặng 2 và 3: chống trùng, tạo member, ClearForm
```csharp
 135 |             try
 136 |             {
 137 |                 // 2. Mỗi số điện thoại chỉ thuộc về một member
 138 |                 var owner = _db.Members.FirstOrDefault(m => m.Phone == phone);
 139 |                 if (owner != null)
 140 |                 {
 141 |                     string loai = owner.UserId != null ? "đã có tài khoản" : "đăng ký tại quầy";
 142 |                     MessageBox.Show("Số điện thoại " + phone + " đã thuộc về member \"" + owner.FullName + "\" (#" + owner.MemberId + ", " + loai + ").\n\n"
 143 |                         + "Mỗi số điện thoại chỉ dùng cho một member, hãy dùng hồ sơ đó trong danh sách.");
 144 |                     txtSearch.Text = phone;     // lọc sẵn danh sách để thấy hồ sơ cũ
 145 |                     cbxStatus.SelectedIndex = 0;
 146 |                     LoadMembers();
 147 |                     return;
 148 |                 }
 149 | 
 150 |                 // 3. Thêm member
 151 |                 var member = new Member
 152 |                 {
 153 |                     FullName = fullName,
 154 |                     Phone = phone,
 155 |                     Email = email == "" ? null : email,
 156 |                     CreatedAt = DateTime.Now
 157 |                 };
 158 |                 _db.Members.Add(member);
 159 |                 _db.SaveChanges();
 160 |                 AuditHelper.Log(_db, "CreateMember", "Đăng ký member " + fullName + " tại quầy", "Members", member.MemberId);
 161 | 
 162 |                 LoadMembers();
 163 |                 ClearForm();
 164 |                 MessageBox.Show("Đã đăng ký member mới tại quầy.");
 165 |             }
 166 |             catch (Exception ex)
 167 |             {
 168 |                 MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
 169 |             }
 170 |         }
 171 | 
 172 |         // Xóa các ô nhập của form đăng ký member
 173 |         private void ClearForm()
 174 |         {
 175 |             txtFullName.Clear();
 176 |             txtPhone.Clear();
 177 |             txtEmail.Clear();
 178 |         }
 179 |     }
```
- **138-148:** **Chống trùng số điện thoại:** tìm member nào đang dùng số này (`FirstOrDefault`). Có thì báo rõ ai đang giữ số (họ tên, mã, đã có tài khoản hay đăng ký tại quầy), tự điền số vào ô tìm kiếm để danh sách hiện sẵn hồ sơ cũ, rồi `return`.
- **151-157:** Tạo object `Member`. `email == "" ? null : email`: email rỗng thì lưu `null`.
- **158-159:** `Add` rồi `SaveChanges()`: lúc này DB mới cấp `MemberId`.
- **160:** Ghi log "Đăng ký member ... tại quầy" (cần `member.MemberId` nên đặt sau `SaveChanges`).
- **162-164:** Tải lại danh sách, `ClearForm()` xóa 3 ô nhập, báo thành công. Gọi `LoadMembers()` chứ không gọi `LoadData()`, vì `LoadData()` sẽ đặt lại ô lọc trạng thái.
- **173-178:** `ClearForm()`: xóa các ô họ tên, số điện thoại, email.
- **mẫu chung:** Hai lớp chống trùng SĐT: code báo lỗi thân thiện, còn unique index `UX_Members_Phone` ở DB là lớp chặn cuối.

---

## 7. BuyPage

**`Pages/BuyPage.xaml.cs`** (226 dòng): Màn **Mua / gia hạn gói**: chọn member (nhân viên) hoặc tự mua (Member), chọn gói, xem trước hiệu lực và giá, xác nhận đã thu tiền. Tạo gói và hóa đơn trong một transaction.

#### Hai hàm tạo, LoadData, LoadMethod
```csharp
  12 |         // Mở từ menu: chưa chọn sẵn member
  13 |         public BuyPage() : this(null)
  14 |         {
  15 |         }
  16 | 
  17 |         // Mở từ nút "Mua / gia hạn gói" của màn Member: member được chọn sẵn
  18 |         public BuyPage(int? memberId)
  19 |         {
  20 |             InitializeComponent();
  21 |             _db = new();
  22 |             GridHelper.Setup(dgPackages);
  23 |             LoadData(memberId);
  24 |         }
  25 | 
  26 |         private void LoadData(int? memberId)
  27 |         {
  28 |             LoadMethod();
  29 |             LoadMember(memberId);
  30 |             LoadPackages();
  31 |             ShowCurrent();      // hiện sẵn thông tin ban đầu
  32 |             ShowPreview();
  33 |         }
  34 | 
  35 |         // Ô chọn phương thức thanh toán
  36 |         private void LoadMethod()
  37 |         {
  38 |             cbxMethod.ItemsSource = Rules.MethodList;
  39 |             cbxMethod.DisplayMemberPath = "Name";      // ô hiện chữ (vd "Tiền mặt")
  40 |             cbxMethod.SelectedValuePath = "Key";       // giá trị thật (vd "Cash")
  41 |             cbxMethod.SelectedIndex = 0;
  42 |         }
```
- **13-15:** Hàm tạo không tham số, gọi sang hàm tạo bên dưới với `null`. Dùng khi mở màn từ menu mà chưa chọn member.
- **18:** Hàm tạo có `memberId`: dùng khi bấm "Mua / gia hạn gói" từ màn Member (member được chọn sẵn).
- **20-23:** Khuôn quen thuộc, rồi `LoadData(memberId)`.
- **26-33:** `LoadData` gom các bước nạp: phương thức thanh toán, ô chọn member, bảng gói; `ShowCurrent` và `ShowPreview` hiện sẵn thông tin ban đầu.
- **38:** `Rules.MethodList`: danh sách phương thức thanh toán.
- **39:** `DisplayMemberPath = "Name"`: ô **hiện chữ** ("Tiền mặt").
- **40:** `SelectedValuePath = "Key"`: **giá trị thật** lấy ra là mã ("Cash"). Một combo có hai mặt: chữ hiển thị và giá trị thật.

#### Chọn member: LoadMember và GetMemberId
```csharp
  44 |         private void LoadMember(int? memberId)
  45 |         {
  46 |             // Member đăng nhập thì tự mua cho mình, không cần ô chọn member
  47 |             if (Session.Role == "Member")
  48 |             {
  49 |                 panelMember.Visibility = Visibility.Collapsed;
  50 |                 return;
  51 |             }
  52 | 
  53 |             cbxMember.ItemsSource = _db.Members.OrderBy(m => m.MemberId).ToList();
  54 |             cbxMember.DisplayMemberPath = nameof(Member.FullName);
  55 |             cbxMember.SelectedValuePath = nameof(Member.MemberId);
  56 |             if (memberId != null)
  57 |             {
  58 |                 cbxMember.SelectedValue = memberId;
  59 |             }
  60 |             if (cbxMember.SelectedIndex < 0)
  61 |             {
  62 |                 cbxMember.SelectedIndex = 0;
  63 |             }
  64 |         }
  65 | 
  66 |         // Đang mua cho ai: Member thì là chính mình, nhân viên thì là người đang chọn (0 = chưa chọn)
  67 |         private int GetMemberId()
  68 |         {
  69 |             if (Session.Role == "Member")
  70 |             {
  71 |                 return Session.CurrentMember?.MemberId ?? 0;
  72 |             }
  73 |             if (cbxMember.SelectedItem is not Member member)
  74 |             {
  75 |                 return 0;
  76 |             }
  77 |             return member.MemberId;
  78 |         }
```
- **47-51:** Member đăng nhập thì **ẩn ô chọn member** (họ tự mua cho mình).
- **53:** Nhân viên: nạp danh sách member vào combo.
- **54-55:** `nameof(Member.FullName)` cho ra chuỗi `"FullName"` nhưng được trình biên dịch kiểm tra (đổi tên thuộc tính thì báo lỗi ngay). Hiện họ tên, giá trị thật là `MemberId`.
- **56-63:** Có member được truyền vào thì chọn sẵn; không thì chọn người đầu.
- **66-78:** `GetMemberId()`: **"đang mua cho ai"**. Member thì là chính mình (`Session.CurrentMember`). Nhân viên thì lấy mục đang chọn trong combo: `cbxMember.SelectedItem is not Member member` nghĩa là chưa chọn gì thì trả `0` (quy ước: 0 = chưa chọn), ngược lại trả `member.MemberId`.

#### LoadPackages: chỉ gói đang bán
```csharp
  80 |         // Chỉ hiện các gói đang bán
  81 |         private void LoadPackages()
  82 |         {
  83 |             dgPackages.ItemsSource = _db.MembershipPackages
  84 |                 .Where(p => p.IsActive)
  85 |                 .OrderBy(p => p.PackageId)
  86 |                 .Select(p => new { p.PackageId, p.PackageName, p.DurationDays, p.Price })
  87 |                 .ToList();
  88 |         }
```
- **84:** Giống `PackagesPage` nhưng thêm `.Where(p => p.IsActive)`: **chỉ hiện gói đang bán** (luật P1). Không cần cột `Status`.

#### ShowCurrent: dòng chữ "member đang có gói gì"
```csharp
  90 |         // Dòng chữ: member đang có gói gì
  91 |         private void ShowCurrent()
  92 |         {
  93 |             int memberId = GetMemberId();
  94 |             var member = _db.Members.FirstOrDefault(m => m.MemberId == memberId);
  95 |             if (member == null)
  96 |             {
  97 |                 txtCurrent.Text = "Chưa chọn member.";
  98 |                 return;
  99 |             }
 100 | 
 101 |             var current = Rules.ActiveSubscription(_db, memberId);
 102 |             if (current == null)
 103 |             {
 104 |                 txtCurrent.Text = member.FullName + " · chưa có gói còn hạn";
 105 |                 return;
 106 |             }
 107 | 
 108 |             string pkgName = _db.MembershipPackages.Find(current.PackageId)!.PackageName;
 109 |             txtCurrent.Text = member.FullName + " · đang có " + pkgName + ", còn hạn đến " + current.EndDate.ToString("dd/MM/yyyy");
 110 | 
 111 |             // Các gói đã mua nối tiếp (chưa tới ngày bắt đầu)
 112 |             var queued = Rules.QueuedSubscriptions(_db, memberId);
 113 |             if (queued.Count > 0)
 114 |             {
 115 |                 txtCurrent.Text += "\nĐã mua nối tiếp: " + string.Join("; ",
 116 |                     queued.Select(q => q.Package.PackageName + " (" + q.StartDate.ToString("dd/MM/yyyy") + " → " + q.EndDate.ToString("dd/MM/yyyy") + ")"));
 117 |             }
 118 |         }
```
- **93-99:** Lấy member; chưa chọn thì hiện "Chưa chọn member." rồi thoát.
- **101-106:** `ActiveSubscription`: gói đang dùng (xem mục Rules.Packages). Không có thì hiện "chưa có gói còn hạn" rồi thoát.
- **108-109:** Có gói đang dùng: lấy tên gói, ghép chuỗi "Tên · đang có Gói X, còn hạn đến dd/MM/yyyy". `ToString("dd/MM/yyyy")` định dạng ngày.
- **112-117:** Có gói mua nối tiếp thì thêm dòng "Đã mua nối tiếp: ...". `\n` là xuống dòng. `string.Join("; ", ...)` ghép các phần tử, giữa mỗi cặp là "; ". `q.Package.PackageName` có được nhờ `Include` trong `QueuedSubscriptions`.

#### ShowPreview và hai hàm sự kiện
```csharp
 120 |         // Xem trước: gói đang chọn có hiệu lực từ ngày nào đến ngày nào, bao nhiêu tiền
 121 |         private void ShowPreview()
 122 |         {
 123 |             int memberId = GetMemberId();
 124 |             if (GridHelper.SelectedId(dgPackages, "PackageId") is not int packageId || memberId == 0)
 125 |             {
 126 |                 txtPreview.Text = "Chọn một gói ở trên.";
 127 |                 btnConfirm.IsEnabled = false;
 128 |                 return;
 129 |             }
 130 | 
 131 |             var pkg = _db.MembershipPackages.Find(packageId)!;
 132 |             var start = Rules.NextStartDate(_db, memberId);
 133 |             var end = start.AddDays(pkg.DurationDays - 1);
 134 | 
 135 |             txtPreview.Text = pkg.PackageName + ": hiệu lực " + start.ToString("dd/MM/yyyy") + " → " + end.ToString("dd/MM/yyyy");
 136 |             if (start > Rules.Today)
 137 |             {
 138 |                 txtPreview.Text += " (nối tiếp: dùng sau khi gói hiện có hết hạn)";
 139 |             }
 140 |             txtPreview.Text += "\nSố tiền: " + pkg.Price.ToString("N0") + " ₫";
 141 |             btnConfirm.IsEnabled = true;
 142 |         }
 143 | 
 144 |         private void cbxMember_SelectionChanged(object sender, SelectionChangedEventArgs e)
 145 |         {
 146 |             ShowCurrent();
 147 |             ShowPreview();
 148 |         }
 149 | 
 150 |         private void dgPackages_SelectionChanged(object sender, SelectionChangedEventArgs e)
 151 |         {
 152 |             ShowPreview();
 153 |         }
```
- **123-129:** Thiếu gói được chọn hoặc member thì hiện "Chọn một gói ở trên." và **khóa nút Xác nhận** (`IsEnabled = false`). Cú pháp `... is not int packageId || memberId == 0`: không chọn gói, hoặc chưa có member.
- **131:** `Find(packageId)!`: lấy đúng một gói theo khóa chính.
- **132:** `NextStartDate`: ngày bắt đầu theo luật P3 (nối tiếp hoặc hôm nay).
- **133:** `start.AddDays(pkg.DurationDays - 1)`: **trừ 1** vì ngày bắt đầu đã tính là ngày thứ nhất. Gói 30 ngày bắt đầu 01/12 thì ngày dùng cuối là 30/12; không trừ 1 sẽ thành 31 ngày.
- **135-140:** Ghép chuỗi xem trước. Dòng 136: `start > Rules.Today` nghĩa là bắt đầu sau hôm nay, tức **mua nối tiếp**, nên thêm chú thích. `pkg.Price.ToString("N0")` định dạng 1.500.000.
- **141:** Mở nút Xác nhận.
- **144-153:** Đổi member hoặc chọn dòng gói khác thì gọi lại `ShowCurrent()` và/hoặc `ShowPreview()`. Các hàm này được gắn trong file `.xaml` (ví dụ `SelectionChanged="cbxMember_SelectionChanged"`).

#### btnConfirmPayment_Click, chặng 1: kiểm tra (trái tim của phần bạn)
```csharp
 155 |         // Xác nhận đã thu tiền: tạo gói của member và hóa đơn
 156 |         private void btnConfirmPayment_Click(object sender, RoutedEventArgs e)
 157 |         {
 158 |             // 1. Kiểm tra
 159 |             int memberId = GetMemberId();
 160 |             if (memberId == 0)
 161 |             {
 162 |                 MessageBox.Show("Vui lòng chọn member.");
 163 |                 return;
 164 |             }
 165 |             if (GridHelper.SelectedId(dgPackages, "PackageId") is not int packageId)
 166 |             {
 167 |                 MessageBox.Show("Vui lòng chọn một gói.");
 168 |                 return;
 169 |             }
 170 |             if (cbxMethod.SelectedValue is not string method)
 171 |             {
 172 |                 MessageBox.Show("Vui lòng chọn phương thức thanh toán.");
 173 |                 return;
 174 |             }
 175 | 
 176 |             try
 177 |             {
 178 |                 var pkg = _db.MembershipPackages.Find(packageId)!;
 179 |                 if (!pkg.IsActive)
 180 |                 {
 181 |                     MessageBox.Show("Gói này đã ngừng bán.");
 182 |                     return;
 183 |                 }
```
- **156:** Tên hàm theo kiểu template Q2: `btnConfirmPayment_Click` (nút "Xác nhận đã thu tiền").
- **159-164:** Chưa chọn member (`GetMemberId() == 0`) thì cảnh báo và thoát.
- **165-169:** `... is not int packageId`: chưa chọn gói thì cảnh báo và thoát; qua khỏi `if` thì `packageId` là số nguyên.
- **170-174:** `cbxMethod.SelectedValue is not string method`: chưa chọn phương thức thanh toán thì cảnh báo; qua khỏi `if` thì `method` là mã ("Cash"/"BankTransfer"/"Card").
- **178:** `Find(packageId)!`: lấy gói theo khóa chính.
- **179-183:** **Luật P1, kiểm tra lại ngay trước khi lưu:** nếu gói đã ngừng bán (ví dụ vừa bị Manager ngừng bán lúc đang mở màn) thì báo "Gói này đã ngừng bán." và thoát.

#### btnConfirmPayment_Click, chặng 2: dựng hai object
```csharp
 185 |                 // 2. Tạo gói (bắt đầu hôm nay, hoặc nối tiếp sau gói hiện có) và hóa đơn
 186 |                 var start = Rules.NextStartDate(_db, memberId);
 187 |                 var sub = new Subscription
 188 |                 {
 189 |                     MemberId = memberId,
 190 |                     PackageId = pkg.PackageId,
 191 |                     StartDate = start,
 192 |                     EndDate = start.AddDays(pkg.DurationDays - 1),
 193 |                     Status = "Active"
 194 |                 };
 195 |                 var pay = new Payment
 196 |                 {
 197 |                     InvoiceNo = Rules.NextInvoiceNo(_db),
 198 |                     MemberId = memberId,
 199 |                     Subscription = sub,                     // gắn hóa đơn với gói; EF tự điền SubscriptionId khi lưu
 200 |                     Amount = pkg.Price,
 201 |                     PaymentMethod = method,
 202 |                     PaidAt = DateTime.Now,
 203 |                     ReceivedBy = Session.CurrentUser?.UserId,
 204 |                     Status = "Paid",
 205 |                     Note = "Mua " + pkg.PackageName
 206 |                 };
```
- **186:** Ngày bắt đầu theo luật P3.
- **187-194:** Object gói của member: `EndDate = start + DurationDays - 1`, `Status = "Active"`. **Chưa có `SubscriptionId`.**
- **197:** `NextInvoiceNo`: mã hóa đơn `INV-000n`, không trùng.
- **199:** **`Subscription = sub`**: gán **object**, không gán số Id. Khi `SaveChanges()`, EF hiểu `sub` phải được INSERT trước, lấy `SubscriptionId` mà SQL vừa cấp, điền vào `pay.SubscriptionId`, rồi mới INSERT `pay`. Bạn không phải tự lo thứ tự.
- **200:** `Amount = pkg.Price`: số tiền lấy từ giá gói, không nhập tay.
- **201:** `PaymentMethod = method`: mã lấy ở dòng 170.
- **203:** `ReceivedBy`: nhân viên đang đăng nhập thu tiền.
- **204-205:** `Status = "Paid"`, ghi chú "Mua ...".

#### btnConfirmPayment_Click, chặng 3: ghi và làm mới
```csharp
 208 |                 // Một lần SaveChanges lưu cả hai: lỗi thì không lưu gì cả
 209 |                 _db.Subscriptions.Add(sub);
 210 |                 _db.Payments.Add(pay);
 211 |                 _db.SaveChanges();
 212 | 
 213 |                 AuditHelper.Log(_db, "Payment", "Thu " + pay.InvoiceNo + " " + pay.Amount.ToString("N0") + " ₫", "Payments", pay.PaymentId);
 214 | 
 215 |                 // 3. Làm mới màn hình
 216 |                 ShowCurrent();
 217 |                 ShowPreview();
 218 |                 MessageBox.Show("Đã tạo gói và hóa đơn " + pay.InvoiceNo + ".");
 219 |             }
 220 |             catch (Exception ex)
 221 |             {
 222 |                 MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
 223 |             }
 224 |         }
```
- **209-211:** **Hai `Add` rồi một `SaveChanges()` = một transaction.** Tạo gói và hóa đơn cùng lúc; nếu INSERT hóa đơn lỗi thì gói cũng không được lưu. Không bao giờ có gói mà thiếu hóa đơn (hoặc ngược lại).
- **213:** `AuditHelper.Log` nằm **sau** và lưu riêng (nó tự gọi `SaveChanges`). Dùng được `pay.PaymentId` vì lúc này SQL đã cấp Id. Nếu ghi log lỗi thì giao dịch mua vẫn đã lưu.
- **216-218:** Làm mới hai dòng thông tin (lúc này đã thấy gói vừa mua) và báo thành công kèm số hóa đơn.
- **220-223:** `catch`: có lỗi thì hiện hộp lỗi; vì chưa `SaveChanges` thành công nên không có gì bị lưu.

> **4 điểm then chốt (hay bị hỏi)**
> 1. Dòng 199 gán **object** `Subscription = sub`, EF tự điền `SubscriptionId` khi lưu.
> 2. Dòng 209 đến 211: hai `Add` + một `SaveChanges()` = một transaction, nên không có gói thiếu hóa đơn.
> 3. Dòng 213: `Log` ghi sau, lưu riêng, dùng được `PaymentId` vì SQL đã cấp.
> 4. Nhân viên và Member đi cùng một đường này; phân biệt bằng `GetMemberId()`.

---

## 8. Ôn tập

### Demo (làm đúng như khi trình bày)

Menu theo vai trò: **Manager** thấy Thành viên và gói → Member + Gói thành viên, và Hệ thống → Lịch sử hoạt động. **Lễ tân** thấy Thành viên → Member + Mua / gia hạn gói. Vì vậy demo phải đổi qua lại giữa hai tài khoản (mật khẩu đều là `123456`).

1. Chạy `SportsCenter.sql`, mở app, đăng nhập `manager1`.
2. **Thành viên và gói → Gói thành viên**: thêm một gói mới (thử nhập sai số ngày để thấy cảnh báo), bấm "Ngừng bán / Mở bán lại".
3. **Member** (vẫn là manager): tìm theo tên, lọc theo "Còn hạn". Thấy **không có** nút mua và khung đăng ký (Manager chỉ xem).
4. Đăng xuất, đăng nhập `reception1`. **Member**: đăng ký một member mới tại quầy; thử đăng ký lại cùng số điện thoại để thấy cảnh báo.
5. Chọn một member → **Mua / gia hạn gói** → mua một gói, đọc dòng xem trước. Mua **lần nữa** → thấy "(nối tiếp ...)" và dòng "Đã mua nối tiếp".
6. Đăng nhập lại `manager1` → **Hệ thống → Lịch sử hoạt động**: thấy "Tạo gói", "Thanh toán", "Đăng ký tại quầy"; lọc theo người, hành động, khoảng ngày.

### Câu hay bị hỏi

- **Gói nối tiếp tính ngày thế nào?** Lấy gói chưa hết hạn có `EndDate` xa nhất, cộng 1 ngày (`NextStartDate`). Không còn gói nào thì bắt đầu hôm nay. Ngày kết thúc = bắt đầu + số ngày − 1.
- **Vì sao không xóa gói?** `Subscriptions` còn trỏ tới gói (khóa ngoại) và cần giữ lịch sử. Chỉ ngừng bán (`IsActive = false`); gói ngừng bán không hiện ở màn mua.
- **Vì sao Manager không bán gói?** Phân quyền: Manager chỉ xem; lễ tân bán và thu tiền. Code ẩn nút bằng `Session.Role == "Manager"`.
- **Mua gói đảm bảo toàn vẹn thế nào?** Một `SaveChanges()` cho cả `Subscription` và `Payment` (một transaction), nên không có chuyện có gói mà thiếu hóa đơn.
- **Trùng số điện thoại chặn ở đâu?** Hai lớp: code (`FirstOrDefault(m => m.Phone == phone)` trong `btnAddMember_Click`) báo lỗi thân thiện, và unique index `UX_Members_Phone` ở DB là lớp chặn cuối.

### Tự kiểm tra

Trả lời trong đầu rồi bấm để xem đáp án.

1. Trong `btnAddPackage_Click`, nếu đảo `AuditHelper.Log` (dòng 105) lên trước `SaveChanges()` (dòng 104), `pkg.PackageId` bằng bao nhiêu và vì sao?
   - Đáp án: **Bằng 0.** `PackageId` là cột `IDENTITY`: số do SQL Server cấp lúc `INSERT`. Trước `SaveChanges()` chưa có `INSERT`, nên object vẫn giữ giá trị mặc định 0, và log sẽ ghi `EntityId = 0` (sai).
2. Vì sao `NextStartDate` lấy gói có `EndDate` xa nhất chứ không lấy gói đang dùng?
   - Đáp án: Vì member có thể đã mua nối tiếp nhiều gói. Gói mới phải bắt đầu sau gói **cuối cùng** trong chuỗi, nếu chỉ lấy gói đang dùng thì gói mới sẽ chồng lên gói đã mua nối tiếp.
3. Vì sao `LoadMembers` lọc trạng thái sau `ToList()` còn lọc từ khóa thì trước?
   - Đáp án: Từ khóa so với cột có thật trong DB (`FullName`, `Phone`, `Email`) nên SQL lọc được. Trạng thái do hàm C# `SubscriptionText` tính từ ngày, không có cột nào trong DB, nên phải lọc ở bộ nhớ sau khi đã `ToList()`.
4. Vì sao `AuditPage` lọc hành động **sau** khi đã đổi mã sang tên tiếng Việt?
   - Đáp án: Ô chọn hiện tên tiếng Việt ("Thanh toán") còn DB lưu mã ("Payment"). Hàm đổi tên `Rules.ActionName` là hàm C#, SQL không dịch được, nên phải `ToList()` trước, đổi tên, rồi so tên được chọn với cột `Action` đã đổi.
5. Ở `btnConfirmPayment_Click`, hóa đơn lấy `SubscriptionId` ở đâu khi dòng `Subscription` chưa tồn tại lúc tạo object `pay`?
   - Đáp án: Từ `Subscription = sub`. Khi `SaveChanges()`, EF INSERT `sub` trước, nhận `SubscriptionId` SQL vừa cấp, điền vào `pay.SubscriptionId`, rồi INSERT `pay`. Tất cả trong một transaction.
6. Nếu bấm Xác nhận khi gói vừa bị người khác ngừng bán, chuyện gì xảy ra và dòng nào xử lý?
   - Đáp án: Dòng 179 đến 183: `if (!pkg.IsActive)` báo "Gói này đã ngừng bán." rồi `return`, không tạo gói và hóa đơn nào.
7. Vì sao `ShowPreview` tính `AddDays(pkg.DurationDays - 1)`?
   - Đáp án: Ngày bắt đầu đã là ngày thứ nhất của gói. Gói 30 ngày bắt đầu 01/12 dùng đến hết 30/12 (đủ 30 ngày). Không trừ 1 sẽ thành 31 ngày.
8. Dòng `if (GridHelper.SelectedId(dgPackages, "PackageId") is not int id)` làm gì, và sau khối `if` thì `id` là gì?
   - Đáp án: Nếu chưa chọn dòng nào, `SelectedId` trả `null`, không phải `int`, nên vào khối `if` (cảnh báo rồi `return`). Chạy qua được khối `if` nghĩa là đã chọn dòng, và `id` là số nguyên `PackageId` của dòng đó, dùng thẳng được.

---
