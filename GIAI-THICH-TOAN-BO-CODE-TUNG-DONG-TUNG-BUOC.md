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

Tập tin nằm tại `SportsCenterApp/SportsCenter/Helpers/PasswordHelper.cs` (36 dòng).

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
**Giải thích từng dòng & từng bước (Chuẩn đề thi PRN212)**:
- **Dòng 11**: Khai báo lớp tiện ích tĩnh `public static class PasswordHelper`.
- **Dòng 15**: Hàm `public static string Hash(string password)` nhận vào mật khẩu plain text và trả về chuỗi Hex in hoa.
- **Dòng 17-21 (Bước 1)**: Kiểm tra `string.IsNullOrEmpty(password)`, nếu chuỗi trống hoặc null thì trả về `string.Empty` để tránh ngoại lệ runtime.
- **Dòng 23-24 (Bước 2)**: `byte[] inputBytes = Encoding.UTF8.GetBytes(password);` – Chuyển đổi chuỗi mật khẩu sang mảng byte chuẩn UTF-8.
- **Dòng 26-27 (Bước 3)**: `byte[] hashBytes = SHA256.HashData(inputBytes);` – Sử dụng thuật toán SHA-256 sinh ra mảng 32 byte băm an toàn một chiều.
- **Dòng 29-30 (Bước 4)**: `string hexString = Convert.ToHexString(hashBytes);` – Đổi mảng 32 byte thành chuỗi ký tự Hexadecimal **IN HOA** gồm 64 ký tự (ví dụ: `BA3253876AED6BC044DADE2FE...`). Kết quả này khớp 100% với hàm `HASHBYTES('SHA2_256', '123456')` của SQL Server trong tập tin `SportsCenter.sql`.
- **Dòng 32-33 (Bước 5)**: Trả về chuỗi `hexString`.

---

## 3.2. `Session.cs` – Lưu vết thông tin người dùng đang hoạt động

Tập tin nằm tại `SportsCenterApp/SportsCenter/Helpers/Session.cs` (79 dòng).

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
**Giải thích từng dòng & từng bước**:
- **Dòng 11**: Khai báo lớp `public static class Session` duy trì trạng thái đăng nhập toàn cục trong suốt phiên chạy của ứng dụng.
- **Dòng 14-20**: Khai báo 3 thuộc tính:
  - `CurrentUser`: Bản ghi `User` đang đăng nhập.
  - `CurrentMember`: Hồ sơ hội viên nếu tài khoản có vai trò là Member.
  - `CurrentCoach`: Hồ sơ huấn luyện viên nếu tài khoản có vai trò là Coach.
- **Dòng 23-34**: Thuộc tính `Role` với khối `get` rõ ràng: Trả về `CurrentUser.Role` nếu đã đăng nhập, ngược lại trả về chuỗi rỗng `string.Empty`.
- **Dòng 39-66**: Phương thức `public static void SignIn(SportsCenterContext db, User user)`:
  - **Dòng 42 (Bước 1)**: Gán `CurrentUser = user`.
  - **Dòng 44-52 (Bước 2)**: Nếu `user.Role == "Member"`, truy vấn bảng `Members` với chế độ `AsNoTracking()` tìm bản ghi theo `user.UserId`. Ngược lại gán `null`.
  - **Dòng 54-62 (Bước 3)**: Nếu `user.Role == "Coach"`, truy vấn bảng `Coaches` theo `user.UserId`. Ngược lại gán `null`.
  - **Dòng 64-65 (Bước 4)**: Tự động ghi nhật ký đăng nhập hệ thống qua `AuditHelper.Log(db, "Login", "Đăng nhập hệ thống")`.
- **Dòng 71-77**: Phương thức `public static void Clear()` xóa sạch dữ liệu phiên làm việc khi người dùng bấm Đăng xuất.

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

Tập tin nằm tại `SportsCenterApp/SportsCenter/Helpers/AccountHelper.cs` (218 dòng).
- **Hàm `Normalize(string text)`** (Dòng 24-52):
  - Bước 1: Kiểm tra rỗng qua `string.IsNullOrWhiteSpace(text)`.
  - Bước 2: Cắt khoảng trắng hai đầu bằng `.Trim()`, chuyển toàn bộ chuỗi về chữ thường `.ToLowerInvariant()`, chuyển ký tự `đ` thành `d`.
  - Bước 3-4: Sử dụng chuẩn phân rã `NormalizationForm.FormD` để tách các dấu thanh âm (sắc, huyền, hỏi, ngã, nặng) ra khỏi nguyên âm, sau đó lọc bỏ các ký tự thuộc nhóm `UnicodeCategory.NonSpacingMark`.
  - Bước 5: Cắt bỏ khoảng trắng thừa ở hai đầu và giữa các từ bằng `.Split(' ', StringSplitOptions.RemoveEmptyEntries)`. Kết quả biến chuỗi `"  Nguyễn   Văn Đạt "` thành `"nguyen van dat"`. Giúp so sánh họ tên hội viên hoàn toàn không phụ thuộc vào dấu và cách viết hoa thường.
