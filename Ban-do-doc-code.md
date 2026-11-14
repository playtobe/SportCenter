# Bản đồ đọc code: Sports Center

Mỗi người đọc **mục 0 và mục 1 (cả nhóm)**, sau đó chỉ đọc **phần của mình**. Phần cuối là bảng tra nhanh từ mã luật sang file và hàm. Cách chia việc (ai làm file nào, quy tắc git) nằm trong `Chia-viec-5-nguoi.md`.

Quy ước: `Tên.xaml` là giao diện, `Tên.xaml.cs` là code-behind của nó. Đường dẫn bên dưới tính từ `SportsCenterApp/SportsCenter/`. Khi cần tìm hàm, hãy tìm theo **tên hàm** (số dòng đổi liên tục nên không ghi).

---

## 0. Chương trình chạy thế nào (5 phút)

```
App.xaml (StartupUri = LoginWindow, kho style dùng chung)
   │
   ▼
LoginWindow.btnLogin_Click ── sai mật khẩu / bị khóa → báo bằng Msg.Warn
   │  đúng
   ▼
Session.SignIn   (nhớ CurrentUser; nếu là Member nạp CurrentMember; Coach nạp CurrentCoach; ghi nhật ký "Login")
   │
   ▼
MainWindow  ──  BuildMenu(role): dựng menu theo role (AddItem = mục đơn, AddGroup = nhóm xổ xuống)
   │               ShowPage(page): đưa một Page vào vùng nội dung bên phải
   ▼
Một Page (UserControl) = một màn hình
```

**Khuôn của mọi Page** (hiểu một màn là hiểu cả 13 màn):

1. Hàm tạo: `InitializeComponent(); _db = new(); GridHelper.Setup(dgX); pgX.PageChanged += ...; LoadX();`
2. `LoadX()`: lọc bằng `query.Where(...)`, chọn cột bằng `Select(new { ... }).ToList()`, rồi `dgX.ItemsSource = pgX.Slice(rows)` (mỗi trang 8 dòng).
3. Mỗi nút bấm là một hàm `btnXxx_Click`: đọc ô nhập → kiểm tra (lỗi thì `Msg.Warn` rồi `return`) → `try { thao tác; _db.SaveChanges(); AuditHelper.Log(...); LoadX(); Msg.Info(...) } catch (Exception ex) { Msg.Error(ex); }`.
4. Luật dùng chung giữa nhiều màn nằm trong các file `Helpers/Rules*.cs` và `Helpers/AccountHelper.cs`. Hàm trả về `string?` nghĩa là **thông báo lỗi, hoặc `null` nếu hợp lệ**.

Mỗi Page tự giữ một `_db` riêng (giống Q2). Luôn đọc bằng `Select(...)` hoặc `AsNoTracking()` để dữ liệu mới nhất, không bị cũ khi màn khác vừa sửa.

---

## 1. Bộ dùng chung (cả nhóm đọc, chỉ cần đọc lướt)

| File | Làm gì |
|---|---|
| `SportsCenter.sql` (ở folder ngoài) | Tạo database, 13 bảng, dữ liệu mẫu. **Chạy lại nhiều lần được** (tự xóa DB cũ). |
| `Entities/*.cs` | 13 class, mỗi class tương ứng một bảng. `SportsCenterContext.cs`: `OnConfiguring` đọc chuỗi kết nối từ `appsettings.json`; `HasKey` cho 3 bảng khóa không tên chuẩn. |
| `Helpers/Session.cs` | Người đang đăng nhập: `CurrentUser`, `CurrentMember`, `CurrentCoach`, `Role`. |
| `Helpers/PasswordHelper.cs` | `Hash`: SHA-256, chuỗi hex IN HOA. |
| `Helpers/AuditHelper.cs` | `Log(db, action, details, ...)`: ghi một dòng vào `AuditLogs`. |
| `Helpers/Msg.cs` | `Warn`, `Info`, `Error`, `Confirm`: bọc `MessageBox` cho ngắn. |
| `Helpers/GridHelper.cs` | `Setup`: tự sinh cột DataGrid, **ẩn cột có tên kết thúc bằng `Id`**, đổi tiêu đề sang tiếng Việt (từ điển `Headers`); `SelectedId`, `SelectById`. |
| `PagerControl.xaml(.cs)` | Thanh phân trang « ‹ 1 2 3 › »: `Slice<T>`, `Reset`, sự kiện `PageChanged`. |
| `App.xaml` | Style dùng chung: nút (`SecondaryButton`, `DangerButton`, `MenuButton`), `PageTitle`, `ErrText`, bảng dữ liệu. |
| `Helpers/Rules*.cs` | Luật dùng chung. Là **một class `Rules` chia thành 4 file**, mỗi file một chủ, gọi vẫn là `Rules.TênHàm`: `Rules.cs` (Người 5: `Today`, `RoleNames`, `MethodNames`, `ActionNames`, `DayNames`, `ScheduleText`, `NextInvoiceNo`), `Rules.Packages.cs` (Người 2: gói thành viên), `Rules.Classes.cs` (Người 3: ca trống, trùng lịch, hủy lớp), `Rules.Booking.cs` (Người 4: đặt lớp, hủy đăng ký, buổi điểm danh). |

