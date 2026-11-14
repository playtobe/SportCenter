using SportsCenter.Entities;
using System.Windows;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private SportsCenterContext _db;

        public LoginWindow()
        {
            InitializeComponent();
            _db = new();
            txtUsername.Focus();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (username == "" || password == "")
            {
                Msg.Warn("Vui lòng nhập tên đăng nhập và mật khẩu.");
                return;
            }

            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null || user.PasswordHash != PasswordHelper.Hash(password))
                {
                    Msg.Warn("Sai tên đăng nhập hoặc mật khẩu.");
                    return;
                }
                if (!user.IsActive)
                {
                    Msg.Warn("Tài khoản đã bị khóa.");
                    return;
                }

                Session.SignIn(_db, user);
                new MainWindow().Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không kết nối được cơ sở dữ liệu. Hãy kiểm tra appsettings.json và đã chạy SportsCenter.sql chưa.\n\n"
                    + (ex.InnerException?.Message ?? ex.Message), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            new RegisterWindow().Show();
            Close();
        }
    }
}
