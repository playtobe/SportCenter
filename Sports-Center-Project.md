# Hệ thống Quản lý Trung tâm Thể thao (Sports Center Management System)

Tài liệu tóm tắt phạm vi, thiết kế và quy ước cho **Group Project PRN212** (WPF + EF Core, .NET 8, 5 thành viên).

| File trong folder | Nội dung |
|---|---|
| `SportsCenter.sql` | Script tạo database `SportsCenter` + dữ liệu mẫu (chạy lại nhiều lần được) |
| `SportsCenterApp/` | **Code WPF đã dựng sẵn** (mở `SportsCenterApp.sln` bằng Visual Studio 2022), xem mục 7 |
| `Prototype.html` | Prototype bấm được của các màn hình (mở bằng trình duyệt) |
| `Ban-do-doc-code.md` | **Bản đồ đọc code** cho từng người (đọc file nào trước, luồng chính, câu hay bị hỏi, bảng tra mã luật), xem thêm mục 8 |
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

- Điểm danh buổi học **chỉ do coach làm**; lễ tân phụ trách quầy (đăng ký member, bán gói, đặt lớp). Dòng "điểm danh thành viên khi đến trung tâm" của lễ tân trong đề hiện chưa có chức năng riêng, xem mục 9.
- Thanh toán **chỉ ghi nhận** vào DB, không nối cổng thanh toán thật. **Không có hoàn tiền** (đã bỏ); Manager chỉ xem danh sách thanh toán và doanh thu.
- Form Đăng ký **không có email** (hệ thống không gửi mail). Thay bằng số điện thoại.
- Đổi mật khẩu là một **hộp thoại nhỏ** mở từ nút ở chân menu, không có màn Cài đặt riêng. Chỉ đổi được mật khẩu; họ tên và username không sửa được (nhờ Manager hoặc lễ tân).
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
| **Receptionist** (lễ tân) | Làm việc **thay cho member** tại quầy: đăng ký member, bán gói, đăng ký lớp, thu tiền (không điểm danh buổi học) |
| **Member** | Tự mua gói, đăng ký lớp, xem lịch và hóa đơn của mình |
| **Coach** | Xem lớp mình dạy và học viên, **điểm danh buổi học** (chỉ coach làm việc này) |

Menu theo role (nhóm bấm để xổ xuống, role ít mục thì để phẳng):

```
Manager
  Báo cáo
  ▸ Quản lý lớp            Lớp học · Bộ môn, phòng
  ▸ Thành viên và gói      Member · Gói thành viên
  Thanh toán
  ▸ Hệ thống               Tài khoản · Lịch sử hoạt động

Receptionist
  ▸ Thành viên             Member · Mua / gia hạn gói
  Đặt lớp
  Thanh toán

Member                     Lịch của tôi · Đặt lớp · Gói thành viên · Thanh toán
Coach                      Lớp của tôi · Điểm danh
```

Mọi role đều có thêm hai nút ở chân menu: **Đổi mật khẩu** (mở hộp thoại nhỏ) và **Đăng xuất**.

Receptionist và Member **dùng chung** các màn Mua/gia hạn gói, Đặt lớp, Thanh toán. Lễ tân chỉ có thêm bước chọn "đang thao tác cho member nào". Việc bán gói là của lễ tân (tại quầy) và member (tự mua); Manager chỉ quản lý danh mục gói, không bán. Đặt lớp cũng vậy: Manager chỉ xem tình trạng đăng ký ở Báo cáo và Quản lý lớp, không đặt lớp cho ai.

---

## 3. Danh sách 16 màn hình và 1 hộp thoại

| # | Màn hình | Ai dùng | Mức |
|---|---|---|---|
| 1 | Login | Tất cả | P0 |
| 2 | Register | Member mới | P0 |
| 3 | MainWindow (khung và menu theo role) | Tất cả | P0 |
| 4 | Hộp thoại đổi mật khẩu (nút ở chân menu) | Tất cả | P1 |
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
| 17 | Điểm danh buổi học | Coach | P2 |

