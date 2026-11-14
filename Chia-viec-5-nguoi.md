# Chia việc cho 5 người: Sports Center

Chia theo **3 luồng bắt buộc của đề** (tài khoản và gói, lớp và đặt lớp, thanh toán và báo cáo). Mỗi người **sở hữu trọn một nhóm file** (màn hình, luật và entity của bảng mình), **mỗi file chỉ có một chủ**. Vì vậy 5 người làm song song, commit và push phần mình mà không phải sửa file của nhau.

Đọc kèm: `Ban-do-doc-code.md` (đọc file nào trước, luồng chính, bước demo, câu hay bị hỏi của từng người) và `Sports-Center-Project.md` (phạm vi, luật nghiệp vụ, cơ sở dữ liệu).

---

## 1. Bảng chia

| Người | Phần | Màn hình | Luật | Bảng dữ liệu | Dòng code (.cs / .xaml) |
|---|---|---|---|---|---|
| 1 | Tài khoản và khung ứng dụng | Login, Register, Đổi mật khẩu (hộp thoại), `MainWindow` (menu theo role), Tài khoản | A1, A2, A3, A5, đổi mật khẩu | Users | 851 / 180 |
| 2 | Member, gói thành viên, nhật ký | Member, Gói thành viên, Mua / gia hạn gói, Lịch sử hoạt động | P1-P5, A4 | Members, MembershipPackages, Subscriptions, AuditLogs | 749 / 181 |
| 3 | Quản lý lớp học | Lớp học, Bộ môn và phòng, Lớp của tôi (coach) | C1-C4 | Classes, Sports, Rooms, TimeSlots, Coaches | 750 / 176 |
| 4 | Đặt lớp, lịch cá nhân, điểm danh | Đặt lớp, Lịch của tôi, Điểm danh (coach) | B1-B8, điểm danh | Registrations, Attendances | 782 / 157 |
| 5 | Thanh toán, báo cáo, nền dùng chung | Thanh toán, Báo cáo; bộ dùng chung `App.xaml`, `GridHelper`, `PagerControl`, `Msg`; script `SportsCenter.sql` | Y1-Y4, R1-R4 | Payments | 805 / 335 và 345 dòng SQL |

Số dòng đã gồm luật của mình trong các file `Rules*.cs` và entity của bảng mình. Người 5 nhiều dòng XAML và SQL hơn nhưng đó là style và dữ liệu mẫu, ít logic nghiệp vụ; số dòng `.cs` của 5 người chỉ chênh nhau khoảng 100 (từ 749 đến 851).

---

## 2. File của từng người

Mỗi cặp `.xaml` + `.xaml.cs` tính là một mục. Đường dẫn tính từ `SportsCenterApp/SportsCenter/`.

| Người | File sở hữu |
|---|---|
| 1 | `LoginWindow`, `RegisterWindow`, `ChangePasswordWindow`, `MainWindow`, `Pages/UsersPage`; `Helpers/Session.cs`, `PasswordHelper.cs`, `AccountHelper.cs`; `Entities/User.cs` |
| 2 | `Pages/MembersPage`, `PackagesPage`, `BuyPage`, `AuditPage`; `Helpers/AuditHelper.cs`, `Rules.Packages.cs`; `Entities/Member.cs`, `MembershipPackage.cs`, `Subscription.cs`, `AuditLog.cs` |
| 3 | `Pages/ClassesPage`, `SportsRoomsPage`, `CoachPage`; `Helpers/Rules.Classes.cs`; `Entities/Class.cs`, `Coach.cs`, `Room.cs`, `Sport.cs`, `TimeSlot.cs` |
| 4 | `Pages/BookingPage`, `MySchedulePage`, `AttendancePage`; `Helpers/Rules.Booking.cs`; `Entities/Registration.cs`, `Attendance.cs` |
| 5 | `Pages/PaymentsPage`, `DashboardPage`; `App.xaml(.cs)`, `PagerControl`; `Helpers/GridHelper.cs`, `Msg.cs`, `Rules.cs` (phần lõi: tên hiển thị, hóa đơn); `Entities/Payment.cs`, `SportsCenterContext.cs`; `SportsCenter.sql` |

**`Helpers/Rules.cs` được tách thành 4 file** (cùng một `static partial class Rules`, gọi `Rules.TênHàm` như cũ): phần lõi của Người 5, `Rules.Packages.cs` của Người 2, `Rules.Classes.cs` của Người 3, `Rules.Booking.cs` của Người 4. Nhờ vậy mỗi người thêm hàm luật vào **file của mình**, không cùng sửa một file.

**`Prototype.html`** cũng chia theo người: mỗi màn là một hàm `sXxx`.

| Người | Hàm trong prototype |
|---|---|
| 1 | `sLogin`, `sRegister`, `sUsers`, `sPwDialog` và `MENU` |
| 2 | `sMembers`, `sPackages`, `sBuy`, `sAudit` |
| 3 | `sClasses`, `sSportsRooms`, `sCoach` |
| 4 | `sBooking`, `sMySchedule`, `sAttendance` |
| 5 | `sPayments`, `sDashboard` |

---

## 3. Chỗ nhiều người cùng đụng, và cách xử lý

Chỉ còn 5 chỗ mà nhiều người cùng thêm một dòng:

| Chỗ | Cách làm |
|---|---|
| Mục menu của role | Thêm một dòng trong `MainWindow.BuildMenu` (chủ: Người 1) |
| Tên hiển thị của hành động | Thêm một dòng vào **nhóm của mình** trong `Rules.ActionNames` (mỗi nhóm có chú thích "Người n") |
| Tiêu đề cột tiếng Việt | Thêm một dòng cuối từ điển `GridHelper.Headers` |
| `SportsCenter.sql` | Mỗi bảng có một khối riêng (tạo bảng và dữ liệu mẫu). Chỉ sửa khối bảng của mình; đổi cột thì báo người đang dùng bảng đó |
| Hai file `.md` và `Prototype.html` | Chỉ sửa mục (hoặc hàm `sXxx`) của mình |

Git báo xung đột ở các từ điển trên thì **giữ cả hai dòng**.

---

## 4. Quy tắc làm việc nhóm

1. **Mỗi người chỉ sửa file của phần mình** (bảng mục 2).
2. **Git:** mỗi người một nhánh (`nguoi1` … `nguoi5`). Chỉ `git add` đúng file của mình (không `git add -A` khi trong máy còn file của người khác). `git pull --rebase origin main` trước khi push, rồi gộp vào `main` sau khi chạy thử.
3. **Không commit `appsettings.json`** của máy mình (mỗi máy một chuỗi kết nối). Máy dùng named instance thì sửa cục bộ, xem mục 6 của `Sports-Center-Project.md`.
4. Mỗi người chạy cùng một `SportsCenter.sql` trên máy của mình.
5. **Trước khi gộp:** chạy lại `SportsCenter.sql`, rồi làm đúng các thao tác ở mục **Demo** của phần mình trong `Ban-do-doc-code.md`.
6. Mỗi người chuẩn bị trình bày phần của mình: luồng chính cộng ba câu hay bị hỏi (có sẵn trong bản đồ).

---

## 5. Thứ tự làm (phụ thuộc)

Người 5 chốt script và bộ dùng chung trước → Người 1 làm khung đăng nhập và menu → Người 2 (gói) và Người 3 (lớp) làm song song → Người 4 đặt lớp cần gói của Người 2 và lớp của Người 3 → báo cáo của Người 5 đọc dữ liệu của tất cả.
