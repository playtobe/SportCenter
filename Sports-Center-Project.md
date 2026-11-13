# Hệ thống Quản lý Trung tâm Thể thao (Sports Center Management System)

Tài liệu tóm tắt phạm vi, thiết kế và quy ước cho **Group Project PRN212** (WPF + EF Core, .NET 8, 5 thành viên).

| File trong folder | Nội dung |
|---|---|
| `SportsCenter.sql` | Script tạo database `SportsCenter` + dữ liệu mẫu (chạy lại nhiều lần được) |
| `SportsCenterApp/` | **Code WPF đã dựng sẵn** (mở `SportsCenterApp.sln` bằng Visual Studio 2022), xem mục 7 |
| `Prototype.html` | Prototype bấm được của các màn hình (mở bằng trình duyệt) |
| `Sports-Center-Project.md` | Tài liệu này |

> Prototype chỉ để xem bố cục và luật nghiệp vụ, không phải code WPF. Dữ liệu trong prototype là dữ liệu mô phỏng, nhiều hơn bản trong `SportsCenter.sql`.

---

## 1. Phạm vi

Đề gốc có 6 flow. Nhóm chọn làm **một chuỗi nghiệp vụ chạy từ đầu đến cuối**:

> Đăng ký tài khoản → Mua gói thành viên → Đăng ký lớp học → Thanh toán → Báo cáo

| Flow gốc | Quyết định |
|---|---|
| 1. Quản lý user và thành viên (bắt buộc) | **Làm** |
| 2. Đặt lớp và quản lý lịch (bắt buộc) | **Làm** |
| 3. Thanh toán và báo cáo (bắt buộc) | **Làm** (thanh toán mô phỏng) |
| 4. Huấn luyện và điểm danh (tùy chọn) | Làm nhẹ, mức P2 |
| 5. AI gợi ý bài tập, 6. Trợ lý AI | **Không làm** (cần API ngoài) |

Các quyết định đã chốt trong quá trình thiết kế:

- Thanh toán **chỉ ghi nhận** vào DB, không nối cổng thanh toán thật.
- Form Đăng ký **không có email** (hệ thống không gửi mail). Thay bằng số điện thoại.
- Cài đặt tài khoản **chỉ cho đổi mật khẩu**. Họ tên và username không sửa được (nhờ Manager hoặc lễ tân).
- **Không có mục tiêu tập luyện** (đã bỏ khỏi DB và giao diện).
- Điểm danh chỉ có **Có mặt** và **Vắng** (đã bỏ "Trễ").
- Lịch học chia **ca cố định** thay vì giờ tự do.
- Mật khẩu **băm SHA-256**.
- Gắn tài khoản mới vào hồ sơ đăng ký tại quầy theo **số điện thoại + họ tên** (lựa chọn đã cân nhắc, kèm giới hạn ở luật A2).

---

## 2. Vai trò và menu

| Role | Việc chính |
|---|---|
| **Center Manager** | Quản lý lớp, bộ môn, phòng, gói, tài khoản; xem báo cáo và lịch sử hoạt động |
| **Receptionist** (lễ tân) | Làm việc **thay cho member** tại quầy: đăng ký member, bán gói, đăng ký lớp, thu tiền, điểm danh |
| **Member** | Tự mua gói, đăng ký lớp, xem lịch và hóa đơn của mình |
| **Coach** | Xem lớp mình dạy và học viên, điểm danh |

Menu theo role (nhóm bấm để xổ xuống, role ít mục thì để phẳng):

```
Manager
  Báo cáo
  ▸ Quản lý lớp            Lớp học · Bộ môn, phòng
  ▸ Thành viên và gói      Member · Gói thành viên · Mua / gia hạn gói
  ▸ Giao dịch              Đặt lớp · Thanh toán
  ▸ Hệ thống               Tài khoản · Lịch sử hoạt động
  Cài đặt

Receptionist
  ▸ Thành viên             Member · Mua / gia hạn gói
  ▸ Lớp học                Đặt lớp · Điểm danh
  Thanh toán
  Cài đặt

Member                     Lịch của tôi · Đặt lớp · Gói thành viên · Thanh toán · Cài đặt
Coach                      Lớp của tôi · Điểm danh · Cài đặt
```