Nếu cần cắt cho kịp thời gian: bỏ P2 (10, 16, 17) và P1 (4, 14) còn **12 màn**, các luồng bắt buộc vẫn chạy đủ.

### Quy ước giao diện chung

- **Phân trang** mọi danh sách dài: 8 dòng mỗi trang, nút tròn « ‹ 1 2 3 4 5 › » , trang hiện tại tô cam, dòng "Hiển thị 1-8 trên N". Lọc hoặc tìm kiếm xong tự về trang 1.
- **Bộ lọc** (nút *Lọc* và *Xóa lọc*) ở các màn lịch sử và danh sách:

| Màn | Lọc theo |
|---|---|
| Quản lý lớp | Bộ môn, huấn luyện viên (kiểu Q2: có mục "Tất cả") |
| Thanh toán | Mã hóa đơn hoặc tên member, từ ngày, đến ngày |
| Lịch sử hoạt động | Từ khóa (chi tiết, mã), người dùng, hành động, từ ngày, đến ngày |
| Tài khoản | Tên đăng nhập hoặc họ tên, vai trò |
| Đặt lớp | Mã hoặc tên lớp, bộ môn |
| Member | Tên, số điện thoại hoặc email; trạng thái gói (Còn hạn, Hết hạn, Chưa có gói) |

- **Điểm danh** thao tác nhanh cho coach: chọn lớp rồi chọn **buổi học** trong danh sách (không phải tự chọn ngày, mặc định chọn sẵn buổi gần nhất). Mỗi học viên là một dòng lớn, **bấm vào cả dòng** để đổi Có mặt / Vắng (dòng xanh = có mặt, dòng đỏ = vắng). Có nút "Có mặt tất cả" / "Vắng tất cả" và dòng tổng kết "Có mặt 2/3 · Vắng 1". Nhiều trang vẫn giữ lựa chọn khi chuyển trang, bấm Lưu thì lưu đủ tất cả các trang.
- Báo lỗi nhập liệu **ngay dưới từng ô** (chữ đỏ), không dùng hộp thông báo chung.
- Màn Coach: chọn **một lớp** rồi chỉ hiện học viên lớp đó (không xếp chồng các lớp).

---

## 4. Luật nghiệp vụ

Mã luật dùng để trao đổi trong nhóm, **không hiển thị cho người dùng**.

### Tài khoản
- **A1 Login**: username tồn tại, `IsActive = 1`, hash mật khẩu khớp. Đúng thì vào menu theo `Role`.
- **A2 Register**: chỉ tạo role Member. Gồm họ tên, số điện thoại, tên đăng nhập, mật khẩu, nhập lại mật khẩu. **Họ tên**: 2-100 ký tự, chỉ chữ cái (có dấu) và khoảng trắng, không có số hay ký tự lạ. **Số điện thoại**: 10-11 số, bắt đầu bằng 0 và **không trùng với member nào** (A5). **Tên đăng nhập**: 3-30 ký tự, chỉ chữ không dấu, số, `.` hoặc `_`, không khoảng trắng, không trùng (không phân biệt hoa thường). Mật khẩu tối thiểu 6 ký tự, hai ô phải khớp, không trùng username. Tạo cùng lúc một dòng `Users` và một dòng `Members`. **Gắn vào hồ sơ tại quầy**: nếu số điện thoại đã thuộc về một hồ sơ đăng ký tại quầy (chưa có tài khoản) **và** họ tên khớp (không phân biệt hoa thường và dấu), hệ thống hỏi xác nhận rồi gắn tài khoản vào hồ sơ đó thay vì tạo hồ sơ mới, nên khách thấy lại gói, hóa đơn, lớp cũ. Chọn không gắn thì không tạo tài khoản (vì không được có hai hồ sơ cùng số). Số đã thuộc về người khác (đã có tài khoản, hoặc hồ sơ quầy khác họ tên) thì báo lỗi. Mỗi lần gắn ghi `LinkMember` vào lịch sử hoạt động. **Giới hạn đã cân nhắc**: ai biết cả họ tên lẫn số điện thoại của khách vẫn gắn được; muốn chặt hơn phải dùng mã liên kết hoặc để lễ tân gắn tay.
- **A3**: Manager không được tự khóa hoặc tự đổi role của chính mình. Tạo user role Coach thì tạo kèm dòng `Coaches`, role Member thì tạo kèm `Members`.
- **A4**: ghi `AuditLogs` cho Login, tạo lớp, đăng ký lớp, hủy lớp, thanh toán, đổi mật khẩu…
- **A5 Số điện thoại duy nhất**: mỗi số điện thoại chỉ thuộc về **một** member (hồ sơ đã có tài khoản hay đăng ký tại quầy đều tính). Đăng ký member tại quầy mà số đã có chủ thì chặn, nêu rõ chủ của số và lọc sẵn danh sách ra hồ sơ cũ. Database cũng có chỉ mục duy nhất `UX_Members_Phone` làm lớp chặn cuối cùng. Họ tên và tên đăng nhập theo luật định dạng ở A2 (Manager tạo tài khoản cũng áp dụng). Email nếu có nhập thì phải đúng định dạng.
- **Đổi mật khẩu**: phải nhập đúng mật khẩu hiện tại; mật khẩu mới ≥ 6 ký tự, khác mật khẩu cũ, không trùng username, hai ô khớp.

