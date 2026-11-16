using SportsCenter.Entities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for UsersPage.xaml
    /// Màn hình Quản lý tài khoản người dùng (Dành cho Quản lý / Manager
    /// </summary>
    public partial class UsersPage : UserControl
    {
        private SportsCenterContext _db;
        private readonly string[] _roles = { "Manager", "Receptionist", "Coach", "Member" };

        private static string RoleName(string code) => Rules.RoleNames.GetValueOrDefault(code, code);

        public UsersPage()
        {
            InitializeComponent();
            _db = new SportsCenterContext();
            GridHelper.Setup(dgUsers);
            pgUsers.PageChanged += (s, e) => LoadUsers();

            LoadRole();
            LoadUsers();
        }

        public void LoadUsers()
        {
            string keyword = txtSearch.Text.Trim();
            int roleIndex = cbxRoleFilter.SelectedIndex;

            // 1. Cú pháp LINQ truy vấn bảng Users:
            var query = _db.Users.AsQueryable();

            // 2. Lọc theo từ khóa tìm kiếm (Username hoặc FullName) nếu có nhập:
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.Username.Contains(keyword) || u.FullName.Contains(keyword));
            }

            // 3. Lọc theo Role nếu có chọn (khác All / Tất cả):
            if (roleIndex > 0)
            {
                string selectedRole = _roles[roleIndex - 1];
                query = query.Where(u => u.Role == selectedRole);
            }

            // 4. Đổ dữ liệu vào DataGrid:
            var userList = query
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

            dgUsers.ItemsSource = pgUsers.Slice(userList);
        }

        public void LoadRole()
        {
            // Xóa item cũ trước khi nạp
            cbxRoleFilter.Items.Clear();
            cbxRole.Items.Clear();
            cbxNewRole.Items.Clear();

            // 1. Nạp ComboBox Filter: thêm "All" ở đầu tiên
            cbxRoleFilter.Items.Add("All");

            // 2. Dùng foreach duyệt qua từng vai trò để nạp vào các ComboBox
            foreach (var role in _roles)
            {
                string name = RoleName(role);
                cbxRoleFilter.Items.Add(name); // Thêm vào bộ lọc
                cbxRole.Items.Add(name);       // Thêm vào ô Thêm mới
                cbxNewRole.Items.Add(name);    // Thêm vào ô Đổi vai trò
            }

            // Chọn vị trí mặc định
            cbxRoleFilter.SelectedIndex = 0; // Chọn "All"
            cbxRole.SelectedIndex = 1;       // Chọn "Lễ tân"
            cbxNewRole.SelectedIndex = 1;    // Chọn "Lễ tân"
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            pgUsers.Reset();
            LoadUsers();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            // Reset cb về "All"
            txtSearch.Clear();
            cbxRoleFilter.SelectedIndex = 0;
            pgUsers.Reset();
            LoadUsers();
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập Họ tên và Tên đăng nhập.");
                return;
            }

            if (AccountHelper.FullNameError(fullName) is string nameError)
            {
                MessageBox.Show(nameError);
                return;
            }

            if (AccountHelper.UsernameError(username) is string usernameError)
            {
                MessageBox.Show(usernameError);
                return;
            }

            if (_db.Users.Any(u => u.Username == username))
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại.");
                return;
            }

            try
            {
                var user = new User
                {
                    Username = username,
                    PasswordHash = PasswordHelper.Hash("123456"),
                    FullName = fullName,
                    Role = _roles[cbxRole.SelectedIndex],
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _db.Users.Add(user);
                EnsureProfile(user);
                _db.SaveChanges();

                AuditHelper.Log(_db, "CreateUser", $"Tạo tài khoản {username}", "Users", user.UserId);

                // reset lại dg và form
                LoadUsers();
                ClearForm();

                MessageBox.Show("Thêm tài khoản thành công. Mật khẩu mặc định là 123456.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thêm tài khoản thất bại: " + ex.Message);
            }
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null) return;

            try
            {
                user.IsActive = !user.IsActive;
                _db.SaveChanges();

                AuditHelper.Log(_db, "ToggleUser", $"{(user.IsActive ? "Mở khóa" : "Khóa")} tài khoản {user.Username}", "Users", user.UserId);

                // reset lại dg
                LoadUsers();
                GridHelper.SelectById(dgUsers, "UserId", user.UserId);

                MessageBox.Show("Cập nhật trạng thái thành công.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cập nhật trạng thái thất bại: " + ex.Message);
            }
        }

        private void btnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null) return;

            string newRole = _roles[cbxNewRole.SelectedIndex];
            if (newRole == user.Role)
            {
                MessageBox.Show("Tài khoản này đã có vai trò " + RoleName(newRole) + ".");
                return;
            }

            try
            {
                user.Role = newRole;
                EnsureProfile(user);
                _db.SaveChanges();

                AuditHelper.Log(_db, "SetRole", $"Đổi vai trò {user.Username} thành {RoleName(newRole)}", "Users", user.UserId);

                // reset lại dg
                LoadUsers();
                GridHelper.SelectById(dgUsers, "UserId", user.UserId);

                MessageBox.Show("Đổi vai trò thành công.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đổi vai trò thất bại: " + ex.Message);
            }
        }

        private User? GetSelectedUser()
        {
            int? id = GridHelper.SelectedId(dgUsers, "UserId");
            if (id == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản trong danh sách.");
                return null;
            }

            // Luật A3: Không thể tự khóa hoặc đổi quyền chính mình
            if (id == Session.CurrentUser!.UserId)
            {
                MessageBox.Show("Không thể khóa hoặc đổi vai trò của chính tài khoản đang đăng nhập.");
                return null;
            }

            return _db.Users.Find(id);
        }

        private void EnsureProfile(User user)
        {
            if (user.Role == "Coach" && !_db.Coaches.Any(c => c.UserId == user.UserId))
            {
                _db.Coaches.Add(new Coach { User = user, FullName = user.FullName, Email = user.Username + "@center.vn", Phone = user.Phone });
            }
            if (user.Role == "Member" && !_db.Members.Any(m => m.UserId == user.UserId))
            {
                _db.Members.Add(new Member { User = user, FullName = user.FullName, Phone = user.Phone, CreatedAt = DateTime.Now });
            }
        }

        // Tách hàm ClearForm() riêng biệt để tái sử dụng
        private void ClearForm()
        {
            txtFullName.Clear();
            txtUsername.Clear();
            cbxRole.SelectedIndex = 1;
        }
    }
}