Receptionist và Member **dùng chung** các màn Mua/gia hạn gói, Đặt lớp, Thanh toán. Lễ tân chỉ có thêm bước chọn "đang thao tác cho member nào".

---

## 3. Danh sách 17 màn hình

| # | Màn hình | Ai dùng | Mức |
|---|---|---|---|
| 1 | Login | Tất cả | P0 |
| 2 | Register | Member mới | P0 |
| 3 | MainWindow (khung và menu theo role) | Tất cả | P0 |
| 4 | Cài đặt tài khoản (đổi mật khẩu) | Tất cả | P1 |
| 5 | Tài khoản và role | Manager | P1 |
| 6 | Gói thành viên | Manager | P0 |
| 7 | Bộ môn và phòng tập | Manager | P0 |
| 8 | Quản lý lớp học | Manager | P0 |
| 9 | Báo cáo và doanh thu | Manager | P0 |
| 10 | Lịch sử hoạt động | Manager | P2 |
| 11 | Quản lý member | Manager, Receptionist | P0 |
| 12 | Mua hoặc gia hạn gói | Receptionist, Member | P0 |
| 13 | Đặt lớp | Receptionist, Member | P0 |
| 14 | Lịch cá nhân kiểu FAP (tuần, kèm kết quả điểm danh trong từng ô) | Member | P1 |
| 15 | Thanh toán và hóa đơn | Receptionist, Member | P0 |
| 16 | Lớp và học viên của tôi | Coach | P2 |
| 17 | Điểm danh | Receptionist, Coach | P2 |

Nếu cần cắt cho kịp thời gian: bỏ P2 (10, 16, 17) và P1 (4, 14) còn **12 màn**, các luồng bắt buộc vẫn chạy đủ.

### Quy ước giao diện chung

- **Phân trang** mọi danh sách dài: 8 dòng mỗi trang, nút tròn « ‹ 1 2 3 4 5 › » , trang hiện tại tô cam, dòng "Hiển thị 1-8 trên N". Lọc hoặc tìm kiếm xong tự về trang 1.
- **Bộ lọc** (nút *Lọc* và *Xóa lọc*) ở các màn lịch sử và danh sách:

| Màn | Lọc theo |
|---|---|
| Quản lý lớp | Bộ môn, huấn luyện viên (kiểu Q2: có mục "Tất cả") |
| Thanh toán | Mã hóa đơn hoặc tên member, trạng thái (Đã thu, Đã hoàn), từ ngày, đến ngày |
| Lịch sử hoạt động | Từ khóa (chi tiết, mã), người dùng, hành động, từ ngày, đến ngày |
| Tài khoản | Tên đăng nhập hoặc họ tên, vai trò |
| Đặt lớp | Mã hoặc tên lớp, bộ môn |
| Member | Tên, số điện thoại hoặc email; trạng thái gói (Còn hạn, Hết hạn, Chưa có gói) |

- **Điểm danh** nhiều trang vẫn giữ lựa chọn Có mặt/Vắng khi chuyển trang, bấm Lưu thì lưu đủ tất cả các trang.
- Báo lỗi nhập liệu **ngay dưới từng ô** (chữ đỏ), không dùng hộp thông báo chung.
- Màn Coach: chọn **một lớp** rồi chỉ hiện học viên lớp đó (không xếp chồng các lớp).

---

## 4. Luật nghiệp vụ

Mã luật dùng để trao đổi trong nhóm, **không hiển thị cho người dùng**.