### Gói thành viên
- **P1**: `Price >= 0`, `DurationDays > 0`, tên gói không trùng (không phân biệt hoa thường và dấu). Gói đã có người mua thì không xóa, chỉ ngừng bán (`IsActive = 0`).
- **P2 Mua gói**: tạo `Subscription` (`StartDate` = hôm nay, `EndDate` = `StartDate + DurationDays - 1`) và `Payment`.
- **P3 Gia hạn (nối tiếp)**: nếu còn gói chưa hết hạn thì gói mới bắt đầu từ ngày sau khi gói **xa nhất** hết hạn (`EndDate + 1`), không thì từ hôm nay. Tính cả gói đã mua nối tiếp mà chưa tới ngày bắt đầu, nên mua nhiều lần thì các gói xếp nối đuôi, không chồng nhau. Trong lúc chờ, member vẫn dùng gói hiện tại; gói mới tự có hiệu lực khi tới ngày bắt đầu (P4).
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
- **C4**: lớp không xóa khỏi DB mà chỉ đổi `Status = Cancelled`, và **chỉ hủy được khi không còn học viên đang đăng ký** (phải hủy đăng ký của họ trước, ví dụ lễ tân hỗ trợ). Phòng, bộ môn đang có lớp thì không xóa.
- **Form thêm lớp chỉ hiện Thứ và Ca còn trống** của HLV và phòng đã chọn. Đổi HLV hoặc phòng thì danh sách tự cập nhật. Nếu HLV và phòng đó kín cả tuần thì khóa nút thêm và báo chọn cái khác. C2, C3 vẫn kiểm tra lại ở BLL để chặn thêm một lớp nữa.

### Đặt lớp
- **B1**: member phải có gói còn hạn.
- **B2**: lớp `Status = Open` và chưa kết thúc.
- **B3 Sĩ số**: số đăng ký `Registered` của lớp phải nhỏ hơn `Capacity`.
- **B4**: một member chỉ có một đăng ký `Registered` cho mỗi lớp (có unique index ở DB).
- **B5**: member không được đăng ký hai lớp cùng thứ và cùng ca.
- **B6 Hủy lớp**: `Status = Cancelled`, ghi `CancelledAt`. Chỉ hủy được khi buổi học gần nhất còn cách hơn **24 giờ**.
- **B7**: lễ tân đăng ký hộ phải qua đủ B1 đến B5, lưu `RegisteredBy` là lễ tân.
- **B8**: Member chỉ thao tác trên dữ liệu của chính mình.

### Thanh toán (mô phỏng)
- **Y1**: `Amount > 0`, mặc định bằng giá gói. Phương thức: `Cash`, `BankTransfer`, `Card`. `InvoiceNo` tự tăng dạng `INV-0001`.
- **Y2**: tạo `Subscription` và `Payment` trong **một transaction**.
- **Y3**: không có chức năng hoàn tiền (đã bỏ vì thanh toán chỉ mô phỏng). Cột `Payments.Status` vẫn còn, hiện luôn là `Paid`.
- **Y4**: xuất hóa đơn ra file `.txt` hoặc `.csv` (hoặc in bằng `PrintDialog`).