Dữ liệu mẫu để demo (mật khẩu chung `123456`): `manager1`, `reception1`, `coach1..3`, `member1..5`. Có sẵn `GYM01` đã đầy, `member4` hết hạn gói, khách "Lê Hải" (SĐT `0920000001`) chưa có tài khoản.

---

## 2. Người 1: Tài khoản và khung ứng dụng

**File:** `LoginWindow`, `RegisterWindow`, `ChangePasswordWindow`, `MainWindow`, `Pages/UsersPage` (mỗi cái gồm `.xaml` và `.xaml.cs`); `Helpers/Session.cs`, `PasswordHelper.cs`, `AccountHelper.cs`; `Entities/User.cs`.
**Bảng:** `Users` (và dòng `Members` tạo kèm khi đăng ký). **Luật:** A1, A2, A3, A5, đổi mật khẩu.

**Đọc theo thứ tự**
1. `SportsCenter.sql`: bảng `Users` (`Username` UNIQUE), `Members` (`UX_Members_UserId`, `UX_Members_Phone`).
2. `PasswordHelper.Hash`, `Session.SignIn`.
3. `LoginWindow.btnLogin_Click` (luật A1): tìm user, so hash, kiểm tra `IsActive`, `Session.SignIn`, mở `MainWindow`.
4. `MainWindow`: hàm tạo → `BuildMenu` (một `switch` theo role) → `MakeButton`, `AddItem`, `AddGroup`, `ShowPage`; `btnChangePassword_Click`, `btnLogout_Click`.
5. `AccountHelper`: `Normalize` (bỏ dấu, hoa thường), `FindByPhone`, `CheckPhoneForRegistration` (trả về `PhoneCheck.Free`, `LinkCounterProfile` hoặc `AlreadyRegistered`), `FindCounterProfiles`, `CreateMemberAccount`, `UsernameError`, `FullNameError`, `IsValidEmail`.
6. `RegisterWindow.btnCreate_Click` (A2, A5).
7. `ChangePasswordWindow.btnChange_Click`.
8. `UsersPage`: `LoadUsers`, `btnAddUser_Click`, `btnToggle_Click`, `btnChangeRole_Click`, `EnsureProfile`, `GetSelectedUser`.

**Luồng "tự đăng ký có gắn hồ sơ tại quầy"** (phần dễ bị hỏi nhất)
1. `btnCreate_Click` kiểm tra từng ô, lỗi hiện **ngay dưới ô** (`errFullName`, `errPhone`, `errUsername`...).
2. `AccountHelper.CheckPhoneForRegistration(họ tên, SĐT)`:
   - số chưa ai dùng → `Free`: tạo hồ sơ mới;
   - số thuộc hồ sơ tại quầy chưa có tài khoản **và đúng họ tên** → `LinkCounterProfile`: hỏi gắn (`Msg.Confirm`); chọn "Không" thì **không tạo gì**;
   - số thuộc người khác → `AlreadyRegistered`: báo lỗi ở ô số điện thoại.
3. `AccountHelper.CreateMemberAccount`: tạo `User` và gắn vào hồ sơ cũ (giữ nguyên gói, hóa đơn, lớp), cùng một lần `SaveChanges`.
4. `Session.SignIn`, ghi nhật ký `LinkMember`, mở `MainWindow`.

**Demo:** đăng ký với tên "Lê Hải" + SĐT `0920000001` → hỏi gắn; dùng SĐT `0910000001` với tên khác → báo đã đăng ký; `manager1` khóa `member5` rồi đăng nhập `member5` → báo bị khóa.

