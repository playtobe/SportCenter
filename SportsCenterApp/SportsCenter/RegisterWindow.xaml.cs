using SportsCenter.Entities;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// Màn hình Đăng ký tài khoản hội viên mới (Member)
    /// </summary>
    public partial class RegisterWindow : Window
    {
        // 1. Khởi tạo đối tượng DbContext để làm việc với CSDL (chuẩn PRN212)
        private SportsCenterContext _db;

        public RegisterWindow()
        {
            InitializeComponent();
            _db = new SportsCenterContext(); // Khởi tạo kết nối DB
        }

        // =====================================================================
        // Hàm xóa sạch các dòng chữ báo lỗi màu đỏ trên form trước khi kiểm tra
        // =====================================================================
        private void ClearErrors()
        {
            errFullName.Text = string.Empty;
            errPhone.Text = string.Empty;
            errUsername.Text = string.Empty;
            errPassword.Text = string.Empty;
            errConfirm.Text = string.Empty;
        }

        // =====================================================================
        // Sự kiện khi người dùng nhấn nút "Tạo tài khoản" (btnCreate)
        // =====================================================================
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            // Bước 1: Xóa thông báo lỗi cũ và lấy dữ liệu người dùng nhập
            ClearErrors();
            string fullName = txtFullName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;
            string confirm = txtConfirm.Password;

            // Biến cờ theo dõi xem toàn bộ dữ liệu có hợp lệ không
            bool isValid = true;
            var phoneCheck = (Result: AccountHelper.PhoneCheck.Free, Profile: (Member?)null);

            // Bước 2: Kiểm tra Họ và tên (FullName)
            if (string.IsNullOrEmpty(fullName))
            {
                errFullName.Text = "Vui lòng nhập họ tên.";
                isValid = false;
            }
            else
            {
                string? nameError = AccountHelper.FullNameError(fullName);
                if (nameError != null)
                {
                    errFullName.Text = nameError;
                    isValid = false;
                }
            }

            // Bước 3: Kiểm tra Số điện thoại (Phone)
            if (string.IsNullOrEmpty(phone))
            {
                errPhone.Text = "Vui lòng nhập số điện thoại.";
                isValid = false;
            }
            else if (!Regex.IsMatch(phone, @"^0\d{9,10}$"))
            {
                errPhone.Text = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0.";
                isValid = false;
            }
            else
            {
                // Quy tắc A5: Mỗi SĐT chỉ thuộc về một hội viên duy nhất
                phoneCheck = AccountHelper.CheckPhoneForRegistration(_db, fullName, phone);
                if (phoneCheck.Result == AccountHelper.PhoneCheck.AlreadyRegistered)
                {
                    errPhone.Text = "Số điện thoại này đã được đăng ký. Hãy đăng nhập nếu là số của bạn.";
                    isValid = false;
                }
            }

            // Bước 4: Kiểm tra Tên đăng nhập (Username)
            if (string.IsNullOrEmpty(username))
            {
                errUsername.Text = "Vui lòng nhập tên đăng nhập.";
                isValid = false;
            }
            else
            {
                string? usernameError = AccountHelper.UsernameError(username);
                if (usernameError != null)
                {
                    errUsername.Text = usernameError;
                    isValid = false;
                }
                else if (_db.Users.Any(u => u.Username == username))
                {
                    // Kiểm tra trùng tên đăng nhập trong Database
                    errUsername.Text = "Tên đăng nhập này đã có người dùng.";
                    isValid = false;
                }
            }

            // Bước 5: Kiểm tra Mật khẩu (Password)
            if (string.IsNullOrEmpty(password))
            {
                errPassword.Text = "Vui lòng nhập mật khẩu.";
                isValid = false;
            }
            else if (password.Length < 6)
            {
                errPassword.Text = "Mật khẩu cần ít nhất 6 ký tự.";
                isValid = false;
            }
            else if (password.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                errPassword.Text = "Mật khẩu không được trùng tên đăng nhập.";
                isValid = false;
            }

            // Bước 6: Kiểm tra Xác nhận mật khẩu (Confirm Password)
            if (string.IsNullOrEmpty(confirm))
            {
                errConfirm.Text = "Vui lòng nhập lại mật khẩu.";
                isValid = false;
            }
            else if (confirm != password)
            {
                errConfirm.Text = "Hai mật khẩu chưa khớp.";
                isValid = false;
            }

            // Nếu có bất kỳ trường dữ liệu nào sai thì dừng lại không xử lý tiếp
            if (!isValid)
            {
                return;
            }

            // Bước 7: Thực hiện lưu tài khoản vào Database
            try
            {
                Member? linkProfile = null;

                // Quy tắc A2: Nếu SĐT trùng với hồ sơ tạo tại quầy (chưa có tài khoản) và khớp họ tên
                if (phoneCheck.Result == AccountHelper.PhoneCheck.LinkCounterProfile)
                {
                    Member counterMember = phoneCheck.Profile!;
                    string confirmPrompt = $"Đã có hồ sơ tại quầy của {counterMember.FullName} (SĐT: {counterMember.Phone}).\n\n"
                                         + "Bạn có muốn gắn tài khoản mới này vào hồ sơ cũ để giữ lại các gói tập và lịch sử không?";
                    
                    MessageBoxResult answer = MessageBox.Show(confirmPrompt, "Xác nhận liên kết", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (answer != MessageBoxResult.Yes)
                    {
                        errPhone.Text = "Số điện thoại này đã có hồ sơ tại quầy. Hãy gắn tài khoản hoặc dùng số khác.";
                        return;
                    }
                    linkProfile = counterMember;
                }

                // Gọi helper tạo User vai trò Member (và liên kết hồ sơ quầy nếu có)
                User createdUser = AccountHelper.CreateMemberAccount(_db, username, password, fullName, phone, linkProfile);

                // Tự động đăng nhập người dùng vừa tạo
                Session.SignIn(_db, createdUser);

                // Ghi nhận Audit Log nếu có thao tác liên kết tài khoản
                if (linkProfile != null)
                {
                    AuditHelper.Log(_db, "LinkMember", $"Tài khoản {username} gắn vào hồ sơ quầy #{linkProfile.MemberId}", "Members", linkProfile.MemberId);
                }

                // Thông báo thành công và chuyển sang màn hình chính
                string successNote = linkProfile != null ? " Tài khoản đã được gắn vào hồ sơ tại quầy của bạn." : "";
                MessageBox.Show("Tạo tài khoản thành công!" + successNote);

                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tạo tài khoản thất bại: " + ex.Message);
            }
        }

        // =====================================================================
        // Sự kiện khi người dùng bấm nút "Đã có tài khoản? Đăng nhập" (btnBack)
        // =====================================================================
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