### Báo cáo (Manager)
- **R1**: tổng member, member mới trong khoảng ngày, số gói đang hoạt động.
- **R2**: số đăng ký trên sĩ số của từng lớp (tỷ lệ lấp đầy).
- **R3 Doanh thu**: theo khoảng ngày, biểu đồ 6 tháng gần nhất. Chỉ tính `Payment.Status = 'Paid'`.
- **R4**: danh sách gói sắp hết hạn.

### Điểm danh (P2)
**Chỉ coach điểm danh buổi học, và chỉ lớp mình dạy** (lễ tân và Manager không có quyền này). Chỉ điểm danh được member đang `Registered` ở lớp đó, và chỉ cho các buổi học thật của lớp: đúng thứ của lớp, trong khoảng `StartDate` đến `EndDate`, không muộn hơn hôm nay (tối đa 12 buổi gần nhất). Buổi được chọn trong danh sách nên không thể chọn sai ngày. Mỗi đăng ký chỉ có một dòng cho mỗi ngày. Trạng thái: `Present`, `Absent`. **Member xem kết quả ngay trong màn Lịch của tôi**, kiểu thời khóa biểu FAP: lịch theo tuần (tiêu đề cột có ngày, cột hôm nay tô sáng, nút Tuần trước / Tuần này / Tuần sau và ô chọn ngày), mỗi ô lớp có huy hiệu **Có mặt** (xanh) hoặc **Vắng** (đỏ) của đúng ngày đó, kèm giờ học, phòng, huấn luyện viên. Buổi đã qua mà chưa điểm danh ghi "Chưa điểm danh"; buổi hôm nay và sau này chưa hiện gì. Dòng tóm tắt ở trên cho biết số buổi có mặt, vắng, chưa điểm danh trong tuần. Dữ liệu điểm danh mẫu trong SQL được đặt đúng thứ của lớp để hiện đúng ô.

### Phân quyền

| Chức năng | Manager | Receptionist | Member | Coach |
|---|:-:|:-:|:-:|:-:|
| CRUD user, gán role | ✔ | | | |
| CRUD gói, bộ môn, phòng, lớp | ✔ | | | |
| Xem và tìm danh sách member | ✔ | ✔ | | |
| Đăng ký member tại quầy | | ✔ | | |
| Bán hoặc gia hạn gói | | ✔ | chính mình | |
| Đăng ký hoặc hủy lớp | | cho member bất kỳ | chính mình | |
| Ghi nhận thanh toán, xuất hóa đơn (khi bán gói) | | ✔ | chính mình | |
| Xem danh sách thanh toán | ✔ | ✔ | của mình | |
| Báo cáo, lịch sử hoạt động | ✔ | | | |
| Điểm danh buổi học | | | | lớp mình dạy |

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
- Luật trùng phòng, trùng HLV, sĩ số, gói còn hạn để **BLL** xử lý. DB chỉ chặn bằng filtered unique index: `Registrations(MemberId, ClassId) WHERE Status = 'Registered'` (đăng ký trùng lớp) và `Members(Phone) WHERE Phone IS NOT NULL` (số điện thoại duy nhất).
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
                Rules.cs + Rules.Packages/Classes/Booking.cs (luật nghiệp vụ dùng chung, chia file theo người),
                AccountHelper (kiểm tra tài khoản), GridHelper (cấu hình DataGrid), Msg (MessageBox)
  Pages/        13 màn hình, mỗi màn là một cặp .xaml + .xaml.cs (UserControl)
  LoginWindow, RegisterWindow, ChangePasswordWindow (hộp thoại đổi mật khẩu),
  MainWindow (menu theo role, nhóm bấm để xổ xuống, nút Đổi mật khẩu / Đăng xuất ở chân menu)
  PagerControl  thanh phân trang dùng chung « ‹ 1 2 3 › »