**Hay bị hỏi:** "Mật khẩu lưu thế nào?" → `PasswordHelper.Hash`. "Menu theo role ở đâu?" → `MainWindow.BuildMenu`. "Chống trùng số điện thoại ở đâu?" → `AccountHelper.CheckPhoneForRegistration` + chỉ mục `UX_Members_Phone`. "Manager có tự khóa mình được không?" → `UsersPage.GetSelectedUser` chặn.
**Giao nhau:** Người 2 gọi `AccountHelper.FindByPhone`; mọi màn dùng `Session`.

---

## 3. Người 2: Member, gói thành viên và nhật ký hoạt động

**File:** `Pages/MembersPage`, `PackagesPage`, `BuyPage`, `AuditPage`; `Helpers/AuditHelper.cs`, `Helpers/Rules.Packages.cs`; `Entities/Member.cs`, `MembershipPackage.cs`, `Subscription.cs`, `AuditLog.cs`.
**Bảng:** `Members`, `MembershipPackages`, `Subscriptions`, `Payments` (được tạo khi mua gói), `AuditLogs`. **Luật:** P1-P5, A4.

**Đọc theo thứ tự**
1. `SportsCenter.sql`: `MembershipPackages`, `Subscriptions` (`CHECK EndDate >= StartDate`), `Payments`.
2. `Rules.Packages.cs`: `ActiveSubscription` (P4), `CurrentOrLatest`, `SubscriptionText`, `NextStartDate` (P3), `QueuedSubscriptions`.
3. `PackagesPage`: `LoadPackages`, `btnAdd_Click` (P1: giá, số ngày, tên không trùng), `btnToggle_Click` (ngừng bán / mở bán lại).
4. `MembersPage`: `LoadMembers` (ghép trạng thái gói của từng member), `btnAddMember_Click` (A5: số điện thoại duy nhất), `btnBuy_Click`. Hàm tạo ẩn nút và khung bán gói với Manager.
5. `BuyPage`: hàm tạo, `LoadMemberPicker` (member tự mua thì ẩn ô chọn member), `ShowCurrent` (gói hiện tại và gói đã mua nối tiếp), `ShowPreview`, `btnConfirm_Click` (P2, Y2).
6. `AuditPage`: `LoadData` (đổ ô lọc, tên hành động qua `Rules.ActionName`), `LoadLogs`.

**Luồng "Mua gói"** (`BuyPage.btnConfirm_Click`)
1. Lấy member (`GetMemberId`) và gói đang chọn; gói ngừng bán thì báo.
2. `Rules.NextStartDate` → ngày bắt đầu (nối tiếp sau gói xa nhất chưa hết hạn, không thì hôm nay).
3. Tạo `Subscription` (`EndDate = Start + DurationDays - 1`) và `Payment` (số hóa đơn từ `Rules.NextInvoiceNo`).
4. **Một lần `SaveChanges`** cho cả hai nên là một transaction (Y2): lỗi thì không lưu gì.
5. `AuditHelper.Log("Payment")`, làm mới ô "gói hiện tại", `Msg.Info`.

**Demo:** `reception1` → Member → chọn Vũ Anh Tuấn → Mua / gia hạn gói → mua hai lần, thấy nhãn "nối tiếp" và dòng "Đã mua nối tiếp"; `manager1` → Gói thành viên → thêm gói tên trùng bị chặn.

**Hay bị hỏi:** "Gói nối tiếp tính thế nào?" → `Rules.NextStartDate`. "Vì sao không xóa được gói đã bán?" → chỉ ngừng bán (`btnToggle_Click`). "Vì sao Manager không bán gói?" → quyết định ở mục 1 của tài liệu.
**Giao nhau:** Người 4 gọi `Rules.ActiveSubscription` (luật B1); `PaymentsPage` của Người 5 hiển thị `Payments` do `BuyPage` tạo.

---

## 4. Người 3: Quản lý lớp học

**File:** `Pages/ClassesPage`, `SportsRoomsPage`, `CoachPage`; `Helpers/Rules.Classes.cs` (`BusySet`, `ClassConflictMessage`, `CancelClassBlockedReason`); `Entities/Class.cs`, `Coach.cs`, `Room.cs`, `Sport.cs`, `TimeSlot.cs`.
**Bảng:** `Classes`, `Sports`, `Rooms`, `TimeSlots`, `Coaches` (đếm `Registrations` để chặn hủy lớp). **Luật:** C1-C4.

