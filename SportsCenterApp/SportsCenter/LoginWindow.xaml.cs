using SportsCenter.Entities;
using System;
using System.Linq;
using System.Windows;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// Màn hình Đăng nhập hệ thống (Login)
    /// </summary>
    public partial class LoginWindow : Window
    {
        // 1. Khởi tạo DbContext kết nối Database (tương tự như trong bài thi PRN)
        private SportsCenterContext _db;

        public LoginWindow()
        {
            InitializeComponent();
            _db = new SportsCenterContext(); // Khởi tạo kết nối DB
            txtUsername.Focus();             // Đặt con trỏ chuột vào ô Tên đăng nhập khi vừa mở
        }

        // =====================================================================
        // Sự kiện khi người dùng bấm nút "Đăng nhập" (btnLogin)
        // =====================================================================
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Bước 1: Lấy chuỗi Tên đăng nhập và Mật khẩu từ giao diện
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            // Bước 2: Kiểm tra dữ liệu rỗng (Validation)
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Bước 3: Tìm tài khoản trong Database khớp với Username vừa nhập
                User? user = _db.Users.FirstOrDefault(u => u.Username == username);

                // Bước 4: Kiểm tra nếu tài khoản không tồn tại HOẶC mật khẩu băm không trùng khớp
                string hashedPassword = PasswordHelper.Hash(password);
                if (user == null || user.PasswordHash != hashedPassword)
                {
                    MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Bước 5: Kiểm tra nếu tài khoản đã bị Quản lý khóa (IsActive == false)
                if (!user.IsActive)
                {
                    MessageBox.Show("Tài khoản đã bị khóa. Vui lòng liên hệ ban quản lý.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Bước 6: Đăng nhập thành công -> Lưu thông tin phiên làm việc vào Session
                Session.SignIn(_db, user);

                // Bước 7: Mở màn hình chính (MainWindow) và đóng màn hình Đăng nhập
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                // Xử lý khi không kết nối được cơ sở dữ liệu
                string errorMsg = "Không kết nối được cơ sở dữ liệu. Hãy kiểm tra file appsettings.json và cơ sở dữ liệu SQL Server.\n\n"
                                  + (ex.InnerException?.Message ?? ex.Message);
                MessageBox.Show(errorMsg, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =====================================================================
        // Sự kiện khi người dùng bấm nút "Đăng ký tài khoản" (btnRegister)
        // =====================================================================
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Mở cửa sổ Đăng ký mới (RegisterWindow)
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();

            // Đóng cửa sổ Đăng nhập hiện tại
            this.Close();
        }
    }
}