### Tài khoản
- **A1 Login**: username tồn tại, `IsActive = 1`, hash mật khẩu khớp. Đúng thì vào menu theo `Role`.
- **A2 Register**: chỉ tạo role Member. Họ tên, số điện thoại (10-11 số, bắt đầu bằng 0), username không trùng, mật khẩu, nhập lại mật khẩu. Mật khẩu tối thiểu 6 ký tự, hai ô phải khớp, không trùng username. Tạo cùng lúc một dòng `Users` và một dòng `Members`. **Gắn vào hồ sơ tại quầy**: nếu số điện thoại **và** họ tên (không phân biệt hoa thường và dấu) khớp **đúng một** hồ sơ đăng ký tại quầy chưa có tài khoản, hệ thống hỏi xác nhận rồi gắn tài khoản vào hồ sơ đó thay vì tạo hồ sơ mới, nên khách thấy lại gói, hóa đơn, lớp cũ. Có nhiều hồ sơ trùng thì không tự gắn. Mỗi lần gắn ghi `LinkMember` vào lịch sử hoạt động. **Giới hạn đã cân nhắc**: ai biết cả họ tên lẫn số điện thoại của khách vẫn gắn được; muốn chặt hơn phải dùng mã liên kết hoặc để lễ tân gắn tay.
- **A3**: Manager không được tự khóa hoặc tự đổi role của chính mình. Tạo user role Coach thì tạo kèm dòng `Coaches`, role Member thì tạo kèm `Members`.
- **A4**: ghi `AuditLogs` cho Login, tạo lớp, đăng ký lớp, hủy lớp, thanh toán, hoàn tiền, đổi mật khẩu…
- **Đổi mật khẩu**: phải nhập đúng mật khẩu hiện tại; mật khẩu mới ≥ 6 ký tự, khác mật khẩu cũ, không trùng username, hai ô khớp.

### Gói thành viên
- **P1**: `Price >= 0`, `DurationDays > 0`. Gói đã có người mua thì không xóa, chỉ ngừng bán (`IsActive = 0`).
- **P2 Mua gói**: tạo `Subscription` (`StartDate` = hôm nay, `EndDate` = `StartDate + DurationDays - 1`) và `Payment`.
- **P3 Gia hạn**: nếu còn gói chưa hết hạn thì gói mới bắt đầu từ `EndDate + 1` (cộng dồn), không thì từ hôm nay.
- **P4**: gói còn hạn là `Status = Active` và `StartDate <= hôm nay <= EndDate`. Tính theo ngày, không chỉ dựa cột `Status`.
- **P5**: cảnh báo gói sắp hết hạn trong 7 ngày.

### Lớp học (Manager)
Lịch học cố định hằng tuần: mỗi lớp có `DayOfWeek` (1 = Thứ Hai … 7 = Chủ Nhật) và **một ca**.

| Ca | Giờ |
|---|---|
| Ca 1 | 06:00 – 07:30 |
| Ca 2 | 07:30 – 09:00 |
| Ca 3 | 09:00 – 10:30 |
| Ca 4 | 16:30 – 18:00 |
| Ca 5 | 18:00 – 19:30 |
| Ca 6 | 19:30 – 21:00 |

- **C1**: `ClassCode` không trùng, `Capacity > 0` và không vượt sức chứa phòng, `EndDate >= StartDate`.
- **C2 Trùng phòng**: cùng `RoomId`, cùng `DayOfWeek`, cùng `SlotId` và lớp kia chưa hủy. Bản code không xét khoảng ngày (chặn chặt hơn cho đơn giản).
- **C3 Trùng HLV**: giống C2 nhưng theo `CoachId`.
- **C4**: lớp đã có đăng ký thì không xóa, chỉ đổi `Status = Cancelled`. Phòng, bộ môn đang có lớp thì không xóa.
- **Form thêm lớp chỉ hiện Thứ và Ca còn trống** của HLV và phòng đã chọn. Đổi HLV hoặc phòng thì danh sách tự cập nhật. Nếu HLV và phòng đó kín cả tuần thì khóa nút thêm và báo chọn cái khác. C2, C3 vẫn kiểm tra lại ở BLL để chặn thêm một lớp nữa.

### Đặt lớp
- **B1**: member phải có gói còn hạn.
- **B2**: lớp `Status = Open` và chưa kết thúc.
- **B3 Sĩ số**: số đăng ký `Registered` của lớp phải nhỏ hơn `Capacity`.
- **B4**: một member chỉ có một đăng ký `Registered` cho mỗi lớp (có unique index ở DB).
- **B5**: member không được đăng ký hai lớp cùng thứ và cùng ca.
- **B6 Hủy lớp**: `Status = Cancelled`, ghi `CancelledAt`. Chỉ hủy được khi buổi học gần nhất còn cách hơn **2 giờ**.
- **B7**: lễ tân đăng ký hộ phải qua đủ B1 đến B5, lưu `RegisteredBy` là lễ tân.
- **B8**: Member chỉ thao tác trên dữ liệu của chính mình.

