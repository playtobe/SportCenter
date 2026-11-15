# CẨM NANG NGƯỜI 1: TÀI KHOẢN VÀ KHUNG ỨNG DỤNG
## DỰ ÁN SPORTS CENTER (PRN212 - WPF & EF CORE 8)

> **Mục tiêu tài liệu**: Tóm tắt toàn bộ phạm vi trách nhiệm, cấu trúc mã nguồn, quy tắc nghiệp vụ, kịch bản demo và các câu hỏi phản biện bảo vệ đồ án dành riêng cho **Người 1**.

---

## 📑 MỤC LỤC TRONG THƯ MỤC NÀY
1. [README.md](file:///D:/github-code/SportCenter/Nguoi-1-Tai-khoan-va-khung-ung-dung/README.md) – Tài liệu tổng quan, nghiệp vụ, kịch bản thuyết trình & câu hỏi bảo vệ (file này).
2. [NGUOI-1-GIAI-THICH-CODE-CHI-TIET-TUNG-DONG.md](file:///D:/github-code/SportCenter/Nguoi-1-Tai-khoan-va-khung-ung-dung/NGUOI-1-GIAI-THICH-CODE-CHI-TIET-TUNG-DONG.md) – Giải thích chi tiết từng dòng code của toàn bộ 9 tệp tin Người 1 phụ trách (chuẩn PRN212).

---

## 👤 1. PHẠM VI TRÁCH NHIỆM CỦA NGƯỜI 1

Người 1 chịu trách nhiệm về **hạ tầng xác thực, phiên làm việc, khung giao diện ứng dụng và quản trị tài khoản người dùng**:

| Nhóm | Tệp tin (Mã nguồn C# & XAML) | Vai trò chính |
| :--- | :--- | :--- |
| **Thực thể (Entity)** | `Entities/User.cs` | Mô hình ánh xạ bảng `Users` trong CSDL SQL Server |
| **Tiện ích (Helpers)** | `Helpers/PasswordHelper.cs` | Băm mật khẩu một chiều chuẩn SHA-256 (chuỗi Hex in hoa) |
| | `Helpers/Session.cs` | Lưu vết phiên làm việc toàn cục (`CurrentUser`, `CurrentMember`, `CurrentCoach`) |
| | `Helpers/AccountHelper.cs` | Chuẩn hóa tiếng Việt, Regex kiểm tra dữ liệu, thuật toán liên kết hồ sơ quầy |
| **Cửa sổ (Windows)** | `LoginWindow.xaml` / `.xaml.cs` | Màn hình đăng nhập hệ thống, kiểm tra mật khẩu & trạng thái khóa |
| | `RegisterWindow.xaml` / `.xaml.cs` | Màn hình hội viên tự đăng ký, thuật toán tự động nhận diện hồ sơ tạo tại quầy |
| | `MainWindow.xaml` / `.xaml.cs` | Khung ứng dụng chính (Shell), thanh điều hướng menu động theo 4 vai trò |
| | `ChangePasswordWindow.xaml` / `.xaml.cs` | Hộp thoại đổi mật khẩu của tài khoản đang đăng nhập |
| **Trang quản trị (Page)** | `Pages/UsersPage.xaml` / `.xaml.cs` | Màn hình Quản lý tài khoản dành cho Manager (cấu trúc chuẩn câu 2 PRN212) |

---

## ⚖️ 2. MA TRẬN 4 QUY TẮC NGHIỆP VỤ CỐT LÕI (RULES)

| Mã luật | Tên quy tắc | Nơi xử lý trong code | Mô tả chi tiết nghiệp vụ |
| :---: | :--- | :--- | :--- |
| **A1** | **Đăng nhập an toàn** | `LoginWindow.xaml.cs` | Mật khẩu nhập vào được băm bằng SHA-256 rồi so sánh với `PasswordHash` trong DB. Nếu sai username hoặc sai mật khẩu thì chỉ báo thông báo chung: *"Sai tên đăng nhập hoặc mật khẩu"*. Nếu tài khoản có `IsActive == false` thì chặn đăng nhập và báo *"Tài khoản đã bị khóa"*. |
| **A2** | **Liên kết hồ sơ quầy** | `AccountHelper.cs`<br>`RegisterWindow.xaml.cs` | Khách hàng đã từng đăng ký tại quầy lễ tân (chưa có tài khoản User, `UserId == null`) khi tự đăng ký online: Nếu nhập đúng Số điện thoại và đúng Họ tên (sau khi chuẩn hóa `Normalize`) $\rightarrow$ Hệ thống hiện popup hỏi có muốn gắn tài khoản mới vào hồ sơ cũ không để giữ lại toàn bộ gói tập và lịch sử. Cả 2 thao tác được lưu trong đúng 1 lần `SaveChanges()`. |
| **A3** | **Bảo vệ tài khoản Quản lý** | `Pages/UsersPage.xaml.cs` | Quản lý (Manager) không bao giờ được phép tự khóa tài khoản của chính mình hoặc tự hạ quyền/đổi vai trò của chính mình (`id == Session.CurrentUser.UserId`). |
| **A5** | **Duy nhất số điện thoại** | `AccountHelper.cs`<br>`RegisterWindow.xaml.cs` | Mỗi số điện thoại chỉ thuộc về DUY NHẤT một hội viên trong toàn bộ hệ thống (kết hợp với chỉ mục `UNIQUE INDEX UX_Members_Phone` trong SQL Server). |

---

## 🎬 3. KỊCH BẢN DEMO THUYẾT TRÌNH (TỪNG BƯỚC)

Khi báo cáo hoặc thi thực hành, Người 1 thực hiện demo theo đúng 5 bước mạch lạc sau:

### Bước 1: Demo Đăng nhập & Đổi mật khẩu
1. Mở ứng dụng $\rightarrow$ giao diện `LoginWindow` xuất hiện.
2. Nhập sai mật khẩu: `manager1` / `111111` $\rightarrow$ Hệ thống cảnh báo: *"Sai tên đăng nhập hoặc mật khẩu."*
3. Nhập đúng mật khẩu: `manager1` / `123456` $\rightarrow$ Đăng nhập thành công, mở `MainWindow`.
4. Bấm nút **"Đổi mật khẩu"** ở góc dưới thanh menu:
   - Nhập mật khẩu hiện tại: `123456`, mật khẩu mới: `654321`, xác nhận: `654321`.
   - Bấm "Lưu thay đổi" $\rightarrow$ Báo thành công.
   - Bấm "Đăng xuất" $\rightarrow$ Đăng nhập lại với mật khẩu cũ `123456` sẽ báo lỗi, nhập `654321` đăng nhập thành công. Đổi lại về `123456` cho các bạn sau demo.

### Bước 2: Demo Menu động theo 4 vai trò (Role-based Navigation)
- Đăng nhập `manager1` $\rightarrow$ Thấy đủ menu quản trị: Báo cáo, Lớp học, Bộ môn, Member, Gói, Thanh toán, Tài khoản, Lịch sử.
- Đăng xuất, đăng nhập `reception1` $\rightarrow$ Chỉ thấy menu Lễ tân: Member, Mua gói, Đặt lớp, Thanh toán.
- Đăng nhập `coach1` $\rightarrow$ Chỉ thấy menu HLV: Lớp của tôi, Điểm danh.
- Đăng nhập `member1` $\rightarrow$ Chỉ thấy menu Hội viên: Lịch của tôi, Đặt lớp, Gói thành viên, Thanh toán.

### Bước 3: Demo Khóa tài khoản & Luật A3 (Chặn tự khóa mình)
1. Đăng nhập bằng `manager1`, vào trang **"Tài khoản"** (`UsersPage`).
2. Chọn tài khoản `manager1` trong danh sách $\rightarrow$ Bấm nút "Khóa / Mở khóa" hoặc "Đổi vai trò" $\rightarrow$ Hệ thống chặn ngay: *"Không thể khóa hoặc đổi vai trò của chính tài khoản đang đăng nhập."*
3. Chọn tài khoản `member5` $\rightarrow$ Bấm "Khóa / Mở khóa" $\rightarrow$ Trạng thái chuyển thành "Đã khóa".
4. Đăng xuất, thử đăng nhập lại bằng `member5` / `123456` $\rightarrow$ Hệ thống chặn: *"Tài khoản đã bị khóa. Vui lòng liên hệ ban quản lý."*
5. Đăng nhập lại bằng `manager1` $\rightarrow$ Mở khóa lại cho `member5`.

### Bước 4: Demo Tự đăng ký & Thuật toán liên kết hồ sơ quầy (Luật A2, A5)
1. Tại màn hình `LoginWindow`, bấm **"Đăng ký tài khoản"** $\rightarrow$ Mở `RegisterWindow`.
2. **Thử nghiệm 1 (Trùng SĐT của người khác)**:
   - Nhập họ tên: "Trần Văn A", SĐT: `0910000001` (SĐT của member1) $\rightarrow$ Báo lỗi: *"Số điện thoại này đã được đăng ký."*
3. **Thử nghiệm 2 (Liên kết hồ sơ quầy thành công)**:
   - Trong CSDL có sẵn khách hàng tạo tại quầy tên **"Lê Hải"**, SĐT: **`0920000001`** (chưa có tài khoản User).
   - Nhập họ tên: `Lê  Hải` (cố tình gõ khoảng trắng thừa để test hàm `Normalize`).
   - Nhập SĐT: `0920000001`.
   - Nhập username: `lehai_online`, mật khẩu: `123456`.
   - Bấm **"Tạo tài khoản"** $\rightarrow$ Hệ thống hiện popup xác nhận: *"Đã có hồ sơ tại quầy của Lê Hải (SĐT 0920000001)... Gắn tài khoản mới vào hồ sơ đó để xem lại gói, hóa đơn và lớp đã đăng ký?"*
   - Chọn **Yes** $\rightarrow$ Đăng ký thành công, tự động đăng nhập vào xem thấy nguyên vẹn gói tập và lịch sử cũ!

---

## ❓ 4. CÂU HỎI BẢO VỆ PHẢN BIỆN HAY GẶP (VÀ CÁCH TRẢ LỜI)

### Câu 1: Em lưu mật khẩu người dùng trong CSDL như thế nào? Có an toàn không?
> **Trả lời**: "Dạ thưa thầy/cô, hệ thống hoàn toàn không lưu mật khẩu dạng chuỗi thô. Toàn bộ mật khẩu được băm một chiều bằng thuật toán SHA-256 qua lớp `PasswordHelper.cs`. Kết quả băm là chuỗi Hex 64 ký tự in hoa, khớp 100% với hàm `HASHBYTES('SHA2_256', ...)` của SQL Server. Khi đăng nhập, ứng dụng băm mật khẩu người dùng vừa gõ và so khớp với chuỗi băm trong DB."

### Câu 2: Nếu một người khách đã mua gói tại quầy nhưng sau đó về nhà tải app và tự tạo tài khoản thì dữ liệu có bị tách làm 2 hồ sơ không?
> **Trả lời**: "Dạ không, đó chính là **Luật A2** do em thiết kế. Trong `AccountHelper.cs`, em có hàm `CheckPhoneForRegistration` và `FindCounterProfiles`. Khi khách đăng ký online bằng số điện thoại đó và đúng họ tên (đã được hàm `Normalize` loại bỏ dấu tiếng Việt và khoảng trắng thừa), hệ thống phát hiện đây là hồ sơ quầy chưa có tài khoản (`UserId == null`) và hiện hộp thoại hỏi khách có muốn liên kết không. Khi khách chọn Đồng ý, hàm `CreateMemberAccount` sẽ gán `User` mới vào bản ghi `Member` cũ trong đúng một lần `SaveChanges()`, giữ nguyên 100% gói tập, hóa đơn và lịch tập cũ."

### Câu 3: Làm thế nào ứng dụng phân quyền được các menu chức năng khác nhau cho từng người dùng?
> **Trả lời**: "Dạ, khi đăng nhập thành công, lớp `Session.SignIn` sẽ lưu thông tin người dùng và vai trò vào biến static `Session.CurrentUser`. Khi `MainWindow` khởi tạo, phương thức `BuildMenu(user.Role)` sử dụng cấu trúc `switch-case` theo 4 vai trò (`Manager`, `Receptionist`, `Coach`, `Member`) để nạp các nút bấm (`AddItem`) hoặc các nhóm xổ xuống (`AddGroup`) tương ứng vào thanh bên trái. Người dùng không có quyền sẽ không nhìn thấy và không thể truy cập vào các trang khác."

### Câu 4: Vì sao trên trang Quản lý tài khoản (UsersPage) quản trị viên không thể tự khóa mình?
> **Trả lời**: "Dạ thưa thầy/cô, đó là **Luật A3** nhằm ngăn ngừa tình trạng hệ thống bị 'mồ côi quản trị' (Deadlock). Trong phương thức `GetSelectedUser()` của `UsersPage.xaml.cs`, em kiểm tra nếu ID của dòng được chọn trùng với `Session.CurrentUser.UserId` thì hệ thống lập tức xuất cảnh báo và không cho phép thực hiện thao tác khóa hoặc đổi vai trò."

---

## 🔗 5. MỐI QUAN HỆ VỚI CÁC THÀNH VIÊN KHÁC
- **Người 2, 3, 4, 5**: Đều sử dụng `Session.CurrentUser`, `Session.CurrentMember`, `Session.CurrentCoach` và `Session.Role` để nhận diện ngữ cảnh người đang thao tác.
- **Người 2 (Member & Gói)**: Sử dụng `AccountHelper.FindByPhone` để kiểm tra trùng SĐT khi thêm member tại quầy, và dùng `AccountHelper.Normalize` để chuẩn hóa tìm kiếm họ tên.
- **Toàn bộ các trang con (UserControl)**: Đều có thể chuyển trang thông qua thể hiện tĩnh `MainWindow.Instance?.ShowPage(...)`.