- **Hàm `FindByPhone(db, phone)`** (Dòng 58-73):
  - Tìm kiếm hội viên đang sở hữu số điện thoại này trong bảng `Members` qua `db.Members.AsNoTracking().FirstOrDefault(m => m.Phone == phone)`. Nhờ chỉ mục duy nhất `UX_Members_Phone`, mỗi số điện thoại chỉ gắn với tối đa một người.
- **Enum `PhoneCheck` & Hàm `CheckPhoneForRegistration(db, fullName, phone)`** (Dòng 78-111):
  - Kiểm tra số điện thoại khi một người dùng tự đăng ký online trên `RegisterWindow`:
    1. Nếu `FindByPhone(db, phone) == null`: Trả về `PhoneCheck.Free` (Số điện thoại hoàn toàn mới -> cho phép tạo hồ sơ mới).
    2. Nếu số điện thoại đã tồn tại trong bảng `Members`: gọi `FindCounterProfiles(db, fullName, phone)`. Nếu tìm thấy hồ sơ tại quầy (chưa có `UserId`) và có họ tên sau khi `Normalize` khớp với tên đăng ký -> Trả về `PhoneCheck.LinkCounterProfile` (Hỏi người dùng xác nhận để gắn tài khoản vào hồ sơ cũ).
    3. Ngược lại (số điện thoại đã có tài khoản rồi, hoặc họ tên không trùng với hồ sơ quầy) -> Trả về `PhoneCheck.AlreadyRegistered` (Từ chối đăng ký, chặn mạo danh số điện thoại).
- **Hàm kiểm tra định dạng**:
  - Dòng 116-126: `IsValidEmail(email)`: Dùng Regex kiểm tra cú pháp thư điện tử tiêu chuẩn `^[^@\s]+@[^@\s]+\.[^@\s]+$`.
  - Dòng 132-148: `UsernameError(username)`: Kiểm tra độ dài từ 3-30 ký tự, biểu thức chính quy `^[A-Za-z0-9._]+$` chỉ cho phép chữ cái không dấu, chữ số, dấu chấm và gạch dưới (cấm khoảng trắng).
  - Dòng 154-171: `FullNameError(fullName)`: Độ dài từ 2-100 ký tự, sử dụng Regex `^\p{L}[\p{L}\p{M} .'’-]*$` hỗ trợ toàn bộ bảng chữ cái Unicode tiếng Việt có dấu, cấm hoàn toàn chữ số và ký tự lạ.
- **Hàm `FindCounterProfiles(db, fullName, phone)`** (Dòng 176-192):
  - Lọc các hội viên có `UserId == null` (đăng ký tại quầy) và trùng SĐT trong CSDL, sau đó lọc tiếp trên bộ nhớ các bản ghi có họ tên sau khi `Normalize` trùng khớp hoàn toàn.
- **Hàm `CreateMemberAccount(...)`** (Dòng 199-250):
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

## 4.2. `LoginWindow.xaml.cs` – Xác thực đăng nhập & Phân luồng điều hướng (Chuẩn PRN212)

Tập tin nằm tại `SportsCenterApp/SportsCenter/LoginWindow.xaml.cs`.

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

**Giải thích từng bước & từng dòng (Chuẩn PRN212)**:
- **Dòng 15**: `private SportsCenterContext _db;` – Khởi tạo DbContext kết nối CSDL SQL Server.
- **Dòng 17-21 (Constructor)**: Khởi tạo giao diện và DbContext, tinh gọn chuẩn PRN (không chứa focus thừa).
- **Dòng 26-70 (`btnLogin_Click` - Luật A1)**:
  - Bước 1: Đọc `username` và `password`.
  - Bước 2: Kiểm tra bắt buộc nhập.
  - Bước 3: Tìm tài khoản trong DB theo Username.
  - Bước 4: So khớp mật khẩu băm `PasswordHelper.Hash(password)`.
  - Bước 5: Kiểm tra cờ `user.IsActive`. Nếu bị khóa thì từ chối đăng nhập.
  - Bước 6: Gọi `Session.SignIn(_db, user)` lưu phiên làm việc toàn cục.
  - Bước 7: Mở `MainWindow` và đóng `LoginWindow`.
  - Khối catch ngắn gọn: `MessageBox.Show("Đăng nhập thất bại: " + ex.Message);`.