### Thanh toán (mô phỏng)
- **Y1**: `Amount > 0`, mặc định bằng giá gói. Phương thức: `Cash`, `BankTransfer`, `Card`. `InvoiceNo` tự tăng dạng `INV-0001`.
- **Y2**: tạo `Subscription` và `Payment` trong **một transaction**.
- **Y3 Hoàn tiền** (chỉ Manager): `Payment.Status = Refunded`, `Subscription.Status = Cancelled`, không tính vào doanh thu.
- **Y4**: xuất hóa đơn ra file `.txt` hoặc `.csv` (hoặc in bằng `PrintDialog`).

### Báo cáo (Manager)
- **R1**: tổng member, member mới trong khoảng ngày, số gói đang hoạt động.
- **R2**: số đăng ký trên sĩ số của từng lớp (tỷ lệ lấp đầy).
- **R3 Doanh thu**: theo khoảng ngày, biểu đồ 6 tháng gần nhất. Chỉ tính `Payment.Status = 'Paid'`.
- **R4**: danh sách gói sắp hết hạn.

### Điểm danh (P2)
Chỉ điểm danh được member đang `Registered` ở lớp đó, đúng thứ trong tuần của lớp. Mỗi đăng ký chỉ có một dòng cho mỗi ngày. Trạng thái: `Present`, `Absent`. **Member xem kết quả ngay trong màn Lịch của tôi**, kiểu thời khóa biểu FAP: lịch theo tuần (tiêu đề cột có ngày, cột hôm nay tô sáng, nút Tuần trước / Tuần này / Tuần sau và ô chọn ngày), mỗi ô lớp có huy hiệu **Có mặt** (xanh) hoặc **Vắng** (đỏ) của đúng ngày đó, kèm giờ học, phòng, huấn luyện viên. Buổi đã qua mà chưa điểm danh ghi "Chưa điểm danh"; buổi hôm nay và sau này chưa hiện gì. Dòng tóm tắt ở trên cho biết số buổi có mặt, vắng, chưa điểm danh trong tuần. Dữ liệu điểm danh mẫu trong SQL được đặt đúng thứ của lớp để hiện đúng ô.

### Phân quyền

| Chức năng | Manager | Receptionist | Member | Coach |
|---|:-:|:-:|:-:|:-:|
| CRUD user, gán role | ✔ | | | |
| CRUD gói, bộ môn, phòng, lớp | ✔ | | | |
| Đăng ký member tại quầy, tìm member | ✔ | ✔ | | |
| Bán hoặc gia hạn gói | ✔ | ✔ | chính mình | |
| Đăng ký hoặc hủy lớp | ✔ | cho member bất kỳ | chính mình | |
| Ghi nhận thanh toán, xuất hóa đơn | ✔ | ✔ | xem của mình | |
| Hoàn tiền | ✔ | | | |
| Báo cáo, lịch sử hoạt động | ✔ | | | |
| Xem lớp mình dạy, điểm danh | ✔ | ✔ (điểm danh) | | lớp mình dạy |

---

## 5. Cơ sở dữ liệu

Database `SportsCenter`, **13 bảng**.

| Nhóm | Bảng |
|---|---|
| Tài khoản | `Users` (có `Role`), `Members`, `Coaches` |
| Gói | `MembershipPackages`, `Subscriptions` |
| Lớp | `Sports`, `Rooms`, `TimeSlots`, `Classes`, `Registrations` |
| Thanh toán | `Payments` |
| Phần thêm | `Attendances`, `AuditLogs` |

Ghi chú thiết kế:

- `Members.UserId` cho phép **NULL**: khách đăng ký tại quầy chưa cần tài khoản.
- `Users.Email` cho phép NULL (không bắt buộc).
- `Classes` chứa `DayOfWeek` và `SlotId`, không còn cột giờ bắt đầu hoặc kết thúc.
- Luật trùng phòng, trùng HLV, sĩ số, gói còn hạn để **BLL** xử lý. DB chỉ chặn đăng ký trùng bằng filtered unique index trên `Registrations(MemberId, ClassId) WHERE Status = 'Registered'`.
- Tên bảng số nhiều, entity số ít (`Users` → `User`), giống khuôn scaffold của Q2.

