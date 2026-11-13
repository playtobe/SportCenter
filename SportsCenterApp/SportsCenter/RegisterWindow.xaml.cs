using SportsCenter.Entities;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private SportsCenterContext _db;

        public RegisterWindow()
        {
            InitializeComponent();
            _db = new();
            txtFullName.Focus();
        }

        private void ClearErrors()
        {
            errFullName.Text = errPhone.Text = errUsername.Text = errPassword.Text = errConfirm.Text = "";
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            ClearErrors();
            string fullName = txtFullName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;
            string confirm = txtConfirm.Password;
            bool ok = true;

            if (fullName == "")
            {
                errFullName.Text = "Vui lòng nhập họ tên.";
                ok = false;
            }

            if (phone == "")
            {
                errPhone.Text = "Vui lòng nhập số điện thoại.";
                ok = false;
            }
            else if (!Regex.IsMatch(phone, @"^0\d{9,10}$"))
            {
                errPhone.Text = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0.";
                ok = false;
            }
            else if (AccountHelper.FindSamePerson(_db, fullName, phone).Any(m => m.UserId != null))
            {
                // Cùng họ tên + số điện thoại với một member đã có tài khoản: không tạo thêm hồ sơ trùng
                errPhone.Text = "Họ tên và số điện thoại này đã có tài khoản. Hãy đăng nhập, hoặc nhờ lễ tân nếu quên mật khẩu.";
                ok = false;
            }

            if (username == "")
            {
                errUsername.Text = "Vui lòng nhập tên đăng nhập.";
                ok = false;
            }
            else if (_db.Users.Any(u => u.Username == username))
            {
                errUsername.Text = "Tên đăng nhập này đã có người dùng.";
                ok = false;
            }

            if (password == "")
            {
                errPassword.Text = "Vui lòng nhập mật khẩu.";
                ok = false;
            }
            else if (password.Length < 6)
            {
                errPassword.Text = "Mật khẩu cần ít nhất 6 ký tự.";
                ok = false;
            }
            else if (password.ToLower() == username.ToLower())
            {
                errPassword.Text = "Mật khẩu không được trùng tên đăng nhập.";
                ok = false;
            }

            if (confirm == "")
            {
                errConfirm.Text = "Vui lòng nhập lại mật khẩu.";
                ok = false;
            }
            else if (confirm != password)
            {
                errConfirm.Text = "Hai mật khẩu chưa khớp.";
                ok = false;
            }

            if (!ok) return;

            try
            {
                // Có hồ sơ đăng ký tại quầy (chưa có tài khoản) khớp CẢ số điện thoại LẪN họ tên không?
                // Chỉ đề nghị gắn khi có đúng MỘT hồ sơ khớp; nhiều hơn một thì không tự gắn để lễ tân kiểm tra.
                var matches = AccountHelper.FindCounterProfiles(_db, fullName, phone);
                Member? link = null;
                if (matches.Count == 1)
                {
                    var m = matches[0];
                    if (Msg.Confirm($"Đã có hồ sơ tại quầy của {m.FullName} (SĐT {m.Phone}, tạo ngày {m.CreatedAt:dd/MM/yyyy}).\n\n"
                        + "Gắn tài khoản mới vào hồ sơ đó để xem lại gói, hóa đơn và lớp đã đăng ký?"))
                    {
                        link = m;
                    }
                }

                // Tạo User (role Member); gắn vào hồ sơ quầy nếu khách đồng ý, không thì tạo hồ sơ Member mới
                var user = AccountHelper.CreateMemberAccount(_db, username, password, fullName, phone, link);
                Session.SignIn(_db, user);
                if (link != null)
                {
                    AuditHelper.Log(_db, "LinkMember",
                        $"Tài khoản {username} gắn vào hồ sơ quầy {link.FullName} (#{link.MemberId})", "Members", link.MemberId);
                }

                string note = link != null ? " Tài khoản đã được gắn vào hồ sơ tại quầy của bạn."
                    : matches.Count > 1 ? " Có nhiều hồ sơ tại quầy trùng thông tin nên chưa tự gắn, hãy nhờ lễ tân kiểm tra."
                    : "";
                Msg.Info("Tạo tài khoản thành công." + note);
                new MainWindow().Show();
                Close();
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }
    }
}