**Đọc theo thứ tự**
1. `SportsCenter.sql`: `TimeSlots` (6 ca), `Classes` (`DayOfWeek` 1-7 và `SlotId`; **DB không chặn trùng lịch, việc đó làm trong code**).
2. `SportsRoomsPage`: `LoadSports`, `LoadRooms`, `btnAddSport_Click`, `btnAddRoom_Click` (trùng tên), `btnDeleteSport_Click`, `btnDeleteRoom_Click` (không xóa khi còn lớp dùng).
3. `Rules.Classes.cs`: `BusySet` (tập (thứ, ca) đã bận của phòng hoặc HLV) và `ClassConflictMessage` (C2, C3).
4. `ClassesPage`: `LoadData` → `LoadFilters`, `LoadFormCombos`, `LoadClassList`; **form thêm lớp chỉ hiện thứ và ca còn trống**: `cbxCoachRoom_SelectionChanged` → `RefreshFreeDays` → `cbxDay_SelectionChanged` → `RefreshFreeSlots`; `btnAddClass_Click` (C1, kiểm tra lại C2 và C3); `btnCancelClass_Click` (C4); `ClearForm`.
5. `CoachPage`: `LoadClasses` (chỉ lớp của coach đăng nhập, qua `Session.CurrentCoach`), `cbxClass_SelectionChanged` → `LoadStudents`.

**Luồng "thêm lớp"**
1. Chọn HLV và phòng → `RefreshFreeDays` dùng `Rules.BusySet` để chỉ liệt kê thứ còn trống; HLV và phòng kín cả tuần thì khóa nút thêm.
2. Chọn thứ → `RefreshFreeSlots` liệt kê ca còn trống.
3. `btnAddClass_Click` kiểm tra mã lớp không trùng, sĩ số > 0 và không vượt sức chứa phòng, `EndDate >= StartDate` (C1), rồi gọi lại `Rules.ClassConflictMessage` lần nữa.
4. Lưu `Classes`, ghi nhật ký `CreateClass`, làm mới các ô còn trống.

**Demo:** `manager1` → Lớp học → thử hủy `YOGA01` (đang có 2 học viên) bị chặn; hủy `BOX02` (0 học viên) được; `coach1` → Lớp của tôi.

**Hay bị hỏi:** "Chặn trùng phòng làm ở đâu?" → `Rules.ClassConflictMessage`. "Lớp có nhiều buổi mỗi tuần?" → mục 9 (4) của tài liệu. "Vì sao hủy lớp không xóa dòng?" → chỉ đổi `Status = Cancelled`.
**Giao nhau:** Người 4 đọc `Classes` để đặt lớp và điểm danh; hủy lớp phụ thuộc `Registrations` của Người 4.

---

## 5. Người 4: Đặt lớp, lịch cá nhân và điểm danh

**File:** `Pages/BookingPage`, `MySchedulePage`, `AttendancePage`; `Helpers/Rules.Booking.cs` (`NextSession`, `SessionDates`, `RegisterClass`, `CancelRegistration`); `Entities/Registration.cs`, `Attendance.cs`.
**Bảng:** `Registrations`, `Attendances`. **Luật:** B1-B8 và luật điểm danh.

**Đọc theo thứ tự**
1. `SportsCenter.sql`: `Registrations` (chỉ mục duy nhất có điều kiện `Status = 'Registered'` cho cặp member + lớp), `Attendances` (`UNIQUE` theo đăng ký + ngày).
2. `Rules.Booking.cs`: `RegisterClass` (B1-B5), `NextSession`, `CancelRegistration` (B6, hằng `CancelHoursBefore` = 24).
3. `BookingPage`: hàm tạo, `LoadMemberPicker` và `GetMemberId` (B8: member chỉ thao tác trên dữ liệu của mình; lễ tân chọn member), `ShowNote`, `LoadClasses`, `btnRegister_Click`, `btnCancel_Click`.
4. `MySchedulePage`: `MondayOf`, `BuildSchedule` (lưới ca × thứ), `MakeClassCell` và `MakeBadge` (huy hiệu Có mặt / Vắng / Chưa điểm danh), `ChangeWeek`, `btnPrev_Click`, `btnNext_Click`, `btnToday_Click`, `dpJump_SelectedDateChanged`.
5. `AttendancePage` (**chỉ coach**, lễ tân không có mục này): `LoadClasses` (chỉ lấy lớp của coach đăng nhập qua `Session.CurrentCoach`), `cbxClass_SelectionChanged` (gọi `Rules.SessionDates` để đổ ô **Buổi học**), `LoadRows` (mỗi dòng là một `AttendanceRow`), `UpdateSummary`, `btnAllPresent_Click`, `btnAllAbsent_Click`, `btnSave_Click`.