### Chạy script

Script `SportsCenter.sql` **chạy lại nhiều lần được**: tự chuyển về `master`, xóa database cũ (ngắt các kết nối đang giữ), tạo lại, và còn xóa từng bảng theo thứ tự con trước cha sau như một lớp dự phòng. Chạy trong SSMS cửa sổ nào cũng được, kể cả cửa sổ đang đứng trong `SportsCenter`.

### Tài khoản mẫu (mật khẩu chung `123456`)

| Role | Username |
|---|---|
| Manager | `manager1` |
| Receptionist | `reception1` |
| Coach | `coach1`, `coach2`, `coach3` |
| Member | `member1` … `member5` |

Mật khẩu lưu dạng **SHA-256, chuỗi hex IN HOA**. Trong C#:

```csharp
using System.Security.Cryptography;
using System.Text;

string Hash(string password) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
```

Dữ liệu mẫu có sẵn các tình huống để demo: lớp `GYM01` chỉ 3 chỗ và đã đầy, `member4` có gói hết hạn, một khách vãng lai chưa có tài khoản và chưa mua gói, một đăng ký đã hủy, 5 hóa đơn rải trong vài tháng.

---

## 6. Kiến trúc và quy ước code

Viết theo **khuôn Q2** của các assignment: một project WPF, code-behind (không MVVM, không chia nhiều project), mỗi màn hình tự giữ `_db = new()`.

Quy ước kết nối DB, **giống Q2** của các assignment:

| Thành phần | Quy ước |
|---|---|
| File cấu hình | `appsettings.json`, thuộc tính *Copy to Output Directory = Always* |
| Key | `ConnectionStrings:DefaultConnection` |
| Tên DbContext | `SportsCenterContext` (tên DB viết PascalCase + `Context`) |
| Entity | Số ít, scaffold từ DB |
| Package | `Microsoft.EntityFrameworkCore.SqlServer` và `.Design` bản `8.0.18`, `Microsoft.Extensions.Configuration.Json` bản `9.0.7` |

Đọc connection string trong `OnConfiguring`:

```csharp
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

string? strConn = config["ConnectionStrings:DefaultConnection"];
optionsBuilder.UseSqlServer(strConn);
```

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=(local);uid=sa;pwd=12345;database=SportsCenter;TrustServerCertificate=True;"
  }
}
```

Scaffold entity (Package Manager Console):

```
Scaffold-DbContext "server=(local);uid=sa;pwd=12345;database=SportsCenter;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Entities -Context SportsCenterContext
```

Lưu ý khi làm nhóm:

1. **`(local)` chỉ trỏ tới SQL Server cài dạng default instance.** Máy nào là named instance (ví dụ `ANHBA`) thì sửa cục bộ thành `(local)\ANHBA`. Máy chỉ dùng Windows Authentication thì thay `uid=sa;pwd=...` bằng `Trusted_Connection=True;`.
2. **Đừng commit thay đổi của `appsettings.json`** (mỗi người một cấu hình riêng), hoặc commit file `appsettings.example.json` làm mẫu.
3. Chỉ có một project WPF nên `appsettings.json` nằm cùng project và được copy sang thư mục chạy (*Copy to Output Directory = Always*).
4. Mỗi màn hình giữ một `_db` riêng như Q2. Các truy vấn danh sách dùng `Select(new {...})` hoặc `AsNoTracking()` nên luôn đọc dữ liệu mới, không bị cũ khi màn hình khác vừa sửa.

---

## 7. Mã nguồn WPF đã dựng sẵn

Folder `SportsCenterApp` (.NET 8, WPF, EF Core 8.0.18). Code theo đúng khuôn trong `Template Q2.txt`: các hàm `LoadX`, `Select(new {...}).ToList()` đổ vào DataGrid, ComboBox có mục "Tất cả", kiểm tra bằng `MessageBox`, `try/catch` lấy `InnerException`.

```
SportsCenter/
  Entities/     13 entity + SportsCenterContext (đọc connection string từ appsettings.json)
  Helpers/      Session (người đang đăng nhập), PasswordHelper (SHA-256), AuditHelper (ghi lịch sử),
                Rules (luật nghiệp vụ dùng chung), GridHelper (cấu hình DataGrid), Msg (MessageBox)
  Pages/        14 màn hình, mỗi màn là một cặp .xaml + .xaml.cs (UserControl)
  LoginWindow, RegisterWindow, MainWindow (menu theo role, nhóm bấm để xổ xuống)
  PagerControl  thanh phân trang dùng chung « ‹ 1 2 3 › »
