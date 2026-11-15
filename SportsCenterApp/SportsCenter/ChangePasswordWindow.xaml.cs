using SportsCenter.Entities;
using System;
using System.Windows;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// Màn hình Đổi mật khẩu của người dùng đang đăng nhập
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        // 1. Khởi tạo đối tượng DbContext để làm việc với CSDL (chuẩn PRN212)
        private SportsCenterContext _db;

        public ChangePasswordWindow()
        {
            InitializeComponent();
            _db = new SportsCenterContext(); // Khởi tạo kết nối DB

            // Lấy thông tin người dùng hiện tại từ Session để hiển thị lên thanh thông tin
            User currentUser = Session.CurrentUser!;
            string roleDisplayName = Rules.RoleNames.ContainsKey(currentUser.Role) ? Rules.RoleNames[currentUser.Role] : currentUser.Role;
            txtInfo.Text = $"{currentUser.FullName} · {currentUser.Username} · {roleDisplayName}";
        }

        // =====================================================================
        // Sự kiện khi người dùng bấm nút "Lưu thay đổi" / "Đổi mật khẩu" (btnChange)
        // =====================================================================
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            // Bước 1: Xóa thông báo lỗi cũ
            errOld.Text = string.Empty;
            errNew.Text = string.Empty;
            errConfirm.Text = string.Empty;

            // Bước 2: Lấy dữ liệu các ô mật khẩu
            string oldPassword = txtOld.Password;
            string newPassword = txtNew.Password;
            string confirmPassword = txtConfirm.Password;

            // Bước 3: Tìm tài khoản trong Database để cập nhật
            User? user = _db.Users.Find(Session.CurrentUser!.UserId);
            if (user == null)
            {
                MessageBox.Show("Không tìm thấy thông tin tài khoản.");
                return;
            }

            bool isValid = true;

            // Bước 4: Kiểm tra Mật khẩu hiện tại (Old Password)
            if (string.IsNullOrEmpty(oldPassword))
            {
                errOld.Text = "Vui lòng nhập mật khẩu hiện tại.";
                isValid = false;
            }
            else if (user.PasswordHash != PasswordHelper.Hash(oldPassword))
            {
                errOld.Text = "Mật khẩu hiện tại chưa đúng.";
                isValid = false;
            }

            // Bước 5: Kiểm tra Mật khẩu mới (New Password)
            if (string.IsNullOrEmpty(newPassword))
            {
                errNew.Text = "Vui lòng nhập mật khẩu mới.";
                isValid = false;
            }
            else if (newPassword.Length < 6)
            {
                errNew.Text = "Mật khẩu cần ít nhất 6 ký tự.";
                isValid = false;
            }
            else if (newPassword == oldPassword)
            {
                errNew.Text = "Mật khẩu mới phải khác mật khẩu hiện tại.";
                isValid = false;
            }
            else if (newPassword.Equals(user.Username, StringComparison.OrdinalIgnoreCase))
            {
                errNew.Text = "Mật khẩu không được trùng tên đăng nhập.";
                isValid = false;
            }

            // Bước 6: Kiểm tra Xác nhận mật khẩu mới (Confirm Password)
            if (string.IsNullOrEmpty(confirmPassword))
            {
                errConfirm.Text = "Vui lòng nhập lại mật khẩu mới.";
                isValid = false;
            }
            else if (confirmPassword != newPassword)
            {
                errConfirm.Text = "Hai mật khẩu chưa khớp.";
                isValid = false;
            }

            // Nếu có lỗi thì dừng lại
            if (!isValid)
            {
                return;
            }

            // Bước 7: Thực hiện cập nhật mật khẩu mới vào Database
            try
            {
                // Băm mật khẩu mới bằng SHA-256 trước khi lưu
                user.PasswordHash = PasswordHelper.Hash(newPassword);
                _db.SaveChanges(); // Lưu vào SQL Server

                // Ghi lại lịch sử hoạt động đổi mật khẩu
                AuditHelper.Log(_db, "ChangePassword", $"{user.Username} đổi mật khẩu", "Users", user.UserId);

                MessageBox.Show("Đã đổi mật khẩu thành công. Lần đăng nhập sau hãy dùng mật khẩu mới.");
                this.Close(); // Đóng cửa sổ đổi mật khẩu
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đổi mật khẩu thất bại: " + ex.Message);
            }
        }
    }
}