- **Dòng 76-83 (`btnRegister_Click`)**: Mở `RegisterWindow` và đóng `LoginWindow`.

---

## 4.3. `RegisterWindow.xaml.cs` – Tự đăng ký tài khoản & Liên kết hồ sơ quầy (Chuẩn PRN212)

Tập tin nằm tại `SportsCenterApp/SportsCenter/RegisterWindow.xaml.cs`.

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

**Giải thích chi tiết từng bước & từng dòng**:
- **Dòng 15**: Khởi tạo DbContext.
- **Dòng 17-21 (Constructor)**: Tinh gọn 2 dòng khởi tạo giao diện và kết nối DB.
- **Dòng 26-33 (`ClearErrors`)**: Xóa trắng toàn bộ thông báo lỗi đỏ.
- **Dòng 38-190 (`btnCreate_Click` - Luật A2, A5)**: Validate dữ liệu, kiểm tra SĐT qua `AccountHelper.CheckPhoneForRegistration`, popup hỏi xác nhận gắn hồ sơ quầy, tạo tài khoản và lưu DB trong 1 lần `SaveChanges()`, tự động đăng nhập và chuyển sang `MainWindow`. Khối catch rút gọn ngắn gọn.
- **Dòng 196-201 (`btnBack_Click`)**: Mở lại `LoginWindow`.

---

## 4.4. `MainWindow.xaml.cs` – Điều hướng Menu động theo Role

Tập tin nằm tại `SportsCenterApp/SportsCenter/MainWindow.xaml.cs` (188 dòng).

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
**Giải thích chi tiết từng dòng**:
- **Dòng 14**: `public static MainWindow? Instance { get; private set; }` – Thể hiện tĩnh giúp các trang con (như `MembersPage`) có thể gọi `MainWindow.Instance?.ShowPage(...)` để chuyển màn hình dễ dàng.
- **Dòng 17-26**: Quản lý bộ sưu tập nút menu `_buttons` và nhóm menu `_groupOf`, `_groups`.
- **Dòng 28-48 (Constructor)**:
  - Gán `Instance = this`.
  - Lấy thông tin người dùng từ `Session.CurrentUser` hiển thị họ tên, username, tên vai trò tiếng Việt (`Rules.RoleNames`).
  - Gọi `BuildMenu(user.Role)` dựng menu động.
  - Nạp trang đầu tiên mặc định vào vùng hiển thị.
- **Dòng 53-83 (`BuildMenu`)**: Dựng menu theo 4 vai trò:
  - `Manager`: Báo cáo, Lớp học, Bộ môn phòng, Member, Gói thành viên, Thanh toán, Tài khoản, Lịch sử hoạt động.
  - `Receptionist`: Member, Mua/gia hạn gói, Đặt lớp, Thanh toán.
  - `Member`: Lịch của tôi, Đặt lớp, Gói thành viên, Thanh toán.
  - `Coach`: Lớp của tôi, Điểm danh.
- **Dòng 88-111 (`MakeButton`)**: Tạo nút menu với style `MenuButton`, bắt sự kiện Click khởi tạo trang bằng `Activator.CreateInstance(pageType)` và hiển thị lên `contentHost`.
- **Dòng 125-163 (`AddGroup`)**: Tạo nhóm xổ xuống bằng `Expander`, hỗ trợ đóng các nhóm khác khi một nhóm được mở (Accordion).
- **Dòng 170-188 (`ShowPage`)**: Đưa UserControl vào `contentHost.Content`, đồng thời đánh dấu `Tag = "on"` để tô sáng nút menu đang chọn.
- **Dòng 193-199 (`btnChangePassword_Click`)**: Mở hộp thoại `ChangePasswordWindow` dạng Modal Dialog (`ShowDialog()`).
- **Dòng 204-215 (`btnLogout_Click`)**: Xóa phiên làm việc `Session.Clear()`, mở lại `LoginWindow` và đóng màn hình chính.

---

## 4.5. `ChangePasswordWindow.xaml.cs` – Đổi mật khẩu tài khoản (Chuẩn PRN212)

Tập tin nằm tại `SportsCenterApp/SportsCenter/ChangePasswordWindow.xaml.cs`.

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

