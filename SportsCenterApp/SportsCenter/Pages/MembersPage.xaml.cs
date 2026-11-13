using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for MembersPage.xaml
    /// </summary>
    public partial class MembersPage : UserControl
    {
        private SportsCenterContext _db;

        public MembersPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgMembers);
            pgMembers.PageChanged += (s, e) => LoadMembers();

            // Bán / gia hạn gói và đăng ký member tại quầy là việc của lễ tân; Manager chỉ xem và tìm danh sách member
            if (Session.Role == "Manager")
            {
                btnBuy.Visibility = Visibility.Collapsed;
                grpAdd.Visibility = Visibility.Collapsed;
            }

            cbxStatus.ItemsSource = new List<string> { "Tất cả", "Còn hạn", "Hết hạn", "Chưa có gói", "Đã hủy" };
            cbxStatus.SelectedIndex = 0;
            LoadMembers();
        }

        private void LoadMembers()
        {
            var query = _db.Members.AsQueryable();
            string keyword = txtSearch.Text.Trim();
            if (keyword != "")
            {
                query = query.Where(m => m.FullName.Contains(keyword)
                    || (m.Phone ?? "").Contains(keyword)
                    || (m.Email ?? "").Contains(keyword));
            }
            var members = query.OrderBy(m => m.MemberId).AsNoTracking().ToList();
            var subs = _db.Subscriptions.Include(s => s.Package).AsNoTracking().ToList();

            var rows = members.Select(m =>
            {
                var s = Rules.CurrentOrLatest(subs.Where(x => x.MemberId == m.MemberId));
                return new
                {
                    m.MemberId,
                    m.FullName,
                    m.Phone,
                    HasAccount = m.UserId != null ? "Có" : "Chưa có",
                    PackageName = s?.Package.PackageName ?? "—",
                    EndDate = s?.EndDate,
                    Status = Rules.SubscriptionText(s)
                };
            }).ToList();

            if (cbxStatus.SelectedItem is string status && status != "Tất cả")
            {
                rows = rows.Where(r => r.Status == status).ToList();
            }

            dgMembers.ItemsSource = pgMembers.Slice(rows);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            pgMembers.Reset();
            LoadMembers();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cbxStatus.SelectedIndex = 0;
            pgMembers.Reset();
            LoadMembers();
        }

        // Chuyển sang màn Mua / gia hạn gói với member đang chọn
        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            int? memberId = GridHelper.SelectedId(dgMembers, "MemberId");
            if (memberId == null)
            {
                Msg.Warn("Vui lòng chọn một member trong danh sách.");
                return;
            }
            MainWindow.Instance?.ShowPage(new BuyPage(memberId));
        }

        private void btnAddMember_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (fullName == "" || phone == "")
            {
                Msg.Warn("Vui lòng nhập Họ tên và Điện thoại.");
                return;
            }
            if (!Regex.IsMatch(phone, @"^0\d{9,10}$"))
            {
                Msg.Warn("Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0.");
                return;
            }
            if (email != "" && !AccountHelper.IsValidEmail(email))
            {
                Msg.Warn("Email chưa đúng định dạng (ví dụ: ten@mail.com). Có thể để trống.");
                return;
            }

            try
            {
                // Cùng họ tên VÀ cùng số điện thoại với hồ sơ đã có = cùng một người: không tạo thêm
                var same = AccountHelper.FindSamePerson(_db, fullName, phone);
                if (same.Count > 0)
                {
                    var old = same[0];
                    Msg.Warn($"Đã có member \"{old.FullName}\" (#{old.MemberId}) cùng họ tên và số điện thoại "
                        + $"({(old.UserId != null ? "đã có tài khoản" : "đăng ký tại quầy")}).\n\n"
                        + "Không tạo thêm hồ sơ trùng, hãy dùng hồ sơ đó trong danh sách.");
                    // Lọc sẵn danh sách theo số điện thoại để thấy ngay hồ sơ cũ
                    txtSearch.Text = phone;
                    cbxStatus.SelectedIndex = 0;
                    pgMembers.Reset();
                    LoadMembers();
                    return;
                }

                // Cùng số điện thoại nhưng khác họ tên: có thể là người nhà dùng chung số, hỏi lại cho chắc
                var sharedPhone = AccountHelper.FindSamePhoneOtherName(_db, fullName, phone);
                if (sharedPhone.Count > 0 && !Msg.Confirm($"Số {phone} đang dùng cho member \"{sharedPhone[0].FullName}\".\n\n"
                    + $"Vẫn tạo hồ sơ mới cho \"{fullName}\"? (chọn Có nếu là người nhà dùng chung số)"))
                {
                    return;
                }

                var member = new Member
                {
                    FullName = fullName,
                    Phone = phone,
                    Email = email == "" ? null : email,
                    CreatedAt = DateTime.Now
                };
                _db.Members.Add(member);
                _db.SaveChanges();
                AuditHelper.Log(_db, "CreateMember", $"Đăng ký member {fullName} tại quầy", "Members", member.MemberId);

                txtFullName.Clear();
                txtPhone.Clear();
                txtEmail.Clear();
                LoadMembers();
                Msg.Info("Đã đăng ký member mới tại quầy.");
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }
    }
}