```

Luật dùng chung giữa nhiều màn (đăng ký lớp, hủy lớp, ca trống, gói còn hạn, mã hóa đơn) nằm trong `Helpers/Rules.cs`. Màn nào cần thì gọi, các thành viên không phải sửa code của nhau.

**Cách chạy**

1. Chạy `SportsCenter.sql` trong SSMS (tạo database và dữ liệu mẫu).
2. Mở `SportsCenterApp\SportsCenterApp.sln` bằng Visual Studio 2022, kiểm tra `SportsCenter\appsettings.json` (xem mục 6).
3. Chạy (F5) và đăng nhập bằng một tài khoản mẫu ở mục 5.

**Đã kiểm tra** (trên database tạm LocalDB, bằng bộ test tự động mô phỏng thao tác, khoảng 110 kiểm tra): đăng nhập và đăng ký, mở được mọi màn của cả 4 role, tạo lớp với thứ và ca còn trống, hủy lớp, mua gói và cộng dồn, các luật đặt lớp (chưa có gói, đã đăng ký, lớp đầy, trùng giờ, hủy trước 2 giờ), hoàn tiền, điểm danh, đổi mật khẩu, phân trang, bộ lọc.

**Chưa kiểm tra**: chạy trên SQL Server của từng máy trong nhóm (chuỗi kết nối `(local)`), thao tác chuột bàn phím thật, và thiết kế trong Visual Studio Designer. Nhóm nên chạy thử và báo lại chỗ nào lỗi.

---

## 8. Chia việc cho 5 người

| Module | Màn hình |
|---|---|
| 1. Nền tảng và tài khoản | 1, 2, 3, 4, 5 |
| 2. Gói thành viên | 6, 11, 12 |
| 3. Quản lý lớp | 7, 8 (+16 nếu dư thời gian) |
| 4. Đặt lớp | 13, 14 (+17 nếu dư thời gian) |
| 5. Thanh toán và báo cáo | 15, 9 (+10 nếu dư thời gian) |

Nên làm ngay đầu dự án:

1. **Chốt schema và entity trước**, giao một người (thường là người làm module 1) làm trong vài ngày đầu. Các module khác đều phụ thuộc vào đây.
2. Để `SportsCenter.sql` trong repo, mỗi người chạy cùng một script trên máy của mình.
3. Dùng Git. Mỗi người làm các Window hoặc Page riêng, chỉ một `MainWindow` chung chứa menu theo role do một người giữ để tránh xung đột XAML.
4. Mỗi người làm các Page của module mình. Luật dùng chung nằm trong `Helpers/Rules.cs`, thêm hàm mới ở đó thay vì chép code giữa các màn.

---

## 9. Còn cần xác nhận

1. **Rubric của PRN212**: có yêu cầu MVVM hoặc 3-layer cụ thể không (code hiện tại theo khuôn Q2, một project), số màn hình tối thiểu, có chấm đóng góp từng cá nhân không.
2. **NuGet**: luật "không thêm NuGet" là của bài thi, project nhóm có thể khác. Nếu được thêm thì có thể dùng BCrypt để băm mật khẩu và thư viện xuất PDF.
3. Màn Cài đặt (đổi mật khẩu) giữ là màn riêng hay chuyển thành hộp thoại nhỏ (khi đó còn 16 màn).
4. Luật B6 (hủy lớp trước 2 giờ) và P3 (gia hạn cộng dồn) đã đặt mặc định, có thể đổi nếu giảng viên yêu cầu khác.