**Luồng "đặt lớp"** (`Rules.RegisterClass`, trả về `string?`)
1. B1: member có gói còn hạn (`ActiveSubscription`).
2. B2: lớp `Open` và chưa kết thúc.
3. B4: chưa đăng ký lớp này.
4. B3: lớp chưa đầy.
5. B5: không trùng thứ và ca với lớp đang đăng ký.
6. Qua hết thì thêm `Registration` (lưu `RegisteredBy` = người thao tác, B7) và ghi nhật ký `RegisterClass`.

**Luồng "điểm danh"**
1. Chọn lớp → `Rules.SessionDates` trả tối đa 12 buổi thật (đúng thứ của lớp, trong khoảng ngày của lớp, không muộn hơn hôm nay), mặc định chọn buổi gần nhất.
2. `LoadRows` đổ danh sách học viên, mặc định **Có mặt**; mỗi dòng là một `ToggleButton` nên bấm vào đâu trên dòng cũng đổi được.
3. `btnSave_Click` thêm hoặc sửa `Attendances` cho mọi học viên (mọi trang), ghi `CheckedInBy`, `AuditHelper.Log("Attendance")`.
4. Member mở **Lịch của tôi** thấy huy hiệu đúng ngày đó.

**Demo:** `member1` → Đặt lớp → đăng ký `GYM01` (đầy) bị báo; `coach1` → Điểm danh `BOX01` → bỏ tick một bạn → Lưu → đăng nhập `member1` → Lịch của tôi.

**Hay bị hỏi:** "Vì sao hủy trước 24 giờ?" → `Rules.CancelHoursBefore`, đổi một chỗ. "Điểm danh sao không chọn sai ngày được?" → `Rules.SessionDates`. "Lịch tuần vẽ thế nào?" → `MySchedulePage.BuildSchedule`.
**Giao nhau:** gọi `Rules.ActiveSubscription` của Người 2; đọc `Classes` của Người 3.

---

## 6. Người 5: Thanh toán, báo cáo và nền dùng chung

**File:** `Pages/PaymentsPage`, `DashboardPage`; bộ dùng chung `App.xaml(.cs)`, `Helpers/GridHelper.cs`, `Helpers/Msg.cs`, `Helpers/Rules.cs` (phần lõi: tên hiển thị, `NextInvoiceNo`), `PagerControl`, `Entities/Payment.cs`, `Entities/SportsCenterContext.cs`; `SportsCenter.sql`.
**Bảng:** `Payments` (đọc), toàn bộ database. **Luật:** Y1, Y3, Y4, R1-R4.

**Đọc theo thứ tự**
1. `SportsCenter.sql`: thứ tự xóa bảng (con trước cha), tạo 13 bảng, các chỉ mục duy nhất, phần dữ liệu mẫu (đặt đúng ngày để demo được).
2. `SportsCenterContext.OnConfiguring` (đọc `appsettings.json`) và `HasKey`.
3. `App.xaml` (các style), `GridHelper.Setup` (ẩn cột Id, từ điển `Headers`, độ rộng cột), `PagerControl` (`Slice`, `Reset`).
4. `PaymentsPage`: `LoadPayments` (**member chỉ thấy hóa đơn của mình**), `ShowInvoice` (dựng nội dung hóa đơn), `btnExport_Click` (xuất `.txt`).
5. `DashboardPage`: `LoadReport` (R1: tổng member, gói đang hoạt động, doanh thu, đăng ký), `DrawChart` + `CreateBar` + `FormatMillions` + `AddText` (R3: biểu đồ cột 6 tháng vẽ bằng `Canvas`), `LoadFill` (R2: mức lấp đầy lớp), `LoadExpiring` (R4: gói sắp hết hạn trong 7 ngày).

