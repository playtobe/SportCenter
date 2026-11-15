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
    /// </summary>
    public partial class UsersPage : UserControl
    {
        private SportsCenterContext _db;

        // Danh sách vai trò trong hệ thống
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
                query = query.Where(x => x.Username.Contains(keyword) || x.FullName.Contains(keyword));
            }

            // 3. Lọc theo Role nếu có chọn (khác All / Tất cả):
            if (roleIndex > 0)
            {
                string selectedRole = _roles[roleIndex - 1];
                query = query.Where(x => x.Role == selectedRole);
            }

            // 4. Đổ dữ liệu vào DataGrid:
            var rows = query
                .OrderBy(x => x.UserId)
                .Select(x => new
                {
                    x.UserId,
                    x.Username,
                    x.FullName,
                    Role = RoleName(x.Role),
                    Status = x.IsActive ? "Đang hoạt động" : "Đã khóa"
                })
                .ToList();

            dgUsers.ItemsSource = pgUsers.Slice(rows);
        }

        public void LoadRole()
        {
            var roleNames = _roles.Select(RoleName).ToList();

            // Danh sách cho phần Add New User và Change Role (không chứa "All")
            cbxRole.ItemsSource = roleNames;
            cbxRole.SelectedIndex = 1;

            cbxNewRole.ItemsSource = roleNames;
            cbxNewRole.SelectedIndex = 3;

            // Danh sách cho phần Filter: có "All" ở đầu (danh sách riêng)
            var filterRoles = new List<string> { "All" };
            filterRoles.AddRange(roleNames);
            cbxRoleFilter.ItemsSource = filterRoles;
            cbxRoleFilter.SelectedIndex = 0;
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
                MessageBox.Show("Please fill in Full Name and Username.");
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
                MessageBox.Show("Username already exists.");
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

                AuditHelper.Log(_db, "CreateUser", $"Tạo tài khoản {username} ({RoleName(user.Role)})", "Users", user.UserId);

                // reset lại dg và form
                LoadUsers();
                ClearForm();

                MessageBox.Show("User added successfully. Mật khẩu mặc định là 123456.");
            }
            catch (Exception)
            {
                MessageBox.Show("User added failed.");
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

                MessageBox.Show("User status updated successfully.");
            }
            catch (Exception)
            {
                MessageBox.Show("User status update failed.");
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

                AuditHelper.Log(_db, "SetRole", $"Đổi {user.Username} thành {RoleName(newRole)}", "Users", user.UserId);

                // reset lại dg
                LoadUsers();
                GridHelper.SelectById(dgUsers, "UserId", user.UserId);

                MessageBox.Show("User role changed successfully.");
            }
            catch (Exception)
            {
                MessageBox.Show("User role change failed.");
            }
        }

        private User? GetSelectedUser()
        {
            int? id = GridHelper.SelectedId(dgUsers, "UserId");
            if (id == null)
            {
                MessageBox.Show("Please select a User in the list.");
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

        private void ClearForm()
        {
            txtFullName.Clear();
            txtUsername.Clear();
            cbxRole.SelectedIndex = 1;
        }
    }
}
