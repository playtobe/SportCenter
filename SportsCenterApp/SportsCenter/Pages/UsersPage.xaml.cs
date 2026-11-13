using SportsCenter.Entities;
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
        // Mã role lưu trong DB; các ô chọn hiện tên tiếng Việt theo đúng thứ tự này
        private readonly string[] _roles = { "Manager", "Receptionist", "Coach", "Member" };

        private static string RoleName(string code) => Rules.RoleNames.GetValueOrDefault(code, code);

        public UsersPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgUsers);
            pgUsers.PageChanged += (s, e) => LoadUsers();
            LoadData();
        }

        private void LoadData()
        {
            var roleNames = _roles.Select(RoleName).ToList();

            var filterRoles = new List<string> { "Tất cả" };
            filterRoles.AddRange(roleNames);
            cbxRoleFilter.ItemsSource = filterRoles;
            cbxRoleFilter.SelectedIndex = 0;

            cbxNewRole.ItemsSource = roleNames;
            cbxNewRole.SelectedIndex = 3;
            cbxRole.ItemsSource = roleNames;
            cbxRole.SelectedIndex = 1;

            LoadUsers();
        }

        private void LoadUsers()
        {
            var query = _db.Users.AsQueryable();

            string keyword = txtSearch.Text.Trim();
            if (keyword != "")
            {
                query = query.Where(u => u.Username.Contains(keyword) || u.FullName.Contains(keyword));
            }
            if (cbxRoleFilter.SelectedIndex > 0)
            {
                string role = _roles[cbxRoleFilter.SelectedIndex - 1];
                query = query.Where(u => u.Role == role);
            }

            var rows = query
                .OrderBy(u => u.UserId)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.FullName,
                    u.Role,
                    Status = u.IsActive ? "Đang hoạt động" : "Đã khóa"
                })
                .ToList()
                .Select(u => new { u.UserId, u.Username, u.FullName, Role = RoleName(u.Role), u.Status })
                .ToList();
            dgUsers.ItemsSource = pgUsers.Slice(rows);
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            pgUsers.Reset();
            LoadUsers();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cbxRoleFilter.SelectedIndex = 0;
            pgUsers.Reset();
            LoadUsers();
        }

        // Lấy tài khoản đang chọn; không cho thao tác trên chính tài khoản đang đăng nhập
        private User? GetSelectedUser()
        {
            int? id = GridHelper.SelectedId(dgUsers, "UserId");
            if (id == null)
            {
                Msg.Warn("Vui lòng chọn một tài khoản trong danh sách.");
                return null;
            }
            if (id == Session.CurrentUser!.UserId)
            {
                Msg.Warn("Không thể khóa hoặc đổi vai trò của chính tài khoản đang đăng nhập.");
                return null;
            }
            return _db.Users.Find(id);
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
                LoadUsers();
                GridHelper.SelectById(dgUsers, "UserId", user.UserId);
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        private void btnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null) return;

            string newRole = _roles[cbxNewRole.SelectedIndex];
            if (newRole == user.Role)
            {
                Msg.Warn("Tài khoản này đã có vai trò " + RoleName(newRole) + ".");
                return;
            }

            try
            {
                user.Role = newRole;
                EnsureProfile(user);
                _db.SaveChanges();
                AuditHelper.Log(_db, "SetRole", $"Đổi {user.Username} thành {RoleName(newRole)}", "Users", user.UserId);
                LoadUsers();
                GridHelper.SelectById(dgUsers, "UserId", user.UserId);
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        // Role Coach cần có dòng Coaches, role Member cần có dòng Members
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

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            if (fullName == "" || username == "")
            {
                Msg.Warn("Vui lòng nhập Họ tên và Tên đăng nhập.");
                return;
            }
            if (_db.Users.Any(u => u.Username == username))
            {
                Msg.Warn("Tên đăng nhập đã tồn tại.");
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

                txtFullName.Clear();
                txtUsername.Clear();
                pgUsers.Reset();
                LoadUsers();
                Msg.Info($"Đã tạo tài khoản {username}. Mật khẩu mặc định là 123456, hãy nhờ người dùng đổi mật khẩu.");
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }
    }
}