**Luồng "báo cáo doanh thu"**
1. `btnView_Click` kiểm tra khoảng ngày rồi gọi `LoadReport`.
2. `LoadReport` lấy các hóa đơn `Paid` trong khoảng ngày, cộng tiền cho thẻ Doanh thu.
3. Gom theo 6 tháng gần nhất tính đến tháng của "Đến ngày" → `DrawChart` vẽ cột (chọn bước chia trục cho vừa số lớn nhất).

**Demo:** `manager1` → Báo cáo (đổi khoảng ngày xem số đổi); `reception1` → Thanh toán → chọn hóa đơn → Xuất hóa đơn; chạy lại `SportsCenter.sql` hai lần liên tiếp để thấy script tự dọn.

**Hay bị hỏi:** "Biểu đồ vẽ thế nào?" → `DrawChart` dùng `Canvas`, không thư viện ngoài. "Thanh toán thật hay mô phỏng?" → mô phỏng, chỉ ghi vào DB. "Có hoàn tiền không?" → đã bỏ (luật Y3). "Chạy script nhiều lần được vì sao?" → đầu script xóa DB cũ rồi tạo lại.
**Giao nhau:** `Payments` do `BuyPage` (Người 2) tạo; `LoadFill` đọc `Registrations` (Người 4).

---

## 7. Bảng tra nhanh: mã luật nằm ở đâu

| Mã | Nội dung | File và hàm |
|---|---|---|
| A1 | Đăng nhập | `LoginWindow.btnLogin_Click` |
| A2 | Đăng ký, định dạng, gắn hồ sơ quầy | `RegisterWindow.btnCreate_Click`, `AccountHelper.*` |
| A3 | Manager không tự khóa / đổi role mình | `UsersPage.GetSelectedUser`, `EnsureProfile` |
| A4 | Ghi lịch sử hoạt động | `AuditHelper.Log` (gọi ở mọi thao tác quan trọng) |
| A5 | Số điện thoại duy nhất | `AccountHelper.CheckPhoneForRegistration`, `MembersPage.btnAddMember_Click`, chỉ mục `UX_Members_Phone` |
| P1 | Gói: giá, số ngày, tên không trùng | `PackagesPage.btnAdd_Click` |
| P2 | Mua gói | `BuyPage.btnConfirm_Click` |
| P3 | Gói nối tiếp | `Rules.NextStartDate` |
| P4 | Gói còn hạn | `Rules.ActiveSubscription` |
| P5 | Gói sắp hết hạn | `DashboardPage.LoadExpiring` |
| C1-C3 | Tạo lớp, trùng phòng, trùng HLV | `ClassesPage.btnAddClass_Click`, `Rules.ClassConflictMessage` |
| C4 | Hủy lớp chỉ khi hết học viên | `Rules.CancelClassBlockedReason`, `ClassesPage.btnCancelClass_Click` |
| B1-B5 | Đặt lớp | `Rules.RegisterClass` |
| B6 | Hủy đăng ký trước 24 giờ | `Rules.CancelRegistration`, `Rules.CancelHoursBefore` |
| B7 | Lễ tân đăng ký hộ | `Rules.RegisterClass` (lưu `RegisteredBy`), `BookingPage` |
| B8 | Member chỉ thao tác dữ liệu của mình | `BookingPage.GetMemberId`, `BuyPage.GetMemberId`, `PaymentsPage.LoadPayments` |
| Y1, Y2 | Hóa đơn, một transaction | `BuyPage.btnConfirm_Click`, `Rules.NextInvoiceNo` |
| Y4 | Xuất hóa đơn | `PaymentsPage.btnExport_Click` |
| R1-R4 | Báo cáo | `DashboardPage.LoadReport`, `LoadFill`, `LoadExpiring` |
| Điểm danh | Buổi học, lưu | `Rules.SessionDates`, `AttendancePage.btnSave_Click` |

**Mẹo khi bị hỏi bất ngờ:** (1) xác định đó là *màn hình* hay *luật*; (2) màn hình thì mở `Pages/Tên.xaml.cs`, tìm hàm `btn..._Click`; (3) luật thì mở bảng ở trên rồi vào `Helpers/Rules*.cs` (đúng file của người đó) hoặc `AccountHelper.cs`; (4) dữ liệu thì mở `SportsCenter.sql`.