**Giải thích chi tiết từng bước & từng dòng**:
- **Dòng 14**: Khởi tạo DbContext.
- **Dòng 16-25 (Constructor)**: Nạp thông tin người dùng từ `Session.CurrentUser` hiển thị lên thanh `txtInfo`. Đã loại bỏ dòng lambda focus thừa thãi.
- **Dòng 30-120 (`btnChange_Click`)**: Kiểm tra mật khẩu cũ khớp mã băm, kiểm tra mật khẩu mới và xác nhận mật khẩu, cập nhật mã băm SHA-256 mới, lưu DB và ghi nhật ký kiểm toán. Khối catch rút gọn ngắn gọn.

---

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

## 5.2. `UsersPage.xaml.cs` – Quản lý tài khoản, Khóa & Đổi vai trò (Chuẩn Q2 PRN212)

Tập tin nằm tại `SportsCenterApp/SportsCenter/Pages/UsersPage.xaml.cs`.

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
 43:                 query = query.Where(x => x.Username.Contains(keyword) || x.FullName.Contains(keyword));
 44:             }
 45: 
 46:             // 3. Lọc theo Role nếu có chọn (khác All / Tất cả):
 47:             if (roleIndex > 0)
 48:             {
 49:                 string selectedRole = _roles[roleIndex - 1];
 50:                 query = query.Where(x => x.Role == selectedRole);
 51:             }
 52: 
 53:             // 4. Đổ dữ liệu vào DataGrid:
 54:             var rows = query
 55:                 .OrderBy(x => x.UserId)
 56:                 .Select(x => new
 57:                 {
 58:                     x.UserId,
 59:                     x.Username,
 60:                     x.FullName,
 61:                     Role = RoleName(x.Role),
 62:                     Status = x.IsActive ? "Đang hoạt động" : "Đã khóa"
 63:                 })
 64:                 .ToList();
 65: 
 66:             dgUsers.ItemsSource = pgUsers.Slice(rows);
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

**Giải thích chi tiết từng bước & từng hàm (Chuẩn câu 2 đề thi PRN212)**:
- **Dòng 15**: Khai báo DbContext kết nối CSDL.
- **Dòng 16**: Mảng 4 vai trò trong DB.
- **Dòng 20-29 (Constructor)**: Khởi tạo DB, thiết lập DataGrid qua `GridHelper.Setup`, đăng ký phân trang `pgUsers.PageChanged += (s, e) => LoadUsers();`, gọi `LoadRole()` và `LoadUsers()`.
- **Dòng 31-70 (`LoadUsers`)**: Truy vấn LINQ 4 bước rõ ràng: đọc keyword/roleIndex $ightarrow$ lọc keyword $ightarrow$ lọc Role (khác All) $ightarrow$ chiếu `.Select(...)` nạp DataGrid.
- **Dòng 72-96 (`LoadRole` - Dùng vòng lặp foreach)**: Nạp 3 ComboBox bằng 1 vòng lặp `foreach`, thêm "All" ở đầu bộ lọc, gán mặc định `cbxRoleFilter.SelectedIndex = 0`, `cbxRole.SelectedIndex = 1`, `cbxNewRole.SelectedIndex = 1`.
- **Dòng 98-102 (`btnFilter_Click`)**: Reset trang và gọi `LoadUsers()`.
- **Dòng 104-111 (`btnClearFilter_Click`)**: Xóa trắng ô tìm kiếm, đưa ComboBox lọc về "All", reset trang và tải lại toàn bộ danh sách.
- **Dòng 113-173 (`btnAddUser_Click`)**: Thêm tài khoản mới: Validate dữ liệu, tạo `User` mới mật khẩu mặc định "123456" (băm SHA-256), lưu DB, ghi log kiểm toán, reset DataGrid bằng `LoadUsers()`, reset form bằng hàm riêng `ClearForm()`, hiển thị thông báo thành công. Khối catch rút gọn.
- **Dòng 175-198 (`btnToggle_Click`)**: Đảo ngược `user.IsActive`, lưu DB, reset DataGrid và thông báo thành công. Khối catch rút gọn.
- **Dòng 200-234 (`btnChangeRole_Click`)**: Cập nhật vai trò mới, gọi `EnsureProfile`, lưu DB, reset DataGrid và thông báo thành công. Khối catch rút gọn.
- **Dòng 236-252 (`GetSelectedUser`)**: Lấy tài khoản đang chọn từ DataGrid, áp dụng Luật A3 không cho Quản lý tự khóa hoặc đổi quyền chính mình.
- **Dòng 254-267 (`EnsureProfile`)**: Tự động thêm bản ghi vào `Coaches` hoặc `Members` nếu chưa có.
- **Dòng 269-275 (`ClearForm`)**: Hàm riêng biệt xóa trắng các ô nhập liệu và reset ComboBox về mặc định (chuẩn PRN212).

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