```

Luật dùng chung giữa nhiều màn (đăng ký lớp, hủy lớp, ca trống, gói còn hạn, mã hóa đơn) nằm trong các file `Helpers/Rules*.cs` (một `static partial class Rules`, mỗi người một file, xem mục 8). Màn nào cần thì gọi `Rules.TênHàm`, các thành viên không phải sửa code của nhau.

**Cách chạy**

1. Chạy `SportsCenter.sql` trong SSMS (tạo database và dữ liệu mẫu).
2. Mở `SportsCenterApp\SportsCenterApp.sln` bằng Visual Studio 2022, kiểm tra `SportsCenter\appsettings.json` (xem mục 6).
3. Chạy (F5) và đăng nhập bằng một tài khoản mẫu ở mục 5.

**Đã kiểm tra** (bằng bộ test tự động mô phỏng thao tác trên database tạm LocalDB, hơn 140 kiểm tra cho app và hơn 120 cho prototype, không mở cửa sổ nào): hiển thị tiếng Việt (dữ liệu mẫu, tên vai trò, tên hành động), hộp thoại đổi mật khẩu, hủy đăng ký lớp trước 24 giờ, mua gói nối tiếp nhau, menu và nút theo từng role (Manager không bán gói, không đặt lớp, không đăng ký member tại quầy), số điện thoại duy nhất, định dạng họ tên và tên đăng nhập, chống trùng tên gói, gắn tài khoản vào hồ sơ tại quầy, màn Điểm danh (chọn buổi, bấm cả dòng, lưu), chặn hủy lớp còn học viên. Các phần còn lại (đăng nhập, tạo lớp, báo cáo, phân trang, bộ lọc…) đã kiểm tra ở các đợt trước nhưng **chưa chạy lại trọn bộ** sau những lần sửa gần đây.

**Chưa kiểm tra**: chạy trên SQL Server của từng máy trong nhóm (chuỗi kết nối `(local)`), thao tác chuột bàn phím thật, và thiết kế trong Visual Studio Designer. Nhóm nên chạy thử và báo lại chỗ nào lỗi.

---

## 8. Chia việc cho 5 người

Chia theo **3 luồng bắt buộc của đề** (tài khoản và gói, lớp và đặt lớp, thanh toán và báo cáo). Mỗi người **sở hữu trọn một nhóm file** (màn hình, luật và entity của bảng mình), **mỗi file chỉ có một chủ**, nên 5 người làm song song, commit và push phần mình mà không phải sửa file của nhau. **Cách đọc từng phần (đọc file nào trước, luồng chính, câu hay bị hỏi) xem `Ban-do-doc-code.md`.**

| Người | Phần | Màn hình | Luật | Bảng dữ liệu | Dòng code (.cs / .xaml) |
|---|---|---|---|---|---|
| 1 | Tài khoản và khung ứng dụng | Login, Register, Đổi mật khẩu (hộp thoại), `MainWindow` (menu theo role), Tài khoản | A1, A2, A3, A5, đổi mật khẩu | Users | 851 / 180 |
| 2 | Member, gói thành viên, nhật ký | Member, Gói thành viên, Mua / gia hạn gói, Lịch sử hoạt động | P1-P5, A4 | Members, MembershipPackages, Subscriptions, AuditLogs | 749 / 181 |
| 3 | Quản lý lớp học | Lớp học, Bộ môn và phòng, Lớp của tôi (coach) | C1-C4 | Classes, Sports, Rooms, TimeSlots, Coaches | 750 / 176 |
| 4 | Đặt lớp, lịch cá nhân, điểm danh | Đặt lớp, Lịch của tôi, Điểm danh (coach) | B1-B8, điểm danh | Registrations, Attendances | 782 / 157 |
| 5 | Thanh toán, báo cáo, nền dùng chung | Thanh toán, Báo cáo; bộ dùng chung `App.xaml`, `GridHelper`, `PagerControl`, `Msg`; script `SportsCenter.sql` | Y1-Y4, R1-R4 | Payments | 805 / 335 và 345 dòng SQL |

Số dòng đã gồm luật của mình trong các file `Rules*.cs` và entity của bảng mình. Người 5 nhiều dòng XAML và SQL hơn nhưng đó là style và dữ liệu mẫu, ít logic nghiệp vụ; số dòng `.cs` của 5 người chỉ chênh nhau khoảng 100 (từ 749 đến 851).

**File của từng người** (mỗi cặp `.xaml` + `.xaml.cs` tính là một mục, đường dẫn trong `SportsCenterApp/SportsCenter/`):

| Người | File sở hữu |
|---|---|
| 1 | `LoginWindow`, `RegisterWindow`, `ChangePasswordWindow`, `MainWindow`, `Pages/UsersPage`; `Helpers/Session.cs`, `PasswordHelper.cs`, `AccountHelper.cs`; `Entities/User.cs` |
| 2 | `Pages/MembersPage`, `PackagesPage`, `BuyPage`, `AuditPage`; `Helpers/AuditHelper.cs`, `Rules.Packages.cs`; `Entities/Member.cs`, `MembershipPackage.cs`, `Subscription.cs`, `AuditLog.cs` |
| 3 | `Pages/ClassesPage`, `SportsRoomsPage`, `CoachPage`; `Helpers/Rules.Classes.cs`; `Entities/Class.cs`, `Coach.cs`, `Room.cs`, `Sport.cs`, `TimeSlot.cs` |
| 4 | `Pages/BookingPage`, `MySchedulePage`, `AttendancePage`; `Helpers/Rules.Booking.cs`; `Entities/Registration.cs`, `Attendance.cs` |
| 5 | `Pages/PaymentsPage`, `DashboardPage`; `App.xaml(.cs)`, `PagerControl`; `Helpers/GridHelper.cs`, `Msg.cs`, `Rules.cs` (phần lõi: tên hiển thị, hóa đơn); `Entities/Payment.cs`, `SportsCenterContext.cs`; `SportsCenter.sql` |

`Helpers/Rules.cs` được tách thành 4 file (cùng một `static partial class Rules`, gọi `Rules.TênHàm` như cũ): phần lõi của Người 5, `Rules.Packages.cs` của Người 2, `Rules.Classes.cs` của Người 3, `Rules.Booking.cs` của Người 4. Nhờ vậy mỗi người thêm hàm luật vào **file của mình**, không cùng sửa một file.

`Prototype.html` cũng chia theo người: mỗi màn là một hàm `sXxx`. Người 1: `sLogin`, `sRegister`, `sUsers`, `sPwDialog` và `MENU`; Người 2: `sMembers`, `sPackages`, `sBuy`, `sAudit`; Người 3: `sClasses`, `sSportsRooms`, `sCoach`; Người 4: `sBooking`, `sMySchedule`, `sAttendance`; Người 5: `sPayments`, `sDashboard`.

Quy tắc làm việc nhóm:

1. **Mỗi người chỉ sửa file của phần mình.** Chỉ còn 5 chỗ mà nhiều người cùng thêm một dòng, và cách xử lý: (a) mục menu của role: thêm một dòng trong `MainWindow.BuildMenu` (chủ: Người 1); (b) tên hiển thị của hành động: thêm một dòng vào **nhóm của mình** trong `Rules.ActionNames` (mỗi nhóm có chú thích "Người n"); (c) tiêu đề cột tiếng Việt: thêm một dòng cuối từ điển `GridHelper.Headers`; (d) `SportsCenter.sql`: mỗi bảng có một khối riêng (tạo bảng và dữ liệu mẫu), chỉ sửa khối bảng của mình, đổi cột thì báo người đang dùng bảng đó; (e) hai file `.md`: chỉ sửa mục của mình. Git báo xung đột ở các từ điển trên thì **giữ cả hai dòng**.
2. **Thứ tự phụ thuộc:** Người 5 chốt script và bộ dùng chung trước → Người 1 làm khung đăng nhập và menu → Người 2 (gói) và Người 3 (lớp) làm song song → Người 4 đặt lớp cần gói của Người 2 và lớp của Người 3 → báo cáo của Người 5 đọc dữ liệu của tất cả.
3. Để `SportsCenter.sql` trong repo, mỗi người chạy cùng một script trên máy của mình. Dùng Git, **mỗi người một nhánh** (`nguoi1` … `nguoi5`), chỉ `git add` đúng file của mình (không `git add -A` khi trong máy còn file của người khác), `git pull --rebase origin main` trước khi push, rồi gộp vào `main` sau khi chạy thử. Không commit `appsettings.json` của máy mình.
4. Trước khi gộp: chạy lại `SportsCenter.sql`, rồi làm đúng các thao tác ở mục **Demo** của phần mình trong `Ban-do-doc-code.md`.
5. Mỗi người chuẩn bị trình bày phần của mình: luồng chính cộng ba câu hay bị hỏi (có sẵn trong bản đồ).

---

## 9. Còn cần xác nhận

1. **Rubric của PRN212**: có yêu cầu MVVM hoặc 3-layer cụ thể không (code hiện tại theo khuôn Q2, một project), số màn hình tối thiểu, có chấm đóng góp từng cá nhân không.
2. **NuGet**: luật "không thêm NuGet" là của bài thi, project nhóm có thể khác. Nếu được thêm thì có thể dùng BCrypt để băm mật khẩu và thư viện xuất PDF.
3. **Hai luật đề không nói, nhóm đã tự chọn mặc định** (đổi được nếu giảng viên yêu cầu khác):
   - **B6**: member chỉ được hủy đăng ký lớp khi buổi học gần nhất còn cách hơn **24 giờ** (nhóm đã chọn 24 giờ thay cho 2 giờ ban đầu). Con số nằm ở một chỗ: hằng số `Rules.CancelHoursBefore`. Cách khác: cho hủy tự do, hoặc mốc ngắn hơn.
   - **P3**: mua gói khi đang còn gói thì gói mới **nối tiếp** sau gói xa nhất chưa hết hạn (không mất ngày đã trả tiền); member vẫn dùng gói hiện tại cho tới ngày hết hạn rồi mới sang gói mới. Logic nằm ở hàm `Rules.NextStartDate`. Cách khác: không cho mua khi còn hạn, hoặc gói mới thay thế gói cũ.
4. **Lớp có nhiều ngày hoặc nhiều ca trong tuần**: hiện mỗi lớp có đúng **1 thứ + 1 ca**. Muốn dạy nhiều buổi thì tạo nhiều lớp (ví dụ `YOGA01-T2`, `YOGA01-T4`), mỗi lớp đăng ký riêng; cách này không cần sửa gì và đủ cho phạm vi bài. Nếu giảng viên muốn **một lớp có nhiều buổi**, đổi mô hình thành bảng `ClassSchedules(ClassId, DayOfWeek, SlotId)` (một lớp – nhiều dòng lịch, nhiều ca trong cùng một ngày cũng là nhiều dòng) và thêm `SlotId` vào `Attendances` (khóa `RegistrationId + SessionDate + SlotId`). Kéo theo sửa: trùng phòng/HLV (C2, C3) xét mọi dòng lịch, trùng lịch member (B5) xét giao của hai tập lịch, hủy đăng ký (B6) lấy buổi gần nhất trong mọi dòng lịch, danh sách buổi điểm danh (`Rules.SessionDates`) trả về cặp (ngày, ca), màn Lịch của tôi và form tạo lớp (thêm nhiều dòng thứ + ca).
5. **Việc điểm danh của lễ tân**: đề ghi lễ tân "điểm danh thành viên khi đến trung tâm". Nhóm đã bỏ việc điểm danh **buổi học** khỏi lễ tân (chỉ coach làm), nên hiện lễ tân chưa có chức năng riêng cho dòng này. Nếu giảng viên hỏi, hướng làm là **check-in gym**: thêm cờ `IncludesGym` trên `MembershipPackages` và bảng `GymVisits(MemberId, VisitDate, CheckInTime, CheckedInBy)`; lễ tân check-in khi member có gói còn hạn **và** gói đó gồm gym, mỗi ngày một lượt. Cách này không đụng luật nối tiếp gói (P3) nên nhẹ hơn tạo loại gói gym riêng.
