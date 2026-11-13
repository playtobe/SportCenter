using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        private SportsCenterContext _db;

        public SettingsPage()
        {
            InitializeComponent();
            _db = new();

            var u = Session.CurrentUser!;
            txtInfo.Text = $"Họ tên: {u.FullName}\nTên đăng nhập: {u.Username}\nVai trò: {Rules.RoleNames[u.Role]}";
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            errOld.Text = errNew.Text = errConfirm.Text = "";
            string oldPw = txtOld.Password;
            string newPw = txtNew.Password;
            string confirm = txtConfirm.Password;

            var user = _db.Users.Find(Session.CurrentUser!.UserId)!;
            bool ok = true;

            if (oldPw == "")
            {
                errOld.Text = "Vui lòng nhập mật khẩu hiện tại.";
                ok = false;
            }
            else if (user.PasswordHash != PasswordHelper.Hash(oldPw))
            {
                errOld.Text = "Mật khẩu hiện tại chưa đúng.";
                ok = false;
            }

            if (newPw == "")
            {
                errNew.Text = "Vui lòng nhập mật khẩu mới.";
                ok = false;
            }
            else if (newPw.Length < 6)
            {
                errNew.Text = "Mật khẩu cần ít nhất 6 ký tự.";
                ok = false;
            }
            else if (newPw == oldPw)
            {
                errNew.Text = "Mật khẩu mới phải khác mật khẩu hiện tại.";
                ok = false;
            }
            else if (newPw.ToLower() == user.Username.ToLower())
            {
                errNew.Text = "Mật khẩu không được trùng tên đăng nhập.";
                ok = false;
            }

            if (confirm == "")
            {
                errConfirm.Text = "Vui lòng nhập lại mật khẩu mới.";
                ok = false;
            }
            else if (confirm != newPw)
            {
                errConfirm.Text = "Hai mật khẩu chưa khớp.";
                ok = false;
            }

            if (!ok) return;

            try
            {
                user.PasswordHash = PasswordHelper.Hash(newPw);
                _db.SaveChanges();
                AuditHelper.Log(_db, "ChangePassword", $"{user.Username} đổi mật khẩu", "Users", user.UserId);

                txtOld.Clear();
                txtNew.Clear();
                txtConfirm.Clear();
                Msg.Info("Đã đổi mật khẩu. Lần đăng nhập sau dùng mật khẩu mới.");
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }
    }
}
