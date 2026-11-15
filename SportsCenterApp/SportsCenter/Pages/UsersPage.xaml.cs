using SportsCenter.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for UsersPage.xaml
    /// Màn hình Quản lý tài khoản người dùng (Dành cho Quản lý / Manager)
    /// Cấu trúc thiết kế chuẩn theo mẫu bài thi PRN212:
    /// - Khởi tạo DbContext
    /// - Load ComboBox bộ lọc và form thêm mới
    /// - Load dữ liệu lên DataGrid bằng LINQ (Filter theo từ khóa và ComboBox)
    /// - Các chức năng: Lọc, Xóa bộ lọc (Clear Filter), Khóa/Mở tài khoản, Đổi vai trò, Thêm tài khoản mới
    /// </summary>
    public partial class UsersPage : UserControl
    {
        // 1. Khai báo DbContext kết nối Database (chuẩn PRN212)
        private SportsCenterContext _db;

        // 2. Danh sách các mã vai trò lưu trong CSDL
        private readonly string[] _roles = { "Manager", "Receptionist", "Coach", "Member" };

        // 3. Hàm chuyển đổi mã Role trong DB sang tên hiển thị tiếng Việt
        private static string RoleName(string code)
        {
            if (Rules.RoleNames.ContainsKey(code))
            {
                return Rules.RoleNames[code];
            }
            return code;
        }

        public UsersPage()
        {
            InitializeComponent();
            _db = new SportsCenterContext(); // Khởi tạo kết nối CSDL

            // Tái sử dụng GridHelper để tự động căn chỉnh và định dạng cột DataGrid
            GridHelper.Setup(dgUsers);

            // Gán sự kiện khi bấm chuyển trang trên PagerControl thì nạp lại danh sách
            pgUsers.PageChanged += (s, e) => LoadUsers();

            // Nạp dữ liệu ban đầu cho các ComboBox và DataGrid
            LoadData();
        }

        // =====================================================================
        // Bước 1: Nạp danh sách vai trò vào các ComboBox trên giao diện
        // =====================================================================
        private void LoadData()
        {
            // Chuyển danh sách mã vai trò sang tên tiếng Việt
            List<string> roleNames = _roles.Select(RoleName).ToList();

            // 1. Nạp dữ liệu cho ComboBox Lọc (cbxRoleFilter): Thêm lựa chọn "Tất cả" ở đầu danh sách (chuẩn PRN)
            List<string> filterRoles = new List<string> { "Tất cả" };
            filterRoles.AddRange(roleNames);
            cbxRoleFilter.ItemsSource = filterRoles;
            cbxRoleFilter.SelectedIndex = 0; // Mặc định chọn "Tất cả"

            // 2. Nạp dữ liệu cho ComboBox Đổi vai trò (cbxNewRole)
            cbxNewRole.ItemsSource = roleNames;
            cbxNewRole.SelectedIndex = 3; // Mặc định chọn "Hội viên"

            // 3. Nạp dữ liệu cho ComboBox Thêm tài khoản mới (cbxRole)
            cbxRole.ItemsSource = roleNames;
            cbxRole.SelectedIndex = 1; // Mặc định chọn "Lễ tân"

            // 4. Nạp danh sách người dùng lên DataGrid
            LoadUsers();
        }

        // =====================================================================
        // Bước 2: Nạp và lọc danh sách tài khoản lên DataGrid bằng LINQ
        // (Tương tự phương thức LoadEmployee / LoadCourse trong bài PRN212)
        // =====================================================================
        private void LoadUsers()
        {
            // Khởi tạo truy vấn LINQ từ bảng Users
            var query = _db.Users.AsQueryable();

            // 1. Lọc theo từ khóa tìm kiếm (Tên đăng nhập hoặc Họ tên) nếu có nhập
            string keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.Username.Contains(keyword) || u.FullName.Contains(keyword));
            }

            // 2. Lọc theo Vai trò nếu ComboBox khác "Tất cả" (SelectedIndex > 0)
            if (cbxRoleFilter.SelectedIndex > 0)
            {
                string selectedRole = _roles[cbxRoleFilter.SelectedIndex - 1];
                query = query.Where(u => u.Role == selectedRole);
            }

            // 3. Sắp xếp theo UserId tăng dần và chọn các cột cần hiển thị
            var rows = query
                .OrderBy(u => u.UserId)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.FullName,
                    Role = RoleName(u.Role),
                    Status = u.IsActive ? "Đang hoạt động" : "Đã khóa"
                })
                .ToList();

            // 4. Đổ dữ liệu vào DataGrid (kết hợp phân trang qua pgUsers)
            dgUsers.ItemsSource = pgUsers.Slice(rows);
        }

        // =====================================================================
        // Sự kiện khi bấm nút "Lọc" (btnFilter)
        // =====================================================================
        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            pgUsers.Reset(); // Quay về trang 1
            LoadUsers();     // Nạp lại danh sách theo điều kiện lọc
        }

        // =====================================================================
        // Sự kiện khi bấm nút "Bỏ lọc" (btnClearFilter) - Chuẩn bài thi PRN
        // =====================================================================
        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();               // Xóa trắng ô tìm kiếm
            cbxRoleFilter.SelectedIndex = 0; // Đặt lại ComboBox lọc về "Tất cả"
            pgUsers.Reset();                 // Quay về trang 1
            LoadUsers();                     // Nạp lại toàn bộ danh sách ban đầu
        }

        // =====================================================================
        // Lấy thông tin tài khoản đang được chọn trong DataGrid
        // =====================================================================
        private User? GetSelectedUser()
        {
            // Lấy ID từ dòng đang chọn trên DataGrid
            int? id = GridHelper.SelectedId(dgUsers, "UserId");
            if (id == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản trong danh sách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            // Quy tắc A3: Không cho phép tự khóa hoặc đổi quyền của chính tài khoản đang đăng nhập
            if (id == Session.CurrentUser!.UserId)
            {
                MessageBox.Show("Không thể khóa hoặc đổi vai trò của chính tài khoản đang đăng nhập.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            // Tìm và trả về đối tượng User từ Database
            return _db.Users.Find(id);
        }

        // =====================================================================
        // Sự kiện khi bấm nút "Khóa / Mở khóa tài khoản" (btnToggle)
        // =====================================================================
        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            // Bước 1: Lấy tài khoản đang chọn
            User? user = GetSelectedUser();
            if (user == null) return;

            try
            {
                // Bước 2: Đảo ngược trạng thái hoạt động (Active <-> Inactive)
                user.IsActive = !user.IsActive;
                _db.SaveChanges(); // Lưu vào Database

                // Bước 3: Ghi lịch sử hoạt động (Audit Log)
                string actionText = user.IsActive ? "Mở khóa" : "Khóa";
                AuditHelper.Log(_db, "ToggleUser", $"{actionText} tài khoản {user.Username}", "Users", user.UserId);

                // Bước 4: Nạp lại danh sách và giữ dòng đang chọn
                LoadUsers();
                GridHelper.SelectById(dgUsers, "UserId", user.UserId);

                MessageBox.Show($"Đã {actionText.ToLower()} tài khoản {user.Username} thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật trạng thái: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =====================================================================
        // Sự kiện khi bấm nút "Đổi vai trò" (btnChangeRole)
        // =====================================================================
        private void btnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            // Bước 1: Lấy tài khoản đang chọn
            User? user = GetSelectedUser();
            if (user == null) return;

            // Bước 2: Lấy vai trò mới từ ComboBox
            string newRole = _roles[cbxNewRole.SelectedIndex];
            if (newRole == user.Role)
            {
                MessageBox.Show("Tài khoản này đã có vai trò " + RoleName(newRole) + ".", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Bước 3: Cập nhật vai trò mới cho User
                user.Role = newRole;

                // Đảm bảo tạo hồ sơ tương ứng trong bảng Coaches hoặc Members nếu chưa có
                EnsureProfile(user);

                _db.SaveChanges(); // Lưu vào Database

                // Bước 4: Ghi nhận Audit Log
                AuditHelper.Log(_db, "SetRole", $"Đổi vai trò {user.Username} thành {RoleName(newRole)}", "Users", user.UserId);

                // Bước 5: Nạp lại dữ liệu hiển thị
                LoadUsers();
                GridHelper.SelectById(dgUsers, "UserId", user.UserId);

                MessageBox.Show($"Đã đổi vai trò của {user.Username} thành {RoleName(newRole)}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đổi vai trò: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =====================================================================
        // Hàm đồng bộ hồ sơ: Nếu User có role Coach thì phải có dòng trong bảng Coaches,
        // nếu có role Member thì phải có dòng trong bảng Members
        // =====================================================================
        private void EnsureProfile(User user)
        {
            // Nếu là Coach và chưa có hồ sơ huấn luyện viên -> Tự động thêm hồ sơ Coach
            if (user.Role == "Coach" && !_db.Coaches.Any(c => c.UserId == user.UserId))
            {
                Coach newCoach = new Coach
                {
                    User = user,
                    FullName = user.FullName,
                    Email = user.Username + "@center.vn",
                    Phone = user.Phone
                };
                _db.Coaches.Add(newCoach);
            }

            // Nếu là Member và chưa có hồ sơ hội viên -> Tự động thêm hồ sơ Member
            if (user.Role == "Member" && !_db.Members.Any(m => m.UserId == user.UserId))
            {
                Member newMember = new Member
                {
                    User = user,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    CreatedAt = DateTime.Now
                };
                _db.Members.Add(newMember);
            }
        }

        // =====================================================================
        // Sự kiện khi bấm nút "Thêm tài khoản mới" (btnAddUser)
        // (Tương tự phương thức btnAddEmp_Click / btnAddCourse_Click trong bài PRN212)
        // =====================================================================
        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            // Bước 1: Lấy dữ liệu từ các ô nhập liệu
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();

            // Bước 2: Kiểm tra dữ liệu rỗng
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập Họ tên và Tên đăng nhập.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Bước 3: Kiểm tra định dạng Họ tên hợp lệ
            string? nameError = AccountHelper.FullNameError(fullName);
            if (nameError != null)
            {
                MessageBox.Show(nameError, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Bước 4: Kiểm tra định dạng Tên đăng nhập hợp lệ
            string? usernameError = AccountHelper.UsernameError(username);
            if (usernameError != null)
            {
                MessageBox.Show(usernameError, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Bước 5: Kiểm tra xem Tên đăng nhập đã tồn tại trong CSDL chưa
            if (_db.Users.Any(u => u.Username == username))
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Bước 6: Thêm tài khoản mới vào CSDL
            try
            {
                // Khởi tạo đối tượng User mới với mật khẩu mặc định "123456"
                User newUser = new User
                {
                    Username = username,
                    PasswordHash = PasswordHelper.Hash("123456"), // Băm SHA-256
                    FullName = fullName,
                    Role = _roles[cbxRole.SelectedIndex],         // Lấy vai trò được chọn từ ComboBox
                    IsActive = true,                              // Mặc định kích hoạt
                    CreatedAt = DateTime.Now
                };

                _db.Users.Add(newUser);
                EnsureProfile(newUser); // Tạo hồ sơ Coach/Member tương ứng nếu có
                _db.SaveChanges();      // Lưu vào Database

                // Ghi lịch sử hoạt động tạo tài khoản
                AuditHelper.Log(_db, "CreateUser", $"Tạo tài khoản {username} ({RoleName(newUser.Role)})", "Users", newUser.UserId);

                // Xóa trắng form nhập liệu sau khi thêm thành công (giống ClearForm trong PRN)
                txtFullName.Clear();
                txtUsername.Clear();

                // Nạp lại danh sách hiển thị
                pgUsers.Reset();
                LoadUsers();

                MessageBox.Show($"Đã tạo tài khoản {username} thành công! Mật khẩu mặc định là 123456.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo tài khoản: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
