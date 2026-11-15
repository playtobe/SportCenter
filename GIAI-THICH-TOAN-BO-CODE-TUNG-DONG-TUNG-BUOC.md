# HƯỚNG DẪN & GIẢI THÍCH TOÀN BỘ CODE TỪNG DÒNG TỪNG BƯỚC
## HỆ THỐNG QUẢN LÝ TRUNG TÂM THỂ THAO (SPORTS CENTER MANAGEMENT SYSTEM)

> **Công nghệ áp dụng**: C# .NET 8, WPF (Windows Presentation Foundation), Entity Framework Core 8 (EF Core 8), SQL Server.
> **Kiến trúc**: Single Project (`SportsCenterApp/SportsCenter`), Code-Behind hướng sự kiện thực dụng, Database First / Code First đồng bộ, Rules Engine tách partial class theo nghiệp vụ.

---

# MỤC LỤC TỔNG THỂ

1. [PHẦN 1: TỔNG QUAN KIẾN TRÚC & CƠ CHẾ VẬN HÀNH](#phần-1-tổng-quan-kiến-trúc--cơ-chế-vận-hành)
   - 1.1. Luồng chuỗi nghiệp vụ xuyên suốt
   - 1.2. 4 vai trò người dùng trong hệ thống
   - 1.3. Vòng đời khởi chạy ứng dụng (Lifecycle)
   - 1.4. Quản lý trạng thái phiên đăng nhập (`Session`)
   - 1.5. Cấu hình kết nối CSDL & Chu kỳ sống `DbContext`
2. [PHẦN 2: THIẾT KẾ CƠ SỞ DỮ LIỆU & CÁC ENTITIES](#phần-2-thiết-kế-cơ-sở-dữ-liệu--các-entities)
   - 2.1. Phân tích kịch bản CSDL `SportsCenter.sql` & Ràng buộc toàn vẹn
   - 2.2. Lớp kết nối CSDL `SportsCenterContext.cs`
   - 2.3. Chi tiết 13 lớp thực thể (Entities)
3. [PHẦN 3: GIẢI THÍCH CÁC LỚP TIỆN ÍCH (HELPERS) & QUY TẮC NGHIỆP VỤ (RULES)](#phần-3-giải-thích-các-lớp-tiện-ích-helpers--quy-tắc-nghiệp-vụ-rules)
   - 3.1. `PasswordHelper.cs` – Băm mật khẩu chuẩn hóa SHA-256
   - 3.2. `Session.cs` – Lưu vết thông tin người dùng đang hoạt động
   - 3.3. `Msg.cs` – Đóng gói giao diện hộp thoại thông báo
   - 3.4. `GridHelper.cs` – Cấu hình tự động và định dạng hiển thị DataGrid
   - 3.5. `PagerControl.xaml.cs` – Thuật toán phân trang động
   - 3.6. `AuditHelper.cs` – Ghi nhận nhật ký kiểm toán hệ thống
   - 3.7. `AccountHelper.cs` – Chuẩn hóa chuỗi, xác thực tài khoản & thuật toán liên kết hồ sơ quầy
   - 3.8. `Rules.cs` (Partial lõi) – Hằng số ngày tháng, vai trò, phương thức và sinh số hóa đơn
   - 3.9. `Rules.Packages.cs` (Partial) – Kiểm tra gói hiệu lực & thuật toán gia hạn nối tiếp (P1-P5)
   - 3.10. `Rules.Classes.cs` (Partial) – Phát hiện xung đột lịch, phòng, HLV và điều kiện hủy lớp (C1-C4)
   - 3.11. `Rules.Booking.cs` (Partial) – Quy tắc đặt lớp, tính buổi học kế tiếp, sinh 12 buổi điểm danh & hủy đăng ký (B1-B8)
4. [PHẦN 4: GIẢI THÍCH TỪNG DÒNG CODE CÁC CỬA SỔ (WINDOWS)](#phần-4-giải-thích-từng-dòng-code-các-cửa-sổ-windows)
   - 4.1. `App.xaml` & `App.xaml.cs` – Định dạng văn hóa Việt Nam & Phong cách UI dùng chung
   - 4.2. `LoginWindow.xaml.cs` – Xác thực đăng nhập & Phân luồng điều hướng
   - 4.3. `RegisterWindow.xaml.cs` – Tự đăng ký tài khoản, phát hiện trùng SĐT & liên kết hồ sơ quầy
   - 4.4. `MainWindow.xaml.cs` – Điều hướng Menu động theo Role & Quản lý Accordion Expander
   - 4.5. `ChangePasswordWindow.xaml.cs` – Đổi mật khẩu tài khoản
5. [PHẦN 5: GIẢI THÍCH TỪNG DÒNG CODE CÁC TRANG CHỨC NĂNG (PAGES)](#phần-5-giải-thích-từng-dòng-code-các-trang-chức-năng-pages)
   - 5.1. `DashboardPage.xaml.cs` – Báo cáo tổng hợp, vẽ biểu đồ cột Canvas & cảnh báo hết hạn
   - 5.2. `UsersPage.xaml.cs` – Quản lý tài khoản, khóa/mở khóa, đổi vai trò & tạo mới tài khoản
   - 5.3. `MembersPage.xaml.cs` – Quản lý hội viên, đăng ký quầy & điều hướng mua gói
   - 5.4. `CoachPage.xaml.cs` – Huấn luyện viên xem lớp phụ trách & danh sách học viên
   - 5.5. `SportsRoomsPage.xaml.cs` – Quản lý danh mục Bộ môn & Phòng tập
   - 5.6. `PackagesPage.xaml.cs` – Thiết lập danh mục gói tập & mở/ngừng bán gói
   - 5.7. `ClassesPage.xaml.cs` – Xếp lịch lớp học, lọc ca trống chống xung đột & hủy lớp an toàn
   - 5.8. `BuyPage.xaml.cs` – Mua gói tập, xem trước ngày hiệu lực nối tiếp & sinh hóa đơn
   - 5.9. `BookingPage.xaml.cs` – Đăng ký tham gia lớp học & kiểm tra điều kiện gói tập
   - 5.10. `MySchedulePage.xaml.cs` – Thời khóa biểu tuần cá nhân & hiển thị kết quả điểm danh FAP
   - 5.11. `AttendancePage.xaml.cs` – Huấn luyện viên điểm danh buổi học & cập nhật trạng thái hai chiều
   - 5.12. `PaymentsPage.xaml.cs` – Quản lý hóa đơn thanh toán & xuất biên lai ra tập tin văn bản
   - 5.13. `AuditPage.xaml.cs` – Tra cứu nhật ký kiểm toán hệ thống
6. [PHẦN 6: BẢNG MA TRẬN ĐỐI CHIẾU MÃ LUẬT NGHIỆP VỤ (RULE MAPPING)](#phần-6-bảng-ma-trận-đối-chiếu-mã-luật-nghiệp-vụ-rule-mapping)

---

# PHẦN 1: TỔNG QUAN KIẾN TRÚC & CƠ CHẾ VẬN HÀNH

## 1.1. Luồng chuỗi nghiệp vụ xuyên suốt
Ứng dụng xây dựng một dòng chảy nghiệp vụ khép kín từ lúc khách hàng tiếp cận trung tâm cho đến khi tham gia tập luyện và quản lý thống kê:
```
[1. Đăng ký tài khoản] ──► [2. Mua gói thành viên] ──► [3. Đặt lớp học] ──► [4. Điểm danh buổi học] ──► [5. Quản lý Doanh thu & Báo cáo]
```
1. **Đăng ký tài khoản**: Khách hàng có thể tự đăng ký tài khoản online qua `RegisterWindow` hoặc đến trực tiếp tại quầy được Lễ tân tiếp nhận tạo hồ sơ trong `MembersPage`. Nếu khách hàng đã đăng ký tại quầy trước đó (chưa có tài khoản đăng nhập), khi tự đăng ký online hệ thống tự động phát hiện số điện thoại + họ tên trùng khớp và cho phép liên kết tài khoản vào hồ sơ cũ để giữ nguyên quyền lợi.
2. **Mua gói thành viên**: Thành viên (hoặc Lễ tân thao tác hộ) mua các gói hội viên có thời hạn (30 ngày, 90 ngày, 365 ngày...). Hệ thống hỗ trợ **mua gia hạn nối tiếp**: nếu gói cũ chưa hết hạn, gói mới sẽ tự động bắt đầu từ ngày liền kề sau ngày kết thúc của gói xa nhất.
3. **Đăng ký lớp học**: Thành viên phải sở hữu gói tập **còn hiệu lực** mới được phép đặt chỗ vào các lớp học theo lịch cố định hàng tuần (Thứ trong tuần + Ca học). Hệ thống kiểm tra nghiêm ngặt sĩ số phòng, trạng thái lớp, và chống trùng lịch học.
4. **Điểm danh buổi học**: Huấn luyện viên phụ trách lớp mở danh sách các buổi học hợp lệ trong quá khứ/hiện tại để tích chọn học viên "Có mặt" hoặc "Vắng".
5. **Thanh toán & Báo cáo**: Mỗi lần mua gói đều phát sinh một hóa đơn thanh toán mô phỏng (`INV-xxxx`) lưu vào CSDL. Quản lý trung tâm có thể theo dõi biểu đồ doanh thu 6 tháng, kiểm tra tỷ lệ lấp đầy lớp học và các gói sắp hết hạn trong 7 ngày tới.

---

## 1.2. 4 vai trò người dùng trong hệ thống

| Vai trò (Role) | Đối tượng đại diện | Quyền hạn và Trách nhiệm chính | Màn hình truy cập |
|---|---|---|---|
| **Manager** | Quản lý trung tâm | Toàn quyền kiểm soát danh mục và nhân sự: cấu hình bộ môn, phòng tập, gói thành viên, lịch lớp học; quản lý tài khoản người dùng; xem báo cáo doanh thu, biểu đồ tài chính, nhật ký kiểm toán. | Dashboard, Classes, SportsRooms, Members, Packages, Payments, Users, Audit |
| **Receptionist** | Nhân viên lễ tân | Đại diện quầy dịch vụ: đăng ký hồ sơ hội viên mới, thực hiện bán gói/gia hạn gói tập cho khách tại quầy, hỗ trợ hội viên đặt lớp học hoặc hủy đăng ký lớp khi có yêu cầu. | Members, Buy, Booking, Payments |
| **Member** | Hội viên trung tâm | Tự phục vụ: theo dõi thời khóa biểu cá nhân dạng ma trận tuần (kèm kết quả điểm danh), tự đăng ký lớp học, tự mua gói tập, xem lịch sử các hóa đơn thanh toán của riêng mình. | MySchedule, Booking, Buy, Payments |
| **Coach** | Huấn luyện viên | Quản lý chuyên môn: xem danh sách các lớp học mình được phân công phụ trách, xem danh sách học viên đăng ký, thực hiện điểm danh buổi học theo từng ngày. | Coach, Attendance |

---

## 1.3. Vòng đời khởi chạy ứng dụng (Lifecycle)
Quy trình thực thi từ khi kích hoạt ứng dụng đến khi hiển thị giao diện:

```
[Khởi chạy ứng dụng]
       │
       ▼
[App.xaml / App.xaml.cs] ──► Thiết lập văn hóa định dạng vi-VN (Ngày: dd/MM/yyyy, Tiền tệ: 1.000.000 ₫)
       │
       ▼
[LoginWindow.xaml]       ──► Người dùng nhập Username, Password
       │
       ├─ Kiểm tra tài khoản trong bảng Users
       ├─ Băm mật khẩu bằng SHA-256 đối chiếu PasswordHash
       ├─ Kiểm tra cờ IsActive == true (Tài khoản không bị khóa)
       │
       ▼
[Session.SignIn]         ──► Nạp thông tin Session.CurrentUser, nạp kèm CurrentMember hoặc CurrentCoach, ghi AuditLog("Login")
       │
       ▼
[MainWindow.xaml]        ──► Khởi tạo cửa sổ chính
       │
       ├─ Đọc Session.CurrentUser.Role
       ├─ Gọi BuildMenu(role) sinh danh sách nút bấm & Expander tương ứng
       └─ Kích hoạt trang mặc định đầu tiên (_firstPage) đưa vào ContentControl (contentHost)
```

---

## 1.4. Quản lý trạng thái phiên đăng nhập (`Session`)
- Lớp `Session` là một `static class` đóng vai trò là kho lưu trữ ngữ cảnh bảo mật trong toàn bộ vòng đời ứng dụng.
- Sau khi xác thực danh tính tại `LoginWindow`, `Session.SignIn(db, user)` sẽ nạp thực thể `User` vào `CurrentUser`. Nếu là Member, truy vấn tiếp hồ sơ `Members` tương ứng gắn vào `CurrentMember`. Nếu là Coach, gắn vào `CurrentCoach`.
- Khi người dùng bấm "Đăng xuất" hoặc đổi tài khoản, hàm `Session.Clear()` dọn dẹp các con trỏ ngữ cảnh về `null` trước khi quay trở lại `LoginWindow`.

---

## 1.5. Cấu hình kết nối CSDL & Chu kỳ sống `DbContext`
- Chuỗi kết nối được tách biệt hoàn toàn trong tập tin `appsettings.json`, giúp chuyển đổi môi trường triển khai linh hoạt mà không cần biên dịch lại mã nguồn:
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "server=(local);uid=sa;pwd=12345;database=SportsCenter;TrustServerCertificate=True;"
    }
  }
  ```
- Phương thức `OnConfiguring` trong `SportsCenterContext` sử dụng `ConfigurationBuilder` nạp động tập tin cấu hình tại thư mục chạy `AppContext.BaseDirectory`.
- **Triết lý quản lý DbContext**: Mỗi màn hình (Window hoặc UserControl Page) tự khởi tạo và nắm giữ một thể hiện `_db = new SportsCenterContext()`. Khi truy vấn danh sách để hiển thị trên DataGrid, hệ thống tận dụng tối đa cơ chế `.AsNoTracking()` hoặc dùng toán tử chiếu `.Select(new { ... })`. Điều này loại bỏ hoàn toàn chi phí theo dõi trạng thái bộ nhớ đệm (Change Tracker) của EF Core, đảm bảo dữ liệu hiển thị luôn là dữ liệu mới nhất từ CSDL SQL Server và tránh xung đột khi nhiều màn hình cùng thao tác.

---

# PHẦN 2: THIẾT KẾ CƠ SỞ DỮ LIỆU & CÁC ENTITIES

## 2.1. Phân tích kịch bản CSDL `SportsCenter.sql` & Ràng buộc toàn vẹn

Tập tin kịch bản SQL Server `SportsCenter.sql` được xây dựng với tính năng tự làm sạch và tái khởi tạo nhiều lần an toàn:
1. **Cơ chế tái tạo sạch (Re-runnable)**:
   - Dòng 17-27: Chuyển ngữ cảnh về database `master`. Nếu cơ sở dữ liệu `SportsCenter` đang tồn tại, lệnh `ALTER DATABASE [SportsCenter] SET SINGLE_USER WITH ROLLBACK IMMEDIATE` sẽ cưỡng chế ngắt toàn bộ phiên kết nối đang mở trước khi thực thi `DROP DATABASE`.
   - Tạo mới database và thiết lập `SET QUOTED_IDENTIFIER ON;` (bắt buộc để SQL Server hỗ trợ tính năng **Filtered Index** - chỉ mục lọc có điều kiện).
2. **Hệ thống khóa & Ràng buộc nghiệp vụ (Constraints)**:
   - **Bảng `Users`**:
     - `Username`: `NVARCHAR(50) NOT NULL UNIQUE` – Tên đăng nhập duy nhất toàn hệ thống.
     - `PasswordHash`: `NVARCHAR(64) NOT NULL` – Chứa chuỗi hex 64 ký tự đại diện cho mã băm SHA-256.
     - `Role`: Ràng buộc `CHECK (Role IN (N'Manager', N'Receptionist', N'Coach', N'Member'))`.
     - `IsActive`: `BIT NOT NULL DEFAULT 1` – Cờ trạng thái khóa/mở tài khoản.
   - **Bảng `Members`**:
     - `UserId`: Cho phép `NULL` vì hội viên có thể đến đăng ký hồ sơ trực tiếp tại quầy thông qua Lễ tân khi chưa từng tạo tài khoản đăng nhập.
     - `UX_Members_UserId`: `CREATE UNIQUE INDEX UX_Members_UserId ON Members(UserId) WHERE UserId IS NOT NULL;` (Filtered Unique Index - Mỗi tài khoản User chỉ được gắn với tối đa một Member).
     - `UX_Members_Phone`: `CREATE UNIQUE INDEX UX_Members_Phone ON Members(Phone) WHERE Phone IS NOT NULL;` – Đảm bảo nguyên tắc: **Mỗi số điện thoại chỉ thuộc về duy nhất một hội viên**.
   - **Bảng `Coaches`**:
     - `UserId`: Liên kết 1-1 với `Users` qua chỉ mục duy nhất `UX_Coaches_UserId`.
     - `Email`: `NVARCHAR(150) NOT NULL` – HLV bắt buộc phải có email liên lạc.
   - **Bảng `MembershipPackages`**:
     - `DurationDays`: Ràng buộc `CHECK (DurationDays > 0)`.
     - `Price`: Ràng buộc `CHECK (Price >= 0)`.
     - `IsActive`: Cờ kiểm soát việc mở bán hoặc ngừng kinh doanh gói tập.
   - **Bảng `Subscriptions`**:
     - `StartDate`, `EndDate`: Ràng buộc `CHECK (EndDate >= StartDate)`.
     - `Status`: `CHECK (Status IN (N'Active', N'Expired', N'Cancelled'))`.
   - **Bảng `Sports`, `Rooms`, `TimeSlots`**:
     - `TimeSlots`: Cung cấp 6 khung giờ cố định trong ngày. Ràng buộc `CHECK (EndTime > StartTime)`.
     - `Rooms`: Ràng buộc `CHECK (Capacity > 0)`.
   - **Bảng `Classes`**:
     - `ClassCode`: Mã lớp học duy nhất (ví dụ `GYM01`, `YOG01`).
     - `DayOfWeek`: `TINYINT CHECK (DayOfWeek BETWEEN 1 AND 7)` (1 đại diện cho Thứ Hai, 7 đại diện cho Chủ Nhật).
     - `StartDate`, `EndDate`: Khoảng thời gian lớp học diễn ra.
     - `Status`: `CHECK (Status IN (N'Open', N'Closed', N'Cancelled'))`.
   - **Bảng `Registrations`**:
     - `UX_Registrations_Active`: `CREATE UNIQUE INDEX UX_Registrations_Active ON Registrations(MemberId, ClassId) WHERE Status = N'Registered';` – Một hội viên chỉ có thể có duy nhất một bản ghi đang hoạt động (`Registered`) trong một lớp học cụ thể.
   - **Bảng `Payments`**:
     - `InvoiceNo`: Mã hóa đơn duy nhất dạng tự tăng `INV-0001`, `INV-0002`...
     - `Amount`: `CHECK (Amount > 0)` – Số tiền thu bắt buộc phải lớn hơn 0.
     - `PaymentMethod`: `CHECK (PaymentMethod IN (N'Cash', N'BankTransfer', N'Card'))`.
     - `Status`: `CHECK (Status IN (N'Paid', N'Refunded'))`.
   - **Bảng `Attendances`**:
     - Khóa duy nhất tổ hợp `UQ_Attendances UNIQUE (RegistrationId, SessionDate)` – Mỗi học viên đăng ký trong một ngày học chỉ tồn tại duy nhất một dòng kết quả điểm danh.
     - `Status`: `CHECK (Status IN (N'Present', N'Absent'))`.
   - **Bảng `AuditLogs`**:
     - Lưu trữ lịch sử: `UserId`, `Action`, `EntityName`, `EntityId`, `Details`, `CreatedAt`.

---

## 2.2. Lớp kết nối CSDL `SportsCenterContext.cs`

Tập tin tọa lạc tại `SportsCenterApp/SportsCenter/Entities/SportsCenterContext.cs`.

```csharp
1: using Microsoft.EntityFrameworkCore;
2: using Microsoft.Extensions.Configuration;
3: 
4: namespace SportsCenter.Entities;
5: 
6: public partial class SportsCenterContext : DbContext
7: {
8:     public SportsCenterContext() { }
9:     public SportsCenterContext(DbContextOptions<SportsCenterContext> options) : base(options) { }
...
17:    public virtual DbSet<Attendance> Attendances { get; set; }
18:    public virtual DbSet<AuditLog> AuditLogs { get; set; }
19:    public virtual DbSet<Class> Classes { get; set; }
20:    public virtual DbSet<Coach> Coaches { get; set; }
21:    public virtual DbSet<Member> Members { get; set; }
22:    public virtual DbSet<MembershipPackage> MembershipPackages { get; set; }
23:    public virtual DbSet<Payment> Payments { get; set; }
24:    public virtual DbSet<Registration> Registrations { get; set; }
25:    public virtual DbSet<Room> Rooms { get; set; }
26:    public virtual DbSet<Sport> Sports { get; set; }
27:    public virtual DbSet<Subscription> Subscriptions { get; set; }
28:    public virtual DbSet<TimeSlot> TimeSlots { get; set; }
29:    public virtual DbSet<User> Users { get; set; }
```
**Giải thích từng dòng OnConfiguring & OnModelCreating**:
- Dòng 44-57: Phương thức ghi đè `OnConfiguring(DbContextOptionsBuilder optionsBuilder)`:
  - Khởi tạo `ConfigurationBuilder()`, gắn thư mục gốc chạy của ứng dụng qua `SetBasePath(AppContext.BaseDirectory)`.
  - Nạp tệp cấu hình `appsettings.json`.
  - Đọc chuỗi kết nối tại nút `ConnectionStrings:DefaultConnection`.
  - Gọi `optionsBuilder.UseSqlServer(strConn)` để cấu hình Entity Framework Core kết nối vào SQL Server.
- Dòng 59-84: Phương thức ghi đè `OnModelCreating(ModelBuilder modelBuilder)`:
  - Dòng 62-65: Khai báo khóa chính rõ ràng cho thực thể `AuditLog` với thuộc tính `LogId` bằng `entity.HasKey(e => e.LogId);` (vì tên thuộc tính không kết thúc bằng quy ước mặc định `Id` hoặc `<EntityName>Id`).
  - Dòng 67-71: Cấu hình bảng `MembershipPackages`: đặt khóa chính `PackageId` và định kiểu cột số thực độ chính xác cao `Price` là `decimal(18, 2)`.
  - Dòng 73-76: Cấu hình khóa chính `SlotId` cho thực thể `TimeSlot`.
  - Dòng 78-81: Cấu hình thuộc tính `Amount` của `Payment` tương ứng kiểu dữ liệu `decimal(18, 2)`.

---

## 2.3. Chi tiết 13 lớp thực thể (Entities)

Toàn bộ thực thể nằm trong thư mục `SportsCenterApp/SportsCenter/Entities/`:

1. **`User.cs`**:
   - `UserId` (int, PK): Khóa chính tự tăng.
   - `Username` (string): Tên đăng nhập.
   - `PasswordHash` (string): Mã băm mật khẩu SHA-256.
   - `FullName`, `Email`, `Phone` (string): Thông tin cá nhân.
   - `Role` (string): Vai trò (`Manager`, `Receptionist`, `Coach`, `Member`).
   - `IsActive` (bool): Trạng thái hoạt động.
   - `CreatedAt` (DateTime): Thời điểm tạo tài khoản.
2. **`Member.cs`**:
   - `MemberId` (int, PK): Khóa chính hội viên.
   - `UserId` (int?, FK): Khóa ngoại trỏ sang `Users` (cho phép null).
   - `FullName`, `Phone`, `Email` (string): Thông tin liên hệ hội viên.
   - `DateOfBirth` (DateOnly?), `Gender` (string?): Ngày sinh và giới tính.
   - `CreatedAt` (DateTime): Ngày tạo hồ sơ.
   - Quan hệ điều hướng: `User? User`, `ICollection<Subscription> Subscriptions`, `ICollection<Registration> Registrations`, `ICollection<Payment> Payments`.
3. **`Coach.cs`**:
   - `CoachId` (int, PK): Khóa chính HLV.
   - `UserId` (int?, FK): Khóa ngoại trỏ sang `Users`.
   - `FullName`, `Email`, `Phone`, `Specialty` (string): Thông tin huấn luyện viên & chuyên môn (Gym, Yoga, Boxing...).
   - Quan hệ điều hướng: `User? User`, `ICollection<Class> Classes`.
4. **`Sport.cs`**:
   - `SportId` (int, PK): Khóa chính bộ môn.
   - `SportName` (string): Tên bộ môn thể thao (Gym, Bơi lội, Yoga...).
   - Quan hệ điều hướng: `ICollection<Class> Classes`.
5. **`Room.cs`**:
   - `RoomId` (int, PK): Khóa chính phòng tập.
   - `RoomName` (string): Tên phòng tập.
   - `Capacity` (int): Sức chứa tối đa của phòng.
   - `IsActive` (bool): Trạng thái hoạt động.
   - Quan hệ điều hướng: `ICollection<Class> Classes`.
6. **`TimeSlot.cs`**:
   - `SlotId` (int, PK): Khóa chính ca học.
   - `SlotName` (string): Tên ca học (Ca 1, Ca 2...).
   - `StartTime`, `EndTime` (TimeOnly): Giờ bắt đầu và kết thúc ca học.
   - Quan hệ điều hướng: `ICollection<Class> Classes`.
7. **`Class.cs`**:
   - `ClassId` (int, PK): Khóa chính lớp học.
   - `ClassCode` (string): Mã lớp (GYM01, YOG01...).
   - `ClassName` (string): Tên lớp học.
   - `SportId`, `CoachId`, `RoomId`, `SlotId` (int, FK): Các khóa ngoại trỏ tới các bảng danh mục.
   - `DayOfWeek` (byte): Thứ trong tuần (1=Thứ Hai đến 7=Chủ Nhật).
   - `Capacity` (int): Sĩ số giới hạn của lớp.
   - `StartDate`, `EndDate` (DateOnly): Khoảng thời gian mở lớp.
   - `Status` (string): Trạng thái lớp (`Open`, `Closed`, `Cancelled`).
   - Quan hệ điều hướng: `Sport`, `Coach`, `Room`, `Slot`, `ICollection<Registration> Registrations`.
8. **`MembershipPackage.cs`**:
   - `PackageId` (int, PK): Khóa chính gói tập.
   - `PackageName` (string): Tên gói (Gói 1 tháng, Gói 3 tháng...).
   - `DurationDays` (int): Số ngày hiệu lực của gói.
   - `Price` (decimal): Mức giá niêm yết.
   - `Description` (string?): Mô tả chi tiết quyền lợi gói.
   - `IsActive` (bool): Trạng thái mở bán / ngừng bán.
   - Quan hệ điều hướng: `ICollection<Subscription> Subscriptions`.
9. **`Subscription.cs`**:
   - `SubscriptionId` (int, PK): Khóa chính đăng ký gói.
   - `MemberId` (int, FK): Hội viên sở hữu.
   - `PackageId` (int, FK): Gói tập được mua.
   - `StartDate`, `EndDate` (DateOnly): Ngày bắt đầu và ngày kết thúc hiệu lực.
   - `Status` (string): Trạng thái (`Active`, `Expired`, `Cancelled`).
   - Quan hệ điều hướng: `Member`, `MembershipPackage Package`, `ICollection<Payment> Payments`.
10. **`Payment.cs`**:
    - `PaymentId` (int, PK): Khóa chính bản ghi thanh toán.
    - `InvoiceNo` (string): Mã số hóa đơn (INV-0001...).
    - `MemberId` (int, FK): Hội viên nộp tiền.
    - `SubscriptionId` (int?, FK): Gói tập được thanh toán tương ứng.
    - `Amount` (decimal): Số tiền thực thu.
    - `PaymentMethod` (string): Phương thức (`Cash`, `BankTransfer`, `Card`).
    - `PaidAt` (DateTime): Thời điểm ghi nhận thanh toán.
    - `ReceivedBy` (int?, FK): Nhân viên thu ngân/lễ tân lập hóa đơn.
    - `Status` (string): Trạng thái hóa đơn (`Paid`, `Refunded`).
    - `Note` (string?): Ghi chú kèm theo.
    - Quan hệ điều hướng: `Member`, `Subscription? Subscription`, `User? ReceivedByUser`.
11. **`Registration.cs`**:
    - `RegistrationId` (int, PK): Khóa chính đăng ký lớp.
    - `MemberId` (int, FK), `ClassId` (int, FK): Khóa ngoại hội viên và lớp học.
    - `RegisteredAt` (DateTime): Ngày giờ đăng ký.
    - `CancelledAt` (DateTime?): Ngày giờ hủy đăng ký.
    - `Status` (string): Trạng thái (`Registered`, `Cancelled`).
    - `RegisteredBy` (int?, FK): Người thao tác (Member tự đăng ký hoặc Lễ tân).
    - Quan hệ điều hướng: `Member`, `Class`, `User? RegisteredByUser`, `ICollection<Attendance> Attendances`.
12. **`Attendance.cs`**:
    - `AttendanceId` (int, PK): Khóa chính điểm danh.
    - `RegistrationId` (int, FK): Lượt đăng ký lớp của học viên.
    - `SessionDate` (DateOnly): Ngày học cụ thể của buổi điểm danh.
    - `Status` (string): Kết quả điểm danh (`Present` - Có mặt, `Absent` - Vắng).
    - `CheckedInBy` (int?, FK): HLV thực hiện điểm danh.
    - `Note` (string?): Ghi chú buổi học.
    - Quan hệ điều hướng: `Registration`, `User? CheckedInByUser`.
13. **`AuditLog.cs`**:
    - `LogId` (int, PK): Khóa chính bản ghi kiểm toán.
    - `UserId` (int?, FK): Tài khoản thực hiện thao tác.
    - `Action` (string): Mã hành động (`Login`, `CreateClass`, `Payment`...).
    - `EntityName` (string?), `EntityId` (int?): Tên bảng và ID bản ghi bị tác động.
    - `Details` (string?): Nội dung chi tiết hành động.
    - `CreatedAt` (DateTime): Thời gian ghi nhật ký.
    - Quan hệ điều hướng: `User? User`.

---

# PHẦN 3: GIẢI THÍCH CÁC LỚP TIỆN ÍCH (HELPERS) & QUY TẮC NGHIỆP VỤ (RULES)

Các lớp nằm trong thư mục `SportsCenterApp/SportsCenter/Helpers/`:

## 3.1. `PasswordHelper.cs` – Băm mật khẩu chuẩn hóa SHA-256

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
10:            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
11:    }
12: }
```
**Giải thích từng dòng**:
- Dòng 6: Khai báo lớp tiện ích tĩnh `PasswordHelper`.
- Dòng 9-10: Hàm `Hash(string password)`:
  - `Encoding.UTF8.GetBytes(password)`: Chuyển đổi chuỗi mật khẩu sang mảng byte chuẩn UTF-8.
  - `SHA256.HashData(...)`: Sử dụng thuật toán SHA-256 sinh ra chuỗi 32 byte băm an toàn một chiều.
  - `Convert.ToHexString(...)`: Đổi mảng 32 byte này thành chuỗi ký tự Hexadecimal in hoa gồm 64 ký tự (ví dụ: `BA3253876AED6BC044DADE2FE...`). Kết quả này khớp 100% với hàm `HASHBYTES('SHA2_256', '123456')` của SQL Server trong tập tin `SportsCenter.sql`.

---

## 3.2. `Session.cs` – Lưu vết thông tin người dùng đang hoạt động

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
10:        public static Member? CurrentMember { get; set; }
11:        public static Coach? CurrentCoach { get; set; }
12:        public static string Role => CurrentUser?.Role ?? "";
13:
14:        public static void SignIn(SportsCenterContext db, User user)
15:        {
16:            CurrentUser = user;
17:            CurrentMember = user.Role == "Member" ? db.Members.AsNoTracking().FirstOrDefault(m => m.UserId == user.UserId) : null;
18:            CurrentCoach = user.Role == "Coach" ? db.Coaches.AsNoTracking().FirstOrDefault(c => c.UserId == user.UserId) : null;
19:            AuditHelper.Log(db, "Login", "Đăng nhập hệ thống");
20:        }
21:
22:        public static void Clear()
23:        {
24:            CurrentUser = null;
25:            CurrentMember = null;
26:            CurrentCoach = null;
27:        }
28:    }
29: }
```
**Giải thích từng dòng**:
- Dòng 9-11: Khai báo 3 thuộc tính `CurrentUser`, `CurrentMember`, `CurrentCoach` đại diện cho các thực thể người dùng tương ứng với vai trò.
- Dòng 12: `Role`: Trả về vai trò hiện tại của `CurrentUser` hoặc chuỗi rỗng nếu chưa đăng nhập.
- Dòng 14-20: Hàm `SignIn`:
  - Dòng 16: Lưu trữ `user` vào `CurrentUser`.
  - Dòng 17: Nếu vai trò là `"Member"`, truy vấn bảng `Members` với chế độ `AsNoTracking()` theo `user.UserId` để lấy thực thể hội viên.
  - Dòng 18: Nếu vai trò là `"Coach"`, truy vấn bảng `Coaches` theo `user.UserId`.
  - Dòng 19: Tự động ghi nhật ký đăng nhập thông qua `AuditHelper.Log`.
- Dòng 22-27: Hàm `Clear()` xóa sạch dữ liệu phiên làm việc khi đăng xuất.

---

## 3.3. `Msg.cs` – Đóng gói giao diện hộp thoại thông báo

```csharp
1: using System.Windows;
2: 
3: namespace SportsCenter
4: {
5:     // Gọi MessageBox ngắn gọn (giống cách Q2 báo lỗi / thành công)
6:     public static class Msg
7:     {
8:         public static void Warn(string message) =>
9:             MessageBox.Show(message, "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
10:
11:        public static void Info(string message) =>
12:            MessageBox.Show(message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
13:
14:        public static void Error(Exception ex) =>
15:            MessageBox.Show(ex.InnerException?.Message ?? ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
16:
17:        public static bool Confirm(string message) =>
18:            MessageBox.Show(message, "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
19:    }
20: }
```
**Giải thích từng dòng**:
- Dòng 8-9: `Warn(message)` hiển thị hộp thoại cảnh báo với biểu tượng dấu chấm than màu vàng.
- Dòng 11-12: `Info(message)` hiển thị thông báo thao tác thành công với biểu tượng chữ `i` màu xanh.
- Dòng 14-15: `Error(ex)` tự động bóc tách thông báo lỗi gốc từ `ex.InnerException?.Message` (rất hữu ích khi EF Core hoặc SQL Server ném ngoại lệ vi phạm khóa/ràng buộc) trước khi lấy `ex.Message`.
- Dòng 17-18: `Confirm(message)` hiển thị hộp thoại xác nhận `Yes/No`, trả về `true` nếu người dùng chọn `Yes`.

---

## 3.4. `GridHelper.cs` – Cấu hình tự động và định dạng hiển thị DataGrid

Tập tin nằm tại `SportsCenterApp/SportsCenter/Helpers/GridHelper.cs` (140 dòng).
- Dòng 10-46: Từ điển `Headers` ánh xạ tên thuộc tính C# sang Tiêu đề cột tiếng Việt (ví dụ: `ClassCode` -> "Mã lớp", `Price` -> "Giá (₫)", `InvoiceNo` -> "Hóa đơn"...).
- Dòng 49-59: Từ điển `Weights` định nghĩa trọng số độ rộng cột dạng Star (ví dụ cột `Details` rộng gấp 3 lần bình thường, `Schedule` rộng 1.9 lần) giúp giao diện cân đối không bị che khuất văn bản.
- Dòng 62-117: Hàm `Setup(DataGrid dg, bool editable = false)`:
  - Bật `AutoGenerateColumns = true;`. Đặt `IsReadOnly = !editable;`.
  - Đăng ký sự kiện `AutoGeneratingColumn`:
    - Dòng 71-75: **Ẩn cột khóa ID**: `if (name.Length > 2 && name.EndsWith("Id")) { e.Cancel = true; return; }`. Ẩn các cột khóa kỹ thuật (`ClassId`, `MemberId`...) khỏi màn hình nhưng dữ liệu ẩn vẫn còn nguyên trong bộ nhớ để code lấy ID khi người dùng click dòng.
    - Dòng 77: Đổi tiêu đề cột sang tiếng Việt thông qua từ điển `Headers`.
    - Dòng 80-95: Nếu chế độ `editable == true` và kiểu dữ liệu là `bool` (áp dụng cho màn hình Điểm danh), tạo một `DataGridTemplateColumn` chứa `CheckBox` tùy biến với cờ `UpdateSourceTrigger.PropertyChanged`. Nhờ đó huấn luyện viên chỉ cần **click 1 lần** là checkbox đổi trạng thái ngay lập tức (loại bỏ nhược điểm của DataGridCheckBoxColumn mặc định vốn phải click đúp 2 lần).
    - Dòng 107-115: Định dạng chuỗi hiển thị: Số thực `decimal` định dạng `"N0"` (phân tách hàng nghìn: 500,000), `DateTime` định dạng `"dd/MM/yyyy HH:mm"`, `DateOnly` định dạng `"dd/MM/yyyy"`.
- Dòng 120-130: Hàm `SelectById(dg, propertyName, id)`: Quét danh sách các hàng trong DataGrid, tìm bản ghi có giá trị thuộc tính ID trùng khớp và thiết lập `dg.SelectedItem = item`. Giúp người dùng giữ nguyên tiêu điểm sau khi danh sách được tải lại (Reload).
- Dòng 133-138: Hàm `SelectedId(dg, propertyName)`: Trích xuất an toàn giá trị ID của dòng người dùng đang chọn; nếu chưa chọn dòng nào sẽ trả về `null`.

---

## 3.5. `PagerControl.xaml.cs` – Thuật toán phân trang động

Tập tin điều khiển phân trang dùng chung cho toàn bộ các danh sách:
```csharp
public partial class PagerControl : UserControl
{
    public int PageSize { get; set; } = 8;
    public int CurrentPage { get; private set; } = 1;
    public event EventHandler? PageChanged;
...
```
**Giải thích thuật toán phân trang**:
- Mỗi trang mặc định hiển thị cố định **8 dòng dữ liệu** (`PageSize = 8`).
- Hàm `Reset()`: Đưa chỉ số trang hiện tại về 1.
- Hàm `Slice<T>(List<T> all)`:
  - Tính tổng số trang: `int pages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));`.
  - Hiệu chỉnh trang hợp lệ: nếu `CurrentPage > pages` thì gán bằng `pages`.
  - Gọi hàm `Render(total, pages)` vẽ thanh điều hướng.
  - Sử dụng LINQ: `all.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList()` để cắt đúng 8 phần tử của trang hiện tại trả về cho DataGrid.
- Hàm `Render(int total, int pages)`:
  - Nếu tổng số phần tử `total <= PageSize`: ẩn thanh phân trang (`Visibility = Collapsed`).
  - Khi có nhiều trang: tạo nút về trang đầu `«`, lùi một trang `‹`.
  - **Thuật toán cửa sổ trượt hiển thị 5 nút số trang**:
    ```csharp
    int start = Math.Max(1, Math.Min(CurrentPage - 2, pages - 4));
    int end = Math.Min(pages, start + 4);
    ```
    Thuật toán đảm bảo luôn hiển thị 5 số trang liên tiếp xoay quanh trang hiện tại mà không bị tràn ra ngoài biên.
  - Tạo nút tiến `›`, nút về trang cuối `»`.
  - Cập nhật dòng thông báo chi tiết: `"Hiển thị {from}-{to} trên {total}"`.
- Mỗi nút khi bấm sẽ gán `CurrentPage = goTo` và kích hoạt sự kiện `PageChanged?.Invoke(this, EventArgs.Empty)` để màn hình cha nạp lại dữ liệu tương ứng.

---

## 3.6. `AuditHelper.cs` – Ghi nhận nhật ký kiểm toán hệ thống

```csharp
1: using SportsCenter.Entities;
2: 
3: namespace SportsCenter
4: {
5:     public static class AuditHelper
6:     {
7:         public static void Log(SportsCenterContext db, string action, string details, string? entityName = null, int? entityId = null)
8:         {
9:             db.AuditLogs.Add(new AuditLog
10:            {
11:                UserId = Session.CurrentUser?.UserId,
12:                Action = action,
13:                EntityName = entityName,
14:                EntityId = entityId,
15:                Details = details,
16:                CreatedAt = DateTime.Now
17:            });
18:            db.SaveChanges();
19:        }
20:    }
21: }
```
**Giải thích từng dòng**:
- Hàm `Log`: Nhận vào context CSDL `db`, mã hành động `action`, chuỗi mô tả `details`, tên bảng nghiệp vụ `entityName` và ID của thực thể `entityId`.
- Tự động lấy ID người thao tác từ `Session.CurrentUser?.UserId`.
- Gọi `db.SaveChanges()` để đảm bảo vết kiểm toán được lưu ngay tức thì vào CSDL.

---

## 3.7. `AccountHelper.cs` – Chuẩn hóa chuỗi, xác thực tài khoản & thuật toán liên kết hồ sơ quầy

Tập tin nằm tại `SportsCenterApp/SportsCenter/Helpers/AccountHelper.cs` (118 dòng).
- **Hàm `Normalize(string text)`**:
  - Dòng 14-24: Chuyển toàn bộ chuỗi về chữ thường, chuyển ký tự `đ` thành `d`.
  - Sử dụng chuẩn phân rã `NormalizationForm.FormD` để tách các dấu thanh âm (sắc, huyền, hỏi, ngã, nặng) ra khỏi nguyên âm, sau đó lọc bỏ các ký tự thuộc nhóm `UnicodeCategory.NonSpacingMark`.
  - Cắt bỏ khoảng trắng thừa ở hai đầu và giữa các từ. Kết quả biến chuỗi `"  Nguyễn   Văn Đạt "` thành `"nguyen van dat"`. Giúp so sánh họ tên hội viên hoàn toàn không phụ thuộc vào dấu và cách viết hoa thường.
- **Hàm `FindByPhone(db, phone)`**:
  - Dòng 28-29: Tìm kiếm hội viên đang sở hữu số điện thoại này trong bảng `Members`. Nhờ chỉ mục duy nhất `UX_Members_Phone`, mỗi số điện thoại chỉ gắn với tối đa một người.
- **Enum `PhoneCheck` & Hàm `CheckPhoneForRegistration(db, fullName, phone)`**:
  - Dòng 32-45: Kiểm tra số điện thoại khi một người dùng tự đăng ký online trên `RegisterWindow`:
    1. Nếu `FindByPhone(db, phone) == null`: Trả về `PhoneCheck.Free` (Số điện thoại hoàn toàn mới -> cho phép tạo hồ sơ mới).
    2. Nếu số điện thoại đã tồn tại trong bảng `Members`: gọi `FindCounterProfiles(db, fullName, phone)`. Nếu tìm thấy hồ sơ tại quầy (chưa có `UserId`) và có họ tên sau khi `Normalize` khớp với tên đăng ký -> Trả về `PhoneCheck.LinkCounterProfile` (Hỏi người dùng xác nhận để gắn tài khoản vào hồ sơ cũ).
    3. Ngược lại (số điện thoại đã có tài khoản rồi, hoặc họ tên không trùng với hồ sơ quầy) -> Trả về `PhoneCheck.AlreadyRegistered` (Từ chối đăng ký, chặn mạo danh số điện thoại).
- **Hàm kiểm tra định dạng**:
  - Dòng 48: `IsValidEmail(email)`: Dùng Regex kiểm tra cú pháp thư điện tử tiêu chuẩn.
  - Dòng 51-57: `UsernameError(username)`: Kiểm tra độ dài từ 3-30 ký tự, biểu thức chính quy `^[A-Za-z0-9._]+$` chỉ cho phép chữ cái không dấu, chữ số, dấu chấm và gạch dưới (cấm khoảng trắng).
  - Dòng 60-67: `FullNameError(fullName)`: Độ dài từ 2-100 ký tự, sử dụng Regex `^\p{L}[\p{L}\p{M} .'’-]*$` hỗ trợ toàn bộ bảng chữ cái Unicode tiếng Việt có dấu, cấm hoàn toàn chữ số và ký tự lạ.
- **Hàm `CreateMemberAccount(...)`**:
  - Dòng 83-116: Thực hiện lưu trữ nguyên khối:
    - Tạo đối tượng `User` với vai trò `"Member"`, mật khẩu được băm qua `PasswordHelper.Hash(password)`.
    - **Trường hợp tạo mới** (`counterProfile == null`): Khởi tạo bản ghi `Member` mới gắn khóa ngoại `User = user`.
    - **Trường hợp gắn hồ sơ quầy** (`counterProfile != null`): Kiểm tra nếu hồ sơ đã có tài khoản thì ném ngoại lệ. Gắn hồ sơ đang có vào DbContext (`db.Members.Attach(counterProfile)`), thêm `user` mới vào `db.Users`, và liên kết `counterProfile.User = user`.
    - Toàn bộ thao tác chỉ gọi `db.SaveChanges()` đúng **một lần duy nhất**, đảm bảo tính toàn vẹn Transaction: Thành công cùng lúc hoặc không lưu gì cả nếu có lỗi.

---

## 3.8. `Rules.cs` (Partial lõi) – Hằng số hệ thống & Sinh mã hóa đơn

- Dòng 11: `Today => DateOnly.FromDateTime(DateTime.Today);`: Cung cấp ngày hiện tại kiểu `DateOnly` chuẩn hóa cho toàn hệ thống.
- Dòng 13-16: Mảng tên các thứ trong tuần: `DayNames` ("Thứ Hai" ... "Chủ Nhật"), `DayShort` ("T2" ... "CN").
- Dòng 18-24: `RoleNames`: Bảng tra cứu tên hiển thị tiếng Việt của các vai trò (`Manager` -> "Quản lý trung tâm", `Receptionist` -> "Nhân viên lễ tân"...).
- Dòng 27-35: `MethodNames` và `MethodList`: Danh mục phương thức thanh toán ("Tiền mặt", "Chuyển khoản", "Thẻ").
- Dòng 39-68: `ActionNames`: Bảng từ điển dịch các mã hành động kiểm toán sang tiếng Việt.
- Dòng 70-72: `IsoDay(d)`: Quy đổi ngày sang số thứ tự chuẩn ISO: Thứ Hai là 1, ..., Chủ Nhật là 7.
- Dòng 74-79: `ScheduleText(...)`: Định dạng chuỗi hiển thị lịch học (ví dụ: `"T2 · Ca 4 (16:30-18:00)"`).
- Dòng 81-91: **Hàm `NextInvoiceNo(SportsCenterContext db)` (Luật Y1)**:
  - Lấy tổng số hóa đơn hiện có cộng 1 làm mốc đếm ban đầu `n = db.Payments.Count() + 1`.
  - Vòng lặp `do ... while (db.Payments.Any(p => p.InvoiceNo == no))` định dạng chuỗi `"INV-" + n.ToString("D4")` (ví dụ `INV-0001`, `INV-0002`...). Nếu mã đã tồn tại thì tự động tăng `n++` cho đến khi tìm được mã hóa đơn chưa từng xuất hiện.

---

## 3.9. `Rules.Packages.cs` (Partial) – Kiểm tra gói hiệu lực & Gia hạn nối tiếp (P1-P5)

- **Hàm `ActiveSubscription(db, memberId)` (Luật P4)**:
  - Truy vấn gói của hội viên thỏa mãn đồng thời: `Status != "Cancelled"` và `StartDate <= Today && EndDate >= Today`.
  - Sắp xếp giảm dần theo ngày kết thúc `OrderByDescending(s => s.EndDate)` và lấy gói đầu tiên.
- **Hàm `SubscriptionText(Subscription? s)`**:
  - Trả về nhãn trạng thái trực quan: Nếu `null` -> "Chưa có gói"; nếu `Status == "Cancelled"` -> "Đã hủy"; nếu `EndDate < Today` -> "Hết hạn"; ngược lại -> "Còn hạn".
- **Hàm `NextStartDate(SportsCenterContext db, int memberId)` (Luật P3 - Thuật toán gia hạn nối tiếp)**:
  ```csharp
  DateOnly? lastEnd = db.Subscriptions
      .AsNoTracking()
      .Where(s => s.MemberId == memberId && s.Status != "Cancelled" && s.EndDate >= today)
      .OrderByDescending(s => s.EndDate)
      .Select(s => (DateOnly?)s.EndDate)
      .FirstOrDefault();
  return lastEnd.HasValue ? lastEnd.Value.AddDays(1) : today;
  ```
  - **Cơ chế**: Tìm ngày kết thúc xa nhất của các gói chưa bị hủy và chưa hết hạn (`EndDate >= today`), bao gồm cả những gói đã mua nối tiếp trước đó.
  - Nếu tìm thấy `lastEnd`: Gói mới sẽ bắt đầu vào ngày hôm sau `lastEnd.Value.AddDays(1)`.
  - Nếu hội viên chưa từng có gói hoặc mọi gói cũ đều đã hết hạn: Gói mới bắt đầu ngay từ ngày hôm nay `today`.
- **Hàm `QueuedSubscriptions(db, memberId)`**:
  - Lấy danh sách các gói thành viên đã thanh toán mua trước nhưng ngày bắt đầu lớn hơn hôm nay (`StartDate > today`).

---

## 3.10. `Rules.Classes.cs` (Partial) – Phát hiện xung đột lịch & Chặn hủy lớp (C1-C4)

- **Hàm `BusySet(db, roomId, coachId)`**:
  - Quét toàn bộ các lớp đang hoạt động (`Status != "Cancelled"`) có cùng phòng `roomId` HOẶC cùng huấn luyện viên `coachId`.
  - Trả về tập hợp các cặp `HashSet<(int Day, int SlotId)>` đã bị chiếm dụng.
- **Hàm `ClassConflictMessage(db, roomId, coachId, day, slotId)` (Luật C2, C3)**:
  - Kiểm tra xung đột phòng (C2): Tìm xem có lớp nào cùng `DayOfWeek`, `SlotId` và `RoomId` không. Nếu có báo lỗi: `"Phòng {RoomName} đã có lớp {ClassCode} ở ca này."`.
  - Kiểm tra xung đột HLV (C3): Tìm xem HLV đó đã dạy lớp nào ở cùng thứ và ca đó chưa. Nếu có báo lỗi: `"HLV đã dạy lớp {ClassCode} cùng khung giờ."`.
  - Không có xung đột trả về `null`.
- **Hàm `CancelClassBlockedReason(db, c)` (Luật C4)**:
  - Đếm số lượng học viên đang có trạng thái `Registered` trong lớp: `db.Registrations.Count(r => r.ClassId == c.ClassId && r.Status == "Registered")`.
  - Nếu `n > 0`: Chặn không cho phép Quản lý hủy lớp, yêu cầu phải hủy đăng ký hoặc chuyển lớp cho các học viên trước.

---

## 3.11. `Rules.Booking.cs` (Partial) – Quy tắc đặt lớp, sinh lịch & Hủy đăng ký (B1-B8)

- **Hàm `NextSession(Class c)`**:
  - Chạy vòng lặp duyệt 8 ngày tới kể từ hôm nay. Tìm ngày có thứ trong tuần trùng với `c.DayOfWeek`. Kết hợp ngày đó với `c.Slot.StartTime`. Nếu thời điểm này lớn hơn thời gian thực tại `DateTime.Now`, trả về thời điểm bắt đầu buổi học kế tiếp.
- **Hàm `SessionDates(Class c, int max = 12)`**:
  - Sinh danh sách tối đa 12 buổi học hợp lệ trong quá khứ/hiện tại của lớp để phục vụ điểm danh:
    1. Lùi ngày từ `Today` về ngày gần nhất có thứ trùng với `c.DayOfWeek`.
    2. Nếu ngày đó vượt quá ngày kết thúc lớp `c.EndDate`, lùi tiếp mỗi lần 7 ngày cho đến khi lọt vào khoảng `EndDate`.
    3. Thu thập các ngày thỏa mãn `>= c.StartDate`, mỗi bước lùi 7 ngày cho đến khi đủ 12 buổi.
- **Hàm `RegisterClass(db, memberId, classId)` (Bộ luật B1-B8)**:
  - Kiểm tra B1: Hội viên phải có gói còn hạn (`ActiveSubscription(db, memberId) != null`).
  - Kiểm tra B2: Lớp học phải có trạng thái `"Open"` và chưa kết thúc (`c.EndDate >= Today`).
  - Kiểm tra B4: Hội viên chưa đăng ký lớp này (`db.Registrations.Any(...)`).
  - Kiểm tra B3: Sĩ số đăng ký hiện tại chưa vượt quá sức chứa lớp (`count < c.Capacity`).
  - Kiểm tra B5: Không trùng lịch: Quét các lớp hội viên đã đăng ký, nếu có lớp nào cùng `DayOfWeek` và `SlotId` thì chặn ngay.
  - Nếu hợp lệ: Thêm bản ghi `Registration` với trạng thái `"Registered"`, lưu `RegisteredBy = Session.CurrentUser?.UserId`, gọi `SaveChanges()` và ghi `AuditHelper.Log`.
- **Hàm `CancelRegistration(db, memberId, classId)` (Luật B6)**:
  - Hằng số `CancelHoursBefore = 24`.
  - Tính thời điểm buổi học gần nhất `next = NextSession(c)`.
  - Nếu `next != null && next.Value - DateTime.Now < TimeSpan.FromHours(24)`: Chặn hủy và thông báo: `"Chỉ được hủy trước giờ học hơn 24 giờ."`.
  - Nếu đủ điều kiện: Cập nhật `reg.Status = "Cancelled"`, lưu `CancelledAt = DateTime.Now`, gọi `SaveChanges()` và ghi log kiểm toán.

---
# PHẦN 4: GIẢI THÍCH TỪNG DÒNG CODE CÁC CỬA SỔ (WINDOWS)

## 4.1. `App.xaml` & `App.xaml.cs` – Định dạng văn hóa & Kho tài nguyên chung

Tập tin `App.xaml.cs`:
```csharp
1: using System.Globalization;
2: using System.Windows;
3: using System.Windows.Markup;
4: 
5: namespace SportsCenter
6: {
7:     public partial class App : Application
8:     {
9:         public App()
10:        {
11:            // Hiển thị ngày và số theo định dạng Việt Nam (dd/MM/yyyy, 1.350.000)
12:            var vi = new CultureInfo("vi-VN");
13:            CultureInfo.DefaultThreadCurrentCulture = vi;
14:            CultureInfo.DefaultThreadCurrentUICulture = vi;
15:            FrameworkElement.LanguageProperty.OverrideMetadata(
16:                typeof(FrameworkElement),
17:                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(vi.IetfLanguageTag)));
18:        }
19:    }
20: }
```
**Giải thích từng dòng code**:
- Dòng 7-8: Lớp `App` kế thừa từ `Application` của WPF, là điểm vào đầu tiên khi ứng dụng khởi động.
- Dòng 12: Khởi tạo đối tượng `CultureInfo("vi-VN")` chứa các quy tắc hiển thị ngôn ngữ và định dạng số/ngày của Việt Nam.
- Dòng 13-14: Gán văn hóa `vi-VN` vào `CultureInfo.DefaultThreadCurrentCulture` và `DefaultThreadCurrentUICulture` để áp dụng cho mọi luồng (thread) chạy trong ứng dụng.
- Dòng 15-17: Ghi đè thuộc tính phụ thuộc `LanguageProperty` của `FrameworkElement` bằng mã ngôn ngữ `vi-VN`. Điều này đảm bảo toàn bộ các điều khiển giao diện XAML (DataGrid, TextBlock, DatePicker...) tự động format ngày tháng thành `dd/MM/yyyy` và phân tách phần nghìn bằng dấu chấm `.` (ví dụ `1.000.000 ₫`) thay vì định dạng Mỹ `MM/dd/yyyy`.

---

## 4.2. `LoginWindow.xaml.cs` – Xác thực đăng nhập & Phân luồng điều hướng

Tập tin nằm tại `SportsCenterApp/SportsCenter/LoginWindow.xaml.cs` (62 dòng).
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
10:        public LoginWindow()
11:        {
12:            InitializeComponent();
13:            _db = new();
14:            txtUsername.Focus();
15:        }
```
**Giải thích từng bước & từng dòng hàm `btnLogin_Click`**:
```csharp
20:        private void btnLogin_Click(object sender, RoutedEventArgs e)
21:        {
22:            string username = txtUsername.Text.Trim();
23:            string password = txtPassword.Password;
24: 
25:            if (username == "" || password == "")
26:            {
27:                Msg.Warn("Vui lòng nhập tên đăng nhập và mật khẩu.");
28:                return;
29:            }
```
- **Bước 1. Thu thập & Tiền kiểm tra dữ liệu**:
  - Dòng 22: Đọc tên đăng nhập từ `txtUsername.Text`, cắt bỏ khoảng trắng thừa hai đầu bằng `.Trim()`.
  - Dòng 23: Đọc mật khẩu bảo mật từ thuộc tính `txtPassword.Password`.
  - Dòng 25-29: Kiểm tra nếu một trong hai trường trống, gọi `Msg.Warn` thông báo và dừng thực thi (`return`).

```csharp
31:            try
32:            {
33:                var user = _db.Users.FirstOrDefault(u => u.Username == username);
34:                if (user == null || user.PasswordHash != PasswordHelper.Hash(password))
35:                {
36:                    Msg.Warn("Sai tên đăng nhập hoặc mật khẩu.");
37:                    return;
38:                }
39:                if (!user.IsActive)
40:                {
41:                    Msg.Warn("Tài khoản đã bị khóa.");
42:                    return;
43:                }
44: 
45:                Session.SignIn(_db, user);
46:                new MainWindow().Show();
47:                Close();
48:            }
49:            catch (Exception ex)
50:            {
51:                MessageBox.Show("Không kết nối được cơ sở dữ liệu. Hãy kiểm tra appsettings.json và đã chạy SportsCenter.sql chưa.\n\n"
52:                    + (ex.InnerException?.Message ?? ex.Message), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
53:            }
54:        }
```
- **Bước 2. Xác thực và Phân quyền đăng nhập**:
  - Dòng 33: Truy vấn bảng `Users` theo `Username`.
  - Dòng 34-38 (Luật A1): Băm mật khẩu người dùng vừa nhập bằng `PasswordHelper.Hash(password)` và so sánh với `user.PasswordHash`. Nếu không tìm thấy người dùng hoặc mã băm không khớp, hiển thị thông báo lỗi chung `"Sai tên đăng nhập hoặc mật khẩu."` để ngăn ngừa tấn công đoán tài khoản.
  - Dòng 39-43: Kiểm tra cờ `user.IsActive`. Nếu tài khoản bị khóa (`false`), từ chối đăng nhập và báo `"Tài khoản đã bị khóa."`.
  - Dòng 45: Gọi `Session.SignIn(_db, user)` để ghi nhận trạng thái đăng nhập toàn cục và ghi log kiểm toán.
- **Bước 3. Điều hướng ứng dụng**:
  - Dòng 46: Khởi tạo và hiển thị cửa sổ làm việc chính `new MainWindow().Show()`.
  - Dòng 47: Đóng màn hình đăng nhập `Close()`.
  - Dòng 49-53: Khối `catch` bắt lỗi mất kết nối SQL Server, đưa ra hướng dẫn kiểm tra `appsettings.json` và file kịch bản SQL.

```csharp
56:        private void btnRegister_Click(object sender, RoutedEventArgs e)
57:        {
58:            new RegisterWindow().Show();
59:            Close();
60:        }
```
- Dòng 56-60: Khi người dùng bấm nút "Đăng ký tài khoản", đóng `LoginWindow` và mở `RegisterWindow`.

---

## 4.3. `RegisterWindow.xaml.cs` – Tự đăng ký tài khoản & Liên kết hồ sơ quầy

Tập tin nằm tại `SportsCenterApp/SportsCenter/RegisterWindow.xaml.cs` (158 dòng).
- Dòng 22-25: Hàm `ClearErrors()`: Xóa sạch toàn bộ các nhãn thông báo lỗi màu đỏ nằm dưới từng ô nhập liệu (`errFullName`, `errPhone`, `errUsername`, `errPassword`, `errConfirm`).

**Giải thích chi tiết quy trình xử lý của hàm `btnCreate_Click`**:
```csharp
27:        private void btnCreate_Click(object sender, RoutedEventArgs e)
28:        {
29:            ClearErrors();
30:            string fullName = txtFullName.Text.Trim();
31:            string phone = txtPhone.Text.Trim();
32:            string username = txtUsername.Text.Trim();
33:            string password = txtPassword.Password;
34:            string confirm = txtConfirm.Password;
35:            bool ok = true;
36:            var phoneCheck = (Result: AccountHelper.PhoneCheck.Free, Profile: (Member?)null);
```
- **Bước 1. Kiểm tra tính hợp lệ của từng trường dữ liệu (Validation)**:
  - Dòng 38-47 (Kiểm tra Họ tên): Kiểm tra không để trống; kiểm tra qua `AccountHelper.FullNameError(fullName)` để cấm số và ký tự lạ. Nếu sai, gán thông báo lỗi vào `errFullName.Text` và đặt `ok = false`.
  - Dòng 49-68 (Kiểm tra Số điện thoại):
    - Kiểm tra định dạng bắt buộc 10-11 số và bắt đầu bằng số `0` bằng Regex `^0\d{9,10}$`.
    - Dòng 62: Gọi `AccountHelper.CheckPhoneForRegistration(_db, fullName, phone)`.
    - Dòng 63-67 (Luật A5): Nếu kết quả trả về `AlreadyRegistered` (số điện thoại đã có tài khoản khác hoặc hồ sơ quầy không khớp tên), gán lỗi cảnh báo vào `errPhone.Text`.
  - Dòng 70-84 (Kiểm tra Tên đăng nhập): Không để trống; kiểm tra quy tắc 3-30 ký tự qua `AccountHelper.UsernameError(username)`; kiểm tra chống trùng `_db.Users.Any(u => u.Username == username)`.
  - Dòng 86-100 (Kiểm tra Mật khẩu): Tối thiểu 6 ký tự; cấm mật khẩu trùng với tên đăng nhập (`password.ToLower() == username.ToLower()`).
  - Dòng 102-111 (Kiểm tra Xác nhận mật khẩu): Hai ô mật khẩu bắt buộc phải trùng khớp `confirm == password`.
  - Dòng 113: Nếu `!ok` (có ít nhất một lỗi), dừng hàm để người dùng sửa lại.

```csharp
115:            try
116:            {
117:                Member? link = null;
118:                if (phoneCheck.Result == AccountHelper.PhoneCheck.LinkCounterProfile)
119:                {
120:                    var m = phoneCheck.Profile!;
121:                    if (!Msg.Confirm($"Đã có hồ sơ tại quầy của {m.FullName} (SĐT {m.Phone}, tạo ngày {m.CreatedAt:dd/MM/yyyy}).\n\n"
122:                        + "Gắn tài khoản mới vào hồ sơ đó để xem lại gói, hóa đơn và lớp đã đăng ký?"))
123:                    {
124:                        errPhone.Text = "Số điện thoại này đã có hồ sơ tại quầy. Hãy gắn tài khoản vào hồ sơ đó, hoặc dùng số điện thoại khác.";
125:                        return;
126:                    }
127:                    link = m;
128:                }
```
- **Bước 2. Xử lý kịch bản liên kết hồ sơ tại quầy (Luật A2 - Gắn hồ sơ)**:
  - Dòng 118-130: Nếu số điện thoại thuộc về một hồ sơ đăng ký tại quầy trước đó và trùng họ tên (`LinkCounterProfile`), hệ thống bật hộp thoại `Msg.Confirm` hỏi hội viên có muốn gắn tài khoản mới vào hồ sơ cũ hay không.
  - Nếu hội viên chọn "Không", hệ thống dừng đăng ký và yêu cầu dùng số điện thoại khác (bảo đảm nguyên tắc toàn vẹn: mỗi số điện thoại chỉ gắn với một hội viên duy nhất).
  - Nếu hội viên chọn "Có", gán `link = m`.

```csharp
132:                var user = AccountHelper.CreateMemberAccount(_db, username, password, fullName, phone, link);
133:                Session.SignIn(_db, user);
134:                if (link != null)
135:                {
136:                    AuditHelper.Log(_db, "LinkMember",
137:                        $"Tài khoản {username} gắn vào hồ sơ quầy {link.FullName} (#{link.MemberId})", "Members", link.MemberId);
138:                }
139: 
140:                string note = link != null ? " Tài khoản đã được gắn vào hồ sơ tại quầy của bạn." : "";
141:                Msg.Info("Tạo tài khoản thành công." + note);
142:                new MainWindow().Show();
143:                Close();
144:            }
145:            catch (Exception ex)
146:            {
147:                Msg.Error(ex);
148:            }
```
- **Bước 3. Lưu trữ và Đăng nhập tự động**:
  - Dòng 132: Gọi `AccountHelper.CreateMemberAccount` để tạo `User` và tạo mới hoặc gắn liên kết `Member` trong cùng một transaction.
  - Dòng 133: Đăng nhập ngay lập tức cho người dùng bằng `Session.SignIn`.
  - Dòng 134-138: Nếu có gắn hồ sơ quầy, ghi nhận nhật ký kiểm toán hành động `"LinkMember"`.
  - Dòng 141-143: Thông báo thành công, mở `MainWindow` và đóng form đăng ký.

---

## 4.4. `MainWindow.xaml.cs` – Điều hướng Menu động theo Role

Tập tin nằm tại `SportsCenterApp/SportsCenter/MainWindow.xaml.cs` (128 dòng).
- Dòng 12: `public static MainWindow? Instance { get; private set; }`: Cung cấp thể hiện Singleton của cửa sổ chính để các Page con có thể gọi chuyển trang (ví dụ từ `MembersPage` chuyển sang `BuyPage`).
- Dòng 21-31: Hàm khởi tạo `MainWindow()`:
  - Hiển thị thông tin người dùng lên góc trên: Họ tên (`txtUserName`), Tên đăng nhập (`txtUserLogin`), Tên vai trò tiếng Việt (`txtRole`).
  - Gọi `BuildMenu(user.Role)` để sinh menu.
  - Mở trang mặc định `_firstPage` vào vùng hiển thị nội dung `contentHost`.

**Giải thích chi tiết hàm `BuildMenu(string role)`**:
```csharp
34:         private void BuildMenu(string role)
35:         {
36:             switch (role)
37:             {
38:                 case "Manager":
39:                     AddItem("Báo cáo", typeof(DashboardPage));
40:                     AddGroup("Quản lý lớp", ("Lớp học", typeof(ClassesPage)), ("Bộ môn, phòng", typeof(SportsRoomsPage)));
41:                     AddGroup("Thành viên và gói", ("Member", typeof(MembersPage)), ("Gói thành viên", typeof(PackagesPage)));
42:                     AddItem("Thanh toán", typeof(PaymentsPage));
43:                     AddGroup("Hệ thống", ("Tài khoản", typeof(UsersPage)), ("Lịch sử hoạt động", typeof(AuditPage)));
44:                     break;
45: 
46:                 case "Receptionist":
47:                     AddGroup("Thành viên", ("Member", typeof(MembersPage)), ("Mua / gia hạn gói", typeof(BuyPage)));
48:                     AddItem("Đặt lớp", typeof(BookingPage));
49:                     AddItem("Thanh toán", typeof(PaymentsPage));
50:                     break;
51: 
52:                 case "Member":
53:                     AddItem("Lịch của tôi", typeof(MySchedulePage));
54:                     AddItem("Đặt lớp", typeof(BookingPage));
55:                     AddItem("Gói thành viên", typeof(BuyPage));
56:                     AddItem("Thanh toán", typeof(PaymentsPage));
57:                     break;
58: 
59:                 case "Coach":
60:                     AddItem("Lớp của tôi", typeof(CoachPage));
61:                     AddItem("Điểm danh", typeof(AttendancePage));
62:                     break;
63:             }
64:         }
```
- **Xây dựng thực đơn phân quyền**:
  - `Manager`: Nhận toàn bộ 8 màn hình quản trị chia thành các nhóm Expander xổ xuống (`Quản lý lớp`, `Thành viên và gói`, `Hệ thống`) và các mục đơn (`Báo cáo`, `Thanh toán`).
  - `Receptionist`: Tập trung vào nghiệp vụ quầy gồm Quản lý Member, Bán gói tập (`BuyPage`), Đặt lớp (`BookingPage`) và Thanh toán.
  - `Member`: Cung cấp 4 tính năng tự phục vụ gồm Thời khóa biểu tuần (`MySchedulePage`), Đặt lớp, Mua gói và Xem hóa đơn cá nhân.
  - `Coach`: Tối giản chỉ gồm 2 chức năng chuyên môn: Xem lớp dạy (`CoachPage`) và Điểm danh (`AttendancePage`).

**Giải thích cơ chế Accordion & Chuyển trang**:
- Dòng 66-73: Hàm `MakeButton`: Khởi tạo nút menu, gắn sự kiện click gọi `ShowPage((UserControl)Activator.CreateInstance(pageType)!)`.
- Dòng 77-97: Hàm `AddGroup`: Khởi tạo điều khiển `Expander`. Đăng ký sự kiện `group.Expanded`: Khi mở một nhóm bất kỳ, tự động thu gọn (`IsExpanded = false`) toàn bộ các nhóm khác, tạo hiệu ứng Accordion mượt mà, chuyên nghiệp.
- Dòng 100-113: Hàm `ShowPage(UserControl page)`:
  - Gán trang mới vào vùng chứa: `contentHost.Content = page;`.
  - Tô sáng nút menu được chọn bằng thuộc tính `btn.Tag = "on"`.
  - Nếu trang thuộc về một nhóm Expander, tự động mở nhóm đó (`group.IsExpanded = true`).
- Dòng 116-119: Nút "Đổi mật khẩu" mở hộp thoại `ChangePasswordWindow` dưới dạng modal dialog (`ShowDialog()`).
- Dòng 121-126: Nút "Đăng xuất" gọi `Session.Clear()`, hiển thị lại `LoginWindow` và đóng `MainWindow`.

---

## 4.5. `ChangePasswordWindow.xaml.cs` – Đổi mật khẩu tài khoản

Tập tin nằm tại `SportsCenterApp/SportsCenter/ChangePasswordWindow.xaml.cs` (93 dòng).
```csharp
23:         private void btnChange_Click(object sender, RoutedEventArgs e)
24:         {
25:             errOld.Text = errNew.Text = errConfirm.Text = "";
26:             string oldPw = txtOld.Password;
27:             string newPw = txtNew.Password;
28:             string confirm = txtConfirm.Password;
29: 
30:             var user = _db.Users.Find(Session.CurrentUser!.UserId)!;
31:             bool ok = true;
```
**Giải thích từng bước xử lý**:
- **Bước 1. Kiểm tra mật khẩu cũ**:
  - Dòng 33-42: Không để trống mật khẩu cũ; so sánh mã băm `user.PasswordHash != PasswordHelper.Hash(oldPw)`. Nếu sai, thông báo `"Mật khẩu hiện tại chưa đúng."`.
- **Bước 2. Kiểm tra mật khẩu mới**:
  - Dòng 44-53: Bắt buộc tối thiểu 6 ký tự.
  - Dòng 54-58: Mật khẩu mới bắt buộc phải khác mật khẩu cũ `newPw != oldPw`.
  - Dòng 59-63: Mật khẩu mới cấm trùng tên đăng nhập `newPw.ToLower() != user.Username.ToLower()`.
  - Dòng 65-74: Xác nhận mật khẩu mới phải khớp hoàn toàn `confirm == newPw`.
- **Bước 3. Cập nhật CSDL & Ghi log**:
  - Dòng 80: Băm mật khẩu mới: `user.PasswordHash = PasswordHelper.Hash(newPw);`.
  - Dòng 81: `_db.SaveChanges();`.
  - Dòng 82: `AuditHelper.Log(_db, "ChangePassword", $"{user.Username} đổi mật khẩu", "Users", user.UserId);`.
  - Dòng 84-85: Báo thành công và đóng hộp thoại.

---
# PHẦN 5: GIẢI THÍCH TỪNG DÒNG CODE CÁC TRANG CHỨC NĂNG (PAGES)

## 5.1. `DashboardPage.xaml.cs` – Báo cáo tổng hợp, Biểu đồ Canvas & Cảnh báo

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/DashboardPage.xaml.cs` (185 dòng).
Màn hình trung tâm điều hành của Manager, gồm 4 khối: 4 thẻ KPI tổng quan, Biểu đồ doanh thu 6 tháng vẽ trực tiếp trên WPF Canvas, Bảng tỷ lệ lấp đầy lớp học, và Bảng cảnh báo gói sắp hết hạn.

```csharp
20:         public DashboardPage()
21:         {
22:             InitializeComponent();
23:             _db = new();
24:             GridHelper.Setup(dgFill);
25:             GridHelper.Setup(dgExpiring);
26:             pgFill.PageChanged += (s, e) => LoadFill();
27:             pgExpiring.PageChanged += (s, e) => LoadExpiring();
28: 
29:             dpFrom.SelectedDate = DateTime.Today.AddDays(-180);
30:             dpTo.SelectedDate = DateTime.Today;
31:             LoadReport();
32:         }
```
**Giải thích hàm khởi tạo**:
- Khởi tạo `_db`, cấu hình tiêu đề và phân trang cho 2 DataGrid `dgFill` (lấp đầy) và `dgExpiring` (sắp hết hạn).
- Đặt khoảng ngày mặc định: Từ 180 ngày trước (`DateTime.Today.AddDays(-180)`) đến hôm nay.

**Giải thích hàm `LoadReport()`**:
```csharp
45:         private void LoadReport()
46:         {
47:             DateTime from = dpFrom.SelectedDate!.Value.Date;
48:             DateTime to = dpTo.SelectedDate!.Value.Date.AddDays(1);
49:             var today = Rules.Today;
50: 
51:             // Doanh thu chỉ tính các hóa đơn có Status = Paid
52:             var paid = _db.Payments.Where(p => p.Status == "Paid" && p.PaidAt >= from && p.PaidAt < to)
53:                 .Select(p => new { p.PaidAt, p.Amount }).ToList();
54: 
55:             txtMembers.Text = _db.Members.Count().ToString();
56:             txtActiveSubs.Text = _db.Subscriptions
57:                 .Count(s => s.Status != "Cancelled" && s.StartDate <= today && s.EndDate >= today).ToString();
58:             txtRevenue.Text = paid.Sum(p => p.Amount).ToString("N0", _vi) + " ₫";
59:             txtRegistrations.Text = _db.Registrations.Count(r => r.Status == "Registered").ToString();
```
- **Bước 1. Thu thập dữ liệu báo cáo**:
  - Dòng 48: `to = dpTo.SelectedDate!.Value.Date.AddDays(1)` đảm bảo quét trọn vẹn cả ngày cuối cùng (đến 23:59:59).
  - Dòng 52-53 (Luật R1): Lọc hóa đơn hợp lệ có `Status == "Paid"` trong khoảng ngày. Chỉ chiếu lấy thuộc tính `PaidAt` và `Amount` để tối ưu bộ nhớ.
  - Dòng 55: Tổng số hội viên trong hệ thống `_db.Members.Count()`.
  - Dòng 56-57 (Luật P4): Tổng số gói đang có hiệu lực (`StartDate <= today && EndDate >= today` và chưa hủy).
  - Dòng 58: Tổng doanh thu thực thu: tính tổng `paid.Sum(p => p.Amount)` và định dạng phân tách hàng nghìn tiếng Việt.
  - Dòng 59: Tổng số lượt đăng ký lớp đang hoạt động (`Status == "Registered"`).

```csharp
61:             // 6 tháng gần nhất tính đến tháng của "Đến ngày"; tháng chưa có doanh thu thì để trống
62:             var months = new List<(string Label, decimal Value)>();
63:             var last = new DateTime(dpTo.SelectedDate!.Value.Year, dpTo.SelectedDate!.Value.Month, 1);
64:             for (int i = 5; i >= 0; i--)
65:             {
66:                 var m = last.AddMonths(-i);
67:                 decimal value = paid.Where(p => p.PaidAt.Year == m.Year && p.PaidAt.Month == m.Month).Sum(p => p.Amount);
68:                 months.Add(("T" + m.Month, value));
69:             }
70:             DrawChart(months);
71: 
72:             LoadFill();
73:             LoadExpiring();
74:         }
```
- **Bước 2. Gom nhóm doanh thu 6 tháng**:
  - Dòng 63-69: Xác định tháng mốc là tháng của `dpTo`. Dùng vòng lặp lùi 5 bước (`last.AddMonths(-i)`) để lấy chuỗi 6 tháng liên tiếp. Tính tổng tiền cho từng tháng rồi đưa vào danh sách `months`.
  - Dòng 70: Gọi hàm vẽ biểu đồ `DrawChart(months)`.

**Giải thích thuật toán vẽ biểu đồ trên WPF Canvas (`DrawChart`)**:
```csharp
77:         private void DrawChart(List<(string Label, decimal Value)> months)
78:         {
79:             canvasChart.Children.Clear();
80: 
81:             const double left = 52, baseline = 190, plotHeight = 150, slot = 82, barWidth = 34;
82:             decimal max = Math.Max(1, months.Max(m => m.Value));
83:             decimal[] steps = { 500_000, 1_000_000, 2_000_000, 5_000_000, 10_000_000, 20_000_000, 50_000_000 };
84:             decimal step = steps.FirstOrDefault(s => Math.Ceiling(max / s) <= 5);
85:             if (step == 0) step = 100_000_000;
86:             int n = (int)Math.Ceiling(max / step);
87:             decimal top = n * step;
```
- Dòng 81: Định nghĩa các hằng số tọa độ: Cạnh trái `left = 52`, đường đáy biểu đồ `baseline = 190`, chiều cao vùng vẽ `plotHeight = 150`, khoảng cách giữa các cột `slot = 82`, độ rộng cột `barWidth = 34`.
- Dòng 82-87: **Thuật toán chia vạch trục tung tự động**: Chọn bước nhảy tròn số từ mảng `steps` sao cho số lượng vạch chia trục không vượt quá 5 vạch (`Math.Ceiling(max / s) <= 5`).
- Dòng 94-104: Vẽ các đường kẻ ngang chấm nét đứt (`StrokeDashArray = 3, 4`) và gắn nhãn số tiền bằng đơn vị triệu đồng (`AddText(FormatMillions(...))`).
- Dòng 106-120: Vòng lặp duyệt 6 tháng:
  - Tính chiều cao cột tương ứng theo tỷ lệ: `double h = (double)(value / top) * plotHeight;`.
  - Vẽ cột chữ nhật bo góc `RadiusX = 4` qua hàm `CreateBar`. Tháng gần nhất được tô đậm (`opacity = 1`), các tháng trước tô mờ hơn (`opacity = 0.45`).
  - Gắn nhãn giá trị trên đầu cột và tên tháng (`T1`, `T2`...) ở dưới đáy cột.

**Giải thích hàm `LoadFill()` & `LoadExpiring()`**:
- `LoadFill()` (Dòng 149-171): Lấy các lớp `"Open"`, đếm số học viên đăng ký `Count` so với `Capacity`, tính tỷ lệ phần trăm `Count * 100 / Capacity + "%"`. Cắt 8 dòng/trang đưa lên `dgFill`.
- `LoadExpiring()` (Dòng 173-183 - Luật P5): Quét các gói thỏa mãn `StartDate <= today && EndDate >= today && EndDate <= today.AddDays(7)` (hết hạn trong vòng 7 ngày tới) để Quản lý kịp thời liên hệ nhắc hội viên gia hạn.

---

## 5.2. `UsersPage.xaml.cs` – Quản lý tài khoản, Khóa & Đổi vai trò

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/UsersPage.xaml.cs` (218 dòng).
Màn hình dành riêng cho Manager quản trị toàn bộ người dùng trong hệ thống.

```csharp
44:         private void LoadUsers()
45:         {
46:             var query = _db.Users.AsQueryable();
47: 
48:             string keyword = txtSearch.Text.Trim();
49:             if (keyword != "")
50:             {
51:                 query = query.Where(u => u.Username.Contains(keyword) || u.FullName.Contains(keyword));
52:             }
53:             if (cbxRoleFilter.SelectedIndex > 0)
54:             {
55:                 string role = _roles[cbxRoleFilter.SelectedIndex - 1];
56:                 query = query.Where(u => u.Role == role);
57:             }
```
- **Hàm `LoadUsers()`**:
  - Lọc đa tiêu chí: theo từ khóa `Username` hoặc `FullName`, kết hợp lọc theo vai trò trong ComboBox `cbxRoleFilter`.
  - Dòng 61-72: Chiếu dữ liệu sang tên vai trò tiếng Việt (`RoleName(u.Role)`) và trạng thái `"Đang hoạt động"` / `"Đã khóa"`. Đưa qua `pgUsers.Slice(rows)` hiển thị lên `dgUsers`.

```csharp
90:         private User? GetSelectedUser()
91:         {
92:             int? id = GridHelper.SelectedId(dgUsers, "UserId");
93:             if (id == null)
94:             {
95:                 Msg.Warn("Vui lòng chọn một tài khoản trong danh sách.");
96:                 return null;
97:             }
98:             if (id == Session.CurrentUser!.UserId)
99:             {
100:                 Msg.Warn("Không thể khóa hoặc đổi vai trò của chính tài khoản đang đăng nhập.");
101:                 return null;
102:             }
103:             return _db.Users.Find(id);
104:         }
```
- **Hàm `GetSelectedUser()` (Luật A3 - Bảo vệ tài khoản quản trị)**:
  - Lấy `UserId` của dòng đang chọn trên DataGrid.
  - Dòng 98-102: **Cấm tự khóa hoặc tự tước quyền**: Nếu tài khoản được chọn trùng với `Session.CurrentUser.UserId`, lập tức cảnh báo và chặn thao tác.

**Hàm `btnToggle_Click` (Khóa / Mở khóa)**:
- Đảo ngược cờ: `user.IsActive = !user.IsActive;`.
- Lưu CSDL `_db.SaveChanges()`, ghi log `AuditHelper.Log(_db, "ToggleUser", ...)`.
- Nạp lại danh sách và gọi `GridHelper.SelectById` để giữ tiêu điểm vào đúng tài khoản vừa sửa.

**Hàm `btnChangeRole_Click` (Đổi vai trò) & `EnsureProfile`**:
- Dòng 130-150: Đọc vai trò mới từ `cbxNewRole`. Nếu trùng vai trò cũ thì cảnh báo.
- Dòng 140: Gọi `EnsureProfile(user)`:
  - Dòng 153-163: Khi đổi tài khoản sang role `"Coach"`, kiểm tra nếu chưa có bản ghi trong bảng `Coaches` thì tự động thêm một dòng mới với email mặc định `{username}@center.vn`. Nếu đổi sang role `"Member"`, kiểm tra và tự động sinh bản ghi trong bảng `Members`. Điều này bảo đảm tính toàn vẹn dữ liệu cho các bảng nghiệp vụ liên đới.
- Ghi log hành động `"SetRole"`.

**Hàm `btnAddUser_Click` (Tạo tài khoản mới)**:
- Kiểm tra tính hợp lệ của họ tên và username qua `AccountHelper.FullNameError` và `AccountHelper.UsernameError`.
- Kiểm tra cấm trùng username trong bảng `Users`.
- Dòng 192-200: Khởi tạo thực thể `User` mới với **mật khẩu mặc định là 123456** băm qua `PasswordHelper.Hash("123456")`, cờ `IsActive = true`.
- Tự động gọi `EnsureProfile(user)` và lưu CSDL trong một transaction.
- Ghi log `"CreateUser"` và hiển thị thông báo hướng dẫn người dùng đổi mật khẩu ở lần đăng nhập đầu tiên.

---

## 5.3. `MembersPage.xaml.cs` – Quản lý hội viên & Đăng ký tại quầy

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/MembersPage.xaml.cs` (165 dòng).
Màn hình phục vụ cả Quản lý và Lễ tân.
- Dòng 24-28 (Phân quyền giao diện theo vai trò):
  ```csharp
  if (Session.Role == "Manager")
  {
      btnBuy.Visibility = Visibility.Collapsed;
      grpAdd.Visibility = Visibility.Collapsed;
  }
  ```
  Nếu là Quản lý: Ẩn nút "Mua / gia hạn gói" và ẩn toàn bộ khung "Đăng ký member tại quầy" vì đây là công việc đặc thù của nhân viên Lễ tân tại quầy dịch vụ.

**Hàm `LoadMembers()`**:
```csharp
45:             var members = query.OrderBy(m => m.MemberId).AsNoTracking().ToList();
46:             var subs = _db.Subscriptions.Include(s => s.Package).AsNoTracking().ToList();
47: 
48:             var rows = members.Select(m =>
49:             {
50:                 var s = Rules.CurrentOrLatest(subs.Where(x => x.MemberId == m.MemberId));
51:                 return new
52:                 {
53:                     m.MemberId,
54:                     m.FullName,
55:                     m.Phone,
56:                     HasAccount = m.UserId != null ? "Có" : "Chưa có",
57:                     PackageName = s?.Package.PackageName ?? "—",
58:                     EndDate = s?.EndDate,
59:                     Status = Rules.SubscriptionText(s)
60:                 };
61:             }).ToList();
```
- Dòng 45-46: Đọc toàn bộ danh sách hội viên và danh sách gói tập với chế độ `AsNoTracking()`.
- Dòng 48-61: Với mỗi hội viên, gọi `Rules.CurrentOrLatest` để tìm gói tập còn hiệu lực hoặc gói gần nhất. Ghép nhãn trạng thái `"Còn hạn"`, `"Hết hạn"`, `"Chưa có gói"` qua `Rules.SubscriptionText`.
- Hiển thị rõ cột `HasAccount`: `"Có"` (đã tạo tài khoản online) hoặc `"Chưa có"` (đăng ký quầy).

**Hàm `btnBuy_Click` (Chuyển trang mua gói)**:
- Lấy `memberId` của dòng được chọn, gọi `MainWindow.Instance?.ShowPage(new BuyPage(memberId))` để chuyển ngay sang màn hình bán gói và chọn sẵn hội viên này.

**Hàm `btnAddMember_Click` (Đăng ký tại quầy - Luật A5)**:
- Kiểm tra Họ tên, Số điện thoại (bắt đầu bằng 0, 10-11 số), Email.
- Dòng 128-140 (Chống trùng hồ sơ):
  ```csharp
  var owner = AccountHelper.FindByPhone(_db, phone);
  if (owner != null)
  {
      Msg.Warn($"Số điện thoại {phone} đã thuộc về member \"{owner.FullName}\" (#{owner.MemberId})...");
      txtSearch.Text = phone;
      LoadMembers();
      return;
  }
  ```
  Nếu số điện thoại đã được dùng, chặn tạo mới và tự động gán số điện thoại vào ô tìm kiếm rồi nạp lại danh sách để Lễ tân thấy ngay hồ sơ cũ.
- Dòng 142-150: Thêm thực thể `Member` với `UserId = null` (chưa có tài khoản). Lưu CSDL và ghi log `"CreateMember"`.

---

## 5.4. `CoachPage.xaml.cs` – Huấn luyện viên xem lớp & Học viên

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/CoachPage.xaml.cs` (87 dòng).
Dành riêng cho Coach theo dõi các lớp học được phân công.
- Dòng 27-48: Hàm `LoadClasses()`:
  - Lấy `coachId` từ `Session.CurrentCoach?.CoachId ?? 0`.
  - Truy vấn các lớp đang mở (`Status != "Cancelled"`) của HLV này. Đưa vào ComboBox `cbxClass`. Nếu HLV chưa được phân công lớp nào, hiển thị thông báo `Msg.Info("Bạn chưa phụ trách lớp nào.")`.
- Dòng 57-85: Hàm `LoadStudents()`:
  - Khi HLV chọn một lớp trên ComboBox: Đọc thông tin chi tiết lớp học (`Include(Room).Include(Slot)`).
  - Cập nhật thông tin lên giao diện: Lịch học qua `Rules.ScheduleText(c)`, Tên phòng `c.Room.RoomName`, Sĩ số hiện tại so với sức chứa `members.Count / c.Capacity`.
  - Truy vấn danh sách học viên đăng ký (`Status == "Registered"`), ghép kèm trạng thái gói thành viên (`PackageStatus`) để HLV biết học viên nào còn hạn gói. Cắt 8 dòng/trang đưa lên DataGrid `dgStudents`.

---

## 5.5. `SportsRoomsPage.xaml.cs` – Danh mục Bộ môn & Phòng tập

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/SportsRoomsPage.xaml.cs` (165 dòng).
Màn hình quản lý 2 danh mục nền tảng: Bộ môn (`Sports`) và Phòng tập (`Rooms`).
- **Quản lý Bộ môn (`Sports`)**:
  - `LoadSports()`: Đọc danh sách bộ môn kèm số lớp học đang mở thuộc bộ môn đó (`ClassCount = s.Classes.Count`).
  - `btnAddSport_Click`: Kiểm tra không để trống tên bộ môn; kiểm tra chống trùng `_db.Sports.Any(s => s.SportName == name)`. Thêm mới và ghi log `"CreateSport"`.
  - `btnDeleteSport_Click`: Kiểm tra ràng buộc toàn vẹn: nếu `_db.Classes.Any(c => c.SportId == id)` (đang có lớp học dùng bộ môn này) thì cấm xóa và thông báo: `"Bộ môn đang có lớp, không thể xóa."`. Nếu an toàn, mở hộp thoại xác nhận `Msg.Confirm` trước khi xóa.
- **Quản lý Phòng tập (`Rooms`)**:
  - `LoadRooms()`: Hiển thị tên phòng, sức chứa (`Capacity`) và số lớp đang dùng phòng.
  - `btnAddRoom_Click`: Kiểm tra sức chứa bắt buộc phải là số nguyên lớn hơn 0 (`capacity > 0`), kiểm tra chống trùng tên phòng. Thêm vào CSDL và ghi log `"CreateRoom"`.
  - `btnDeleteRoom_Click`: Kiểm tra nếu phòng đang có lớp học hoạt động thì chặn xóa. Yêu cầu xác nhận trước khi thực hiện xóa khỏi bảng `Rooms`.

---

## 5.6. `PackagesPage.xaml.cs` – Thiết lập Gói thành viên & Mở/Ngừng bán

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/PackagesPage.xaml.cs` (111 dòng).
Quản lý các gói sản phẩm của trung tâm thể thao.
- **Hàm `btnToggle_Click` (Ngừng bán / Mở bán lại - Luật P1)**:
  - Dòng 40-63: Gói thành viên sau khi đã có khách hàng mua thì **tuyệt đối không xóa** khỏi CSDL để bảo vệ lịch sử thanh toán và hợp đồng.
  - Chức năng chỉ đảo ngược cờ hoạt động: `pkg.IsActive = !pkg.IsActive;`. Gói ngừng bán sẽ không còn xuất hiện trên màn hình Mua gói của khách và lễ tân.
  - Ghi nhận nhật ký kiểm toán `"TogglePackage"`.
- **Hàm `btnAdd_Click` (Thêm gói mới)**:
  - Dòng 74-79: Chuẩn hóa tên gói bằng `AccountHelper.Normalize(name)`. So sánh không phân biệt hoa thường và dấu thanh để ngăn chặn việc tạo trùng lặp (ví dụ `"Gói 3 tháng"` tương đương `"goi 3 thang"`).
  - Dòng 80-89: Kiểm tra thời hạn `DurationDays > 0` và mức giá `Price >= 0`.
  - Thêm thực thể `MembershipPackage`, lưu CSDL và ghi log `"CreatePackage"`.

---

## 5.7. `ClassesPage.xaml.cs` – Xếp lịch lớp học, Lọc ca trống & Chặn xung đột

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/ClassesPage.xaml.cs` (352 dòng).
Trang phức tạp nhất trong phân hệ quản lý lớp, tích hợp thuật toán lọc ca học thông minh để người dùng không thể tạo lịch trùng.

**Thuật toán tự động phát hiện Thứ & Ca học còn trống (`RefreshFreeDays` & `RefreshFreeSlots`)**:
```csharp
123:         private void RefreshFreeDays()
124:         {
125:             if (cbxCoach.SelectedValue is not int coachId || cbxRoom.SelectedValue is not int roomId) return;
126: 
127:             var busy = Rules.BusySet(_db, roomId, coachId);
128:             var slots = _db.TimeSlots.AsNoTracking().ToList();
129:             var days = Enumerable.Range(1, 7)
130:                 .Where(d => slots.Any(s => !busy.Contains((d, s.SlotId))))
131:                 .Select(d => new { Day = d, Name = Rules.DayNames[d] })
132:                 .ToList();
```
- **Bước 1. Lọc thứ trong tuần còn ca trống**:
  - Khi Quản lý chọn một HLV và một Phòng tập, hàm gọi `Rules.BusySet(_db, roomId, coachId)` để lấy toàn bộ các cặp `(Thứ, Ca)` đã bị chiếm.
  - Quét 7 thứ trong tuần (1 đến 7): Chỉ giữ lại những thứ mà vẫn còn ít nhất một ca học chưa bị chiếm `!busy.Contains((d, s.SlotId))`.
  - Đổ danh sách thứ còn trống vào ComboBox `cbxDay`. Nếu không còn thứ nào trống, vô hiệu hóa nút bấm và hiện thông báo `"Không còn ca trống cho HLV và phòng này"`.

```csharp
153:         private void RefreshFreeSlots()
154:         {
155:             if (cbxCoach.SelectedValue is not int coachId || cbxRoom.SelectedValue is not int roomId || cbxDay.SelectedValue is not int day)
156:             {
157:                 cbxSlot.ItemsSource = null;
158:                 return;
159:             }
160: 
161:             var busy = Rules.BusySet(_db, roomId, coachId);
162:             var slots = _db.TimeSlots.AsNoTracking().OrderBy(s => s.SlotId).ToList()
163:                 .Where(s => !busy.Contains((day, s.SlotId)))
164:                 .Select(s => new { s.SlotId, Text = $"{s.SlotName} ({s.StartTime:HH:mm}-{s.EndTime:HH:mm})" })
165:                 .ToList();
```
- **Bước 2. Lọc ca học cụ thể còn trống**:
  - Khi chọn một thứ cụ thể trong `cbxDay`, nạp danh sách 6 ca học và lọc bỏ những ca đã bị trùng lịch với HLV hoặc phòng tập.
  - Đổ các ca thực sự rảnh vào ComboBox `cbxSlot`. Nhờ cơ chế này, người dùng trên giao diện chỉ có thể chọn những khung giờ hoàn toàn hợp lệ.

**Hàm `btnAddClass_Click` (Thêm lớp học - Luật C1, C2, C3)**:
- Kiểm tra Tên lớp, Mã lớp, Sĩ số.
- Dòng 281-285: Kiểm tra khoảng ngày: Ngày kết thúc không được trước ngày bắt đầu (`end >= start`).
- Dòng 287-292: **Kiểm tra sức chứa (Luật C1)**: Đọc sức chứa tối đa của phòng tập `room.Capacity`. Nếu sĩ số nhập vào vượt quá sức chứa của phòng tập thì cảnh báo và chặn lại.
- Dòng 293-297: Kiểm tra cấm trùng mã lớp `_db.Classes.Any(c => c.ClassCode == classCode)`.
- Dòng 300-306: Gọi lại `Rules.ClassConflictMessage` để kiểm tra lớp trùng lần cuối (phòng trường hợp có phiên khác vừa tạo lớp).
- Thêm đối tượng `Class` vào `_db.Classes`, lưu CSDL, ghi log kiểm toán `"CreateClass"`.

**Hàm `btnCancelClass_Click` (Hủy lớp học - Luật C4)**:
- Lấy `classId` dòng đang chọn.
- Dòng 216-221: Gọi `Rules.CancelClassBlockedReason(_db, cl)`. Nếu lớp đang có học viên đăng ký (`Registered`), hiển thị cảnh báo từ chối hủy.
- Nếu không có học viên, hỏi xác nhận `Msg.Confirm`, chuyển trạng thái `cl.Status = "Cancelled"`, giải phóng ca học để tái sử dụng.

---
## 5.8. `BuyPage.xaml.cs` – Mua gói tập, Gia hạn nối tiếp & Hóa đơn

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/BuyPage.xaml.cs` (199 dòng).
Màn hình thực hiện chức năng bán gói tập cho Hội viên (Member tự mua hoặc Lễ tân mua hộ tại quầy).

```csharp
38:         private void LoadMemberPicker(int? presetMemberId)
39:         {
40:             if (Session.Role == "Member")
41:             {
42:                 panelMember.Visibility = Visibility.Collapsed;
43:                 return;
44:             }
45: 
46:             cbxMember.ItemsSource = _db.Members.AsNoTracking().OrderBy(m => m.MemberId).ToList();
47:             cbxMember.DisplayMemberPath = nameof(Member.FullName);
48:             cbxMember.SelectedValuePath = nameof(Member.MemberId);
49:             if (presetMemberId != null)
50:                 cbxMember.SelectedValue = presetMemberId;
51:             if (cbxMember.SelectedIndex < 0)
52:                 cbxMember.SelectedIndex = 0;
53:         }
```
- Dòng 40-44: Nếu người đăng nhập là `Member`, ẩn hoàn toàn khung chọn hội viên `panelMember` (vì hội viên chỉ được phép mua cho chính mình). Nếu là `Receptionist`, nạp danh sách hội viên vào ComboBox và chọn sẵn `presetMemberId` nếu được chuyển tiếp từ màn hình `MembersPage`.

**Hàm `ShowCurrent()` (Hiển thị gói hiện tại & gói chờ)**:
```csharp
72:         private void ShowCurrent()
73:         {
74:             int memberId = GetMemberId();
...
82:             var cur = Rules.ActiveSubscription(_db, memberId);
83:             if (cur != null)
84:             {
85:                 string pkg = _db.MembershipPackages.Find(cur.PackageId)!.PackageName;
86:                 txtCurrent.Text = $"{member.FullName} · đang có {pkg}, còn hạn đến {cur.EndDate:dd/MM/yyyy}";
87: 
88:                 // Gói đã mua nối tiếp: chưa dùng, tự có hiệu lực khi gói hiện tại hết hạn
89:                 var queued = Rules.QueuedSubscriptions(_db, memberId);
90:                 if (queued.Count > 0)
91:                 {
92:                     txtCurrent.Text += "\nĐã mua nối tiếp: " + string.Join("; ",
93:                         queued.Select(q => $"{q.Package.PackageName} ({q.StartDate:dd/MM/yyyy} → {q.EndDate:dd/MM/yyyy})"));
94:                 }
95:                 return;
96:             }
```
- Dòng 82-96: Nếu hội viên đang có gói tập còn hạn, hiển thị tên gói và ngày hết hạn. Đồng thời gọi `Rules.QueuedSubscriptions` để hiển thị toàn bộ danh sách các gói đã mua nối tiếp trước đó đang trong hàng đợi chờ kích hoạt.

**Hàm `ShowPreview()` (Xem trước ngày hiệu lực - Luật P3)**:
```csharp
107:         private void ShowPreview()
108:         {
109:             int? packageId = GridHelper.SelectedId(dgPackages, "PackageId");
110:             int memberId = GetMemberId();
...
118:             var pkg = _db.MembershipPackages.AsNoTracking().First(p => p.PackageId == packageId);
119:             var start = Rules.NextStartDate(_db, memberId);
120:             var end = start.AddDays(pkg.DurationDays - 1);
121: 
122:             txtPreview.Text = $"{pkg.PackageName}: hiệu lực {start:dd/MM/yyyy} → {end:dd/MM/yyyy}"
123:                 + (start > Rules.Today ? " (nối tiếp: dùng sau khi gói hiện có hết hạn)" : "")
124:                 + $"\nSố tiền: {pkg.Price:N0} ₫";
125:             btnConfirm.IsEnabled = true;
126:         }
```
- Tính toán ngày bắt đầu mới thông qua `Rules.NextStartDate`.
- Ngày kết thúc tính bằng: `end = start.AddDays(pkg.DurationDays - 1)`.
- Hiển thị thông báo xem trước rõ ràng cho người dùng: nếu `start > Today`, gắn thêm nhãn `"nối tiếp: dùng sau khi gói hiện có hết hạn"`.

**Hàm `btnConfirm_Click` (Thực hiện thanh toán - Luật P2, Y1, Y2)**:
```csharp
160:                 var start = Rules.NextStartDate(_db, memberId);
161: 
162:                 var sub = new Subscription
163:                 {
164:                     MemberId = memberId,
165:                     PackageId = pkg.PackageId,
166:                     StartDate = start,
167:                     EndDate = start.AddDays(pkg.DurationDays - 1),
168:                     Status = "Active"
169:                 };
170:                 var pay = new Payment
171:                 {
172:                     InvoiceNo = Rules.NextInvoiceNo(_db),
173:                     MemberId = memberId,
174:                     Subscription = sub,
175:                     Amount = pkg.Price,
176:                     PaymentMethod = (string)cbxMethod.SelectedValue,
177:                     PaidAt = DateTime.Now,
178:                     ReceivedBy = Session.CurrentUser?.UserId,
179:                     Status = "Paid",
180:                     Note = "Mua " + pkg.PackageName
181:                 };
182: 
183:                 // Một lần SaveChanges = một transaction: tạo gói và hóa đơn cùng lúc, lỗi thì không lưu gì cả
184:                 _db.Subscriptions.Add(sub);
185:                 _db.Payments.Add(pay);
186:                 _db.SaveChanges();
```
- Dòng 172 (Luật Y1): Tự động sinh mã hóa đơn tăng dần `Rules.NextInvoiceNo(_db)`.
- Dòng 183-186 (Luật Y2): Thêm đồng thời cả thực thể `Subscription` và `Payment` vào DbContext. Một lệnh `_db.SaveChanges()` duy nhất tạo ra một giao dịch nguyên tử (Atomic Database Transaction): Nếu tạo hóa đơn lỗi thì gói thành viên cũng không được tạo.
- Dòng 188: Ghi nhật ký kiểm toán `"Payment"`. Nạp lại giao diện và thông báo thành công.

---

## 5.9. `BookingPage.xaml.cs` – Đăng ký tham gia lớp học

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/BookingPage.xaml.cs` (197 dòng).
Màn hình phục vụ Member tự đặt lớp hoặc Lễ tân đăng ký hộ khách tại quầy.
- Dòng 54-68: Hàm `ShowNote()`: Kiểm tra gói của hội viên qua `Rules.ActiveSubscription`. Nếu chưa có gói còn hạn, hiển thị dòng thông báo màu xám: `"chưa có gói còn hạn, chưa đăng ký lớp được"`.

**Hàm `LoadClasses()` (Tổng hợp sĩ số & Trạng thái cá nhân)**:
```csharp
88:             var counts = _db.Registrations
89:                 .Where(r => r.Status == "Registered")
90:                 .GroupBy(r => r.ClassId)
91:                 .Select(g => new { ClassId = g.Key, Count = g.Count() })
92:                 .ToDictionary(x => x.ClassId, x => x.Count);
93:             var mine = _db.Registrations
94:                 .Where(r => r.MemberId == memberId && r.Status == "Registered")
95:                 .Select(r => r.ClassId)
96:                 .ToHashSet();
```
- Dòng 88-92: Sử dụng LINQ `GroupBy(r => r.ClassId)` đếm số lượng học viên đang ghi danh (`Status == "Registered"`) trong từng lớp, chuyển thành Dictionary để tra cứu với độ phức tạp $O(1)$.
- Dòng 93-96: Lấy danh sách các lớp hội viên hiện tại đã đăng ký và đưa vào `HashSet<int> mine`.
- Dòng 98-114: Nạp danh sách lớp: Cột `Slots` hiển thị dạng `"{n}/{Capacity} · đầy"`. Cột `MyStatus` hiển thị nhãn `"Đã đăng ký"` nếu lớp nằm trong tập `mine`.

**Hàm `btnRegister_Click` & `btnCancel_Click` (Luật B1-B8)**:
- `btnRegister_Click`: Lấy `memberId` và `classId`, gọi `Rules.RegisterClass(_db, memberId, classId.Value)`. Nếu vi phạm bất kỳ luật nào (chưa có gói, lớp đầy, trùng lịch thứ/ca), hàm trả về chuỗi thông báo lỗi để `Msg.Warn` hiển thị. Nếu thành công, nạp lại danh sách và báo thành công.
- `btnCancel_Click`: Gọi `Rules.CancelRegistration(_db, memberId, classId.Value)` thực thi luật B6 (chỉ được hủy nếu cách giờ học hơn 24 giờ).

---

## 5.10. `MySchedulePage.xaml.cs` – Thời khóa biểu tuần cá nhân & Kết quả FAP

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/MySchedulePage.xaml.cs` (221 dòng).
Màn hình mang đậm phong cách quản lý đào tạo FAP: Hiển thị lưới ma trận lịch học 7 cột (Thứ Hai đến Chủ Nhật) x 6 hàng (6 ca học trong ngày), kèm kết quả điểm danh của từng buổi học cụ thể.

```csharp
23:             _weekStart = MondayOf(DateTime.Today);
24:             BuildSchedule();
```
- Dòng 27: Hàm `MondayOf(DateTime d) => d.Date.AddDays(-(Rules.IsoDay(d) - 1));`: Thuật toán tìm ngày Thứ Hai đầu tuần của bất kỳ ngày nào được chọn.

**Giải thích thuật toán dựng ma trận lịch `BuildSchedule()`**:
```csharp
56:             var attendance = _db.Attendances.AsNoTracking()
57:                 .Where(a => a.Registration.MemberId == member.MemberId && a.SessionDate >= from && a.SessionDate <= to)
58:                 .Select(a => new { a.Registration.ClassId, a.SessionDate, a.Status })
59:                 .ToList()
60:                 .GroupBy(a => (a.ClassId, a.SessionDate))
61:                 .ToDictionary(g => g.Key, g => g.First().Status);
```
- Dòng 56-61: Truy vấn toàn bộ các bản ghi điểm danh của hội viên trong tuần (`SessionDate >= from && SessionDate <= to`). Gom nhóm theo cặp `(ClassId, SessionDate)` đưa vào từ điển `attendance` để tra cứu nhanh kết quả điểm danh `"Present"` hay `"Absent"`.
- Dòng 63-74: Khởi tạo lưới `Grid`:
  - 1 cột nhãn ca (rộng 100px) + 7 cột cho 7 ngày trong tuần (độ rộng tỷ lệ `1*`).
  - 1 hàng tiêu đề + 6 hàng tương ứng với 6 ca học (chiều cao tối thiểu 96px/ô).
- Dòng 77-83: Đổ hàng tiêu đề ngày: Nếu cột trùng với ngày hôm nay (`day.Date == DateTime.Today`), đổi màu nền xanh nổi bật để người dùng dễ quan sát.

**Thuật toán điền dữ liệu từng ô lớp học (Dòng 86-121)**:
- Chạy 2 vòng lặp lồng nhau: vòng ngoài duyệt 6 ca học `i = 0..5`, vòng trong duyệt 7 ngày `d = 1..7`.
- Dòng 104-106: Tìm lớp học của hội viên có `DayOfWeek == d` và `SlotId == slot.SlotId`, nằm trong khoảng thời gian diễn ra của lớp (`StartDate <= date && EndDate >= date`).
- Dòng 114-118: Kiểm tra kết quả điểm danh:
  - Nếu kết quả là `"Present"`: hiển thị huy hiệu xanh `"✓ Có mặt"`, tăng biến đếm `present++`.
  - Nếu kết quả là `"Absent"`: hiển thị huy hiệu đỏ `"✗ Vắng"`, tăng biến đếm `absent++`.
  - Nếu ngày học đã qua trong quá khứ mà chưa có dữ liệu điểm danh: hiển thị nhãn xám `"Chưa điểm danh"`, tăng `pending++`.
- Dòng 123-126: Hiển thị dòng tổng kết cuối trang: `"Trong tuần: {present} buổi có mặt, {absent} buổi vắng, {pending} buổi chưa điểm danh."`.

**Điều hướng thời gian tuần**:
- Dòng 207-211: Nút "Tuần trước" (`_weekStart.AddDays(-7)`), "Tuần sau" (`_weekStart.AddDays(7)`), "Tuần này" (`MondayOf(DateTime.Today)`).
- Dòng 214-219: `dpJump_SelectedDateChanged`: Cho phép người dùng chọn một ngày bất kỳ trên DatePicker để nhảy ngay tới tuần chứa ngày đó.

---

## 5.11. `AttendancePage.xaml.cs` – Huấn luyện viên điểm danh buổi học

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/AttendancePage.xaml.cs` (220 dòng).
Màn hình nghiệp vụ dành riêng cho Coach để ghi nhận chuyên cần của học viên.

**Mô hình dữ liệu hai chiều `AttendanceRow`**:
```csharp
11:     public class AttendanceRow : INotifyPropertyChanged
12:     {
13:         public int RegistrationId { get; set; }
14:         public string Student { get; set; } = "";
15:         private bool _present;
16:         public bool Present
17:         {
18:             get => _present;
19:             set
20:             {
21:                 if (_present == value) return;
22:                 _present = value;
23:                 PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Present)));
24:             }
25:         }
26:         public event PropertyChangedEventHandler? PropertyChanged;
27:     }
```
- Lớp `AttendanceRow` triển khai giao diện `INotifyPropertyChanged`. Khi thuộc tính `Present` thay đổi, sự kiện `PropertyChanged` kích hoạt. Giúp nút bấm "Có mặt tất cả" và "Vắng tất cả" có thể cập nhật trạng thái hiển thị của toàn bộ các CheckBox trên giao diện ngay lập tức mà không cần nạp lại trang.

**Quy trình tải buổi học và học viên**:
- Dòng 55-71 (`LoadClasses`): Chỉ lọc các lớp mà HLV đang đăng nhập phụ trách (`c.CoachId == coachId`).
- Dòng 76-96 (`cbxClass_SelectionChanged`): Gọi hàm `Rules.SessionDates(c)` sinh danh sách tối đa 12 buổi học hợp lệ trong quá khứ/hiện tại của lớp. Đổ vào ComboBox `cbxSession` và chọn sẵn buổi gần nhất. Coach **không phải tự gõ ngày** nên hoàn toàn không thể chọn sai thứ của lớp.
- Dòng 104-134 (`LoadRows`):
  - Lấy danh sách học viên đăng ký lớp.
  - Tìm các bản ghi điểm danh đã lưu trong bảng `Attendances` của ngày đó.
  - **Quy tắc công thái học**: Nếu buổi học chưa từng được điểm danh, mặc định toàn bộ học viên là `Present = true` ("Có mặt"). HLV chỉ cần tích bỏ chọn những bạn vắng mặt, giúp thao tác điểm danh nhanh gấp nhiều lần.
  - Gọi `UpdateSummary()` hiển thị dòng tổng kết: `"Có mặt {present}/{total} · Vắng {absent}"`.

**Hàm `btnSave_Click` (Lưu kết quả điểm danh)**:
```csharp
188:             try
189:             {
190:                 foreach (var row in _rows)
191:                 {
192:                     string status = row.Present ? "Present" : "Absent";
193:                     var existing = _db.Attendances.FirstOrDefault(a => a.RegistrationId == row.RegistrationId && a.SessionDate == date);
194:                     if (existing != null)
195:                     {
196:                         existing.Status = status;
197:                         existing.CheckedInBy = Session.CurrentUser?.UserId;
198:                     }
199:                     else
200:                     {
201:                         _db.Attendances.Add(new Attendance
202:                         {
203:                             RegistrationId = row.RegistrationId,
204:                             SessionDate = date,
205:                             Status = status,
206:                             CheckedInBy = Session.CurrentUser?.UserId
207:                         });
208:                     }
209:                 }
210:                 _db.SaveChanges();
211:                 AuditHelper.Log(_db, "Attendance", $"Điểm danh {c.ClassCode} ngày {date:dd/MM/yyyy}", "Classes", c.ClassId);
```
- Duyệt qua từng học viên trong danh sách `_rows`. Nếu đã có bản ghi trong CSDL thì cập nhật lại trạng thái `Status = status`. Nếu chưa có thì tạo mới thực thể `Attendance`.
- Lưu CSDL và ghi log kiểm toán hành động `"Attendance"`.

---

## 5.12. `PaymentsPage.xaml.cs` – Quản lý Hóa đơn & Xuất biên lai

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/PaymentsPage.xaml.cs` (144 dòng).
- Dòng 35-39: Phân quyền bảo mật: Nếu người dùng là `Member`, hệ thống tự động gắn bộ lọc `query = query.Where(p => p.MemberId == memberId)` để hội viên chỉ xem được hóa đơn của chính mình.
- Dòng 41-54: Hỗ trợ tìm kiếm theo mã hóa đơn, tên hội viên, và lọc theo khoảng ngày thanh toán (`dpFrom`, `dpTo`).
- Dòng 75-104 (`ShowInvoice`):
  - Khi người dùng click chọn một hóa đơn trên DataGrid, hệ thống nạp chi tiết bản ghi (`Include(Member)`, `Include(Subscription).ThenInclude(Package)`).
  - Sử dụng `StringBuilder` xây dựng mẫu hóa đơn thanh toán chuẩn hóa:
    ```
    HÓA ĐƠN INV-0001
    Member: Vũ Anh Tuấn
    Nội dung: Gói 1 tháng
    Hiệu lực: 01/10/2026 → 31/10/2026
    Phương thức: Chuyển khoản
    Số tiền: 500.000 ₫
    Ngày thu: 01/10/2026 08:30
    ```
  - Hiển thị mẫu hóa đơn lên vùng xem trước `txtInvoice`.
- Dòng 122-142 (`btnExport_Click` - Luật Y4): Mở hộp thoại `SaveFileDialog`, cho phép người dùng lưu nội dung hóa đơn ra tập tin văn bản `.txt` với mã hóa chuẩn `Encoding.UTF8`.

---

## 5.13. `AuditPage.xaml.cs` – Nhật ký kiểm toán hệ thống

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/AuditPage.xaml.cs` (100 dòng).
Màn hình dành riêng cho Manager để giám sát mọi hành vi trong hệ thống.
- Dòng 26-37 (`LoadData`):
  - Đổ danh sách người dùng từng phát sinh thao tác vào ComboBox `cbxUser`.
  - Đổ danh sách các hành động vào `cbxAction`, tự động dịch mã hành động sang tên tiếng Việt thông qua `Rules.ActionName`.
- Dòng 42-81 (`LoadLogs`):
  - Lọc theo từ khóa chi tiết (`Details`), lọc theo người thao tác (`User`), lọc theo loại hành động (`Action`), và khoảng ngày (`CreatedAt`).
  - Sắp xếp giảm dần theo thời gian `OrderByDescending(a => a.CreatedAt)` để thao tác mới nhất luôn hiển thị ở dòng đầu tiên.
  - Phân trang 8 dòng/trang đưa lên DataGrid `dgAudit`.

---

# PHẦN 6: BẢNG MA TRẬN ĐỐI CHIẾU MÃ LUẬT NGHIỆP VỤ (RULE MAPPING)

Bảng tổng hợp chi tiết vị trí hiện thực toàn bộ các quy tắc nghiệp vụ trong mã nguồn hệ thống:

| Mã luật | Tên quy tắc nghiệp vụ | Tệp mã nguồn | Phương thức / Vị trí code | Giải thích cơ chế hiện thực |
|:---:|---|---|---|---|
| **A1** | Xác thực đăng nhập, kiểm tra hash & khóa tài khoản | `LoginWindow.xaml.cs` | `btnLogin_Click` (L33-L43) | Tìm user theo username, so sánh hash SHA-256 (`PasswordHelper.Hash`), kiểm tra cờ `IsActive == true`. |
| **A2** | Quy tắc đăng ký tài khoản tự do (Online) | `RegisterWindow.xaml.cs` | `btnCreate_Click` (L38-L113) | Kiểm tra định dạng tên, SĐT 10-11 số bắt đầu bằng 0, mật khẩu $\ge$ 6 ký tự, mật khẩu $\ne$ username. |
| **A2 (gắn)**| Tự động gắn tài khoản vào hồ sơ đăng ký tại quầy | `AccountHelper.cs`<br>`RegisterWindow.xaml.cs` | `CheckPhoneForRegistration`<br>`btnCreate_Click` (L118-L138) | Phát hiện SĐT và họ tên khớp hồ sơ quầy chưa có User; mở `Msg.Confirm` xác nhận gắn và giữ nguyên gói, hóa đơn cũ. |
| **A3** | Cấm Quản lý tự khóa tài khoản hoặc tự đổi vai trò của mình | `UsersPage.xaml.cs` | `GetSelectedUser` (L98-L102) | Kiểm tra `id == Session.CurrentUser.UserId`, nếu trùng lập tức chặn và cảnh báo. |
| **A4** | Ghi nhận nhật ký kiểm toán cho các thao tác quan trọng | `AuditHelper.cs` | `Log` (L8-L19) | Lưu `UserId`, `Action`, `EntityName`, `EntityId`, `Details` và `CreatedAt` vào bảng `AuditLogs`. |
| **A5** | Chống trùng lặp số điện thoại hội viên | `AccountHelper.cs`<br>`SportsCenter.sql` | `FindByPhone`<br>`UX_Members_Phone` (L79) | Kiểm tra trước bằng code C#; chốt chặn cuối cùng bằng chỉ mục duy nhất Filtered Unique Index ở SQL Server. |
| **P1** | Ràng buộc gói tập: Giá $\ge$ 0, thời hạn > 0, không xóa | `PackagesPage.xaml.cs` | `btnAdd_Click`<br>`btnToggle_Click` | Kiểm tra tính hợp lệ dữ liệu; không cho phép xóa bản ghi mà chỉ bật/tắt cờ `IsActive`. |
| **P2** | Mua gói: Bắt đầu từ ngày hợp lệ, sinh thời hạn & hóa đơn | `BuyPage.xaml.cs` | `btnConfirm_Click` (L160-L186) | Tính ngày kết thúc `end = start + DurationDays - 1`, sinh hóa đơn `Payment` tương ứng. |
| **P3** | Gia hạn gói nối tiếp tự động không chồng lấn | `Rules.Packages.cs` | `NextStartDate` (L40-L50) | Tìm ngày kết thúc của gói xa nhất chưa hết hạn; gói mới bắt đầu vào ngày liền kề sau đó (`lastEnd.AddDays(1)`). |
| **P4** | Xác định gói tập còn hiệu lực thời gian thực | `Rules.Packages.cs` | `ActiveSubscription` (L11-L19) | Điều kiện: `Status != "Cancelled"` và `StartDate <= Today && EndDate >= Today`. |
| **P5** | Cảnh báo gói thành viên sắp hết hạn trong 7 ngày | `DashboardPage.xaml.cs` | `LoadExpiring` (L173-L183) | Lọc các gói có `EndDate <= Today.AddDays(7)` hiển thị lên bảng cảnh báo của Quản lý. |
| **C1** | Ràng buộc lớp học: Sĩ số $\le$ sức chứa phòng, ngày hợp lệ | `ClassesPage.xaml.cs` | `btnAddClass_Click` (L281-L292) | Kiểm tra `end >= start`, kiểm tra `capacity <= room.Capacity`, cấm trùng mã lớp. |
| **C2** | Chống trùng phòng học theo thứ và ca | `Rules.Classes.cs` | `ClassConflictMessage` (L26-L30) | Quét các lớp đang mở: cấm trùng cặp `(DayOfWeek, SlotId, RoomId)`. |
| **C3** | Chống trùng lịch Huấn luyện viên theo thứ và ca | `Rules.Classes.cs` | `ClassConflictMessage` (L31-L35) | Quét các lớp đang mở: cấm một HLV dạy 2 lớp ở cùng một cặp `(DayOfWeek, SlotId)`. |
| **C4** | Chặn hủy lớp học khi vẫn còn học viên đăng ký | `Rules.Classes.cs` | `CancelClassBlockedReason` (L41-L47) | Đếm số lượng học viên đang `Registered` trong lớp; nếu $> 0$ thì từ chối hủy. |
| **B1** | Hội viên bắt buộc phải có gói tập còn hạn mới được đặt lớp | `Rules.Booking.cs` | `RegisterClass` (L47-L49) | Kiểm tra `ActiveSubscription(db, memberId) != null` trước khi cho phép ghi danh. |
| **B2** | Lớp học phải đang mở và chưa kết thúc | `Rules.Booking.cs` | `RegisterClass` (L50-L52) | Kiểm tra `Status == "Open"` và `EndDate >= Today`. |
| **B3** | Kiểm tra sĩ số lớp chưa vượt quá sức chứa | `Rules.Booking.cs` | `RegisterClass` (L56-L59) | Đếm số học viên đăng ký hiện tại so với `Capacity` của lớp. |
| **B4** | Mỗi hội viên chỉ có duy nhất 1 đăng ký hoạt động trong 1 lớp | `Rules.Booking.cs`<br>`SportsCenter.sql` | `RegisterClass` (L53-L55)<br>`UX_Registrations_Active` | Chặn bằng code LINQ và chốt chặn bằng Filtered Unique Index ở database. |
| **B5** | Không cho phép đăng ký 2 lớp học trùng thứ và ca | `Rules.Booking.cs` | `RegisterClass` (L60-L66) | Quét các lớp hội viên đã đăng ký, đối chiếu cặp `(DayOfWeek, SlotId)`. |
| **B6** | Chỉ được hủy đăng ký lớp khi cách buổi học hơn 24 giờ | `Rules.Booking.cs` | `CancelRegistration` (L84-L99) | Tính `NextSession(c)`; nếu `next - Now < 24 giờ` thì chặn hủy. |
| **B7** | Lễ tân đăng ký hộ phải tuân thủ đủ quy tắc và lưu vết | `BookingPage.xaml.cs` | `btnRegister_Click` | Truyền `memberId` được chọn, lưu `RegisteredBy = Session.CurrentUser.UserId`. |
| **B8** | Phân quyền: Member chỉ thao tác trên dữ liệu của chính mình | `BookingPage.xaml.cs`<br>`PaymentsPage.xaml.cs` | `GetMemberId`<br>`LoadPayments` | Tự động lấy `Session.CurrentMember.MemberId`, ẩn các ô chọn hội viên khác. |
| **Y1** | Số tiền $> 0$, phương thức hợp lệ, sinh mã `INV-xxxx` | `Rules.cs` | `NextInvoiceNo` (L81-L91) | Vòng lặp kiểm tra CSDL đảm bảo sinh mã `INV-0001`, `INV-0002`... duy nhất. |
| **Y2** | Giao dịch nguyên tử khi mua gói (Atomic Transaction) | `BuyPage.xaml.cs` | `btnConfirm_Click` (L183-L186) | Thêm `Subscription` và `Payment` cùng lúc, lưu bằng 1 lệnh `SaveChanges()` duy nhất. |
| **Y4** | Xuất biên lai hóa đơn ra tập tin văn bản | `PaymentsPage.xaml.cs` | `btnExport_Click` (L122-L142) | Định dạng văn bản tiếng Việt UTF-8 và lưu ra file `.txt` qua `SaveFileDialog`. |
| **R1-R4**| Thống kê KPI, doanh thu hóa đơn Paid, lấp đầy & biểu đồ | `DashboardPage.xaml.cs` | `LoadReport` (L45-L74) | Chỉ tính doanh thu của các hóa đơn có `Status == "Paid"`, vẽ biểu đồ cột 6 tháng trên Canvas. |
| **Điểm danh** | Chỉ điểm danh học viên Registered, sinh tối đa 12 buổi học | `Rules.Booking.cs`<br>`AttendancePage.xaml.cs` | `SessionDates`<br>`LoadRows` | Lùi ngày đúng thứ của lớp trong khoảng StartDate..EndDate, giới hạn 12 buổi gần nhất. |

---
*Tài liệu được khởi tạo và biên soạn toàn diện theo cấu trúc mã nguồn thực tế của dự án Sports Center.*
