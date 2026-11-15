using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    public partial class MembersPage : UserControl
    {
        private SportsCenterContext _db;

        public MembersPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgMembers);

            // Manager chỉ xem: ẩn nút mua gói và khung đăng ký member
            if (Session.Role == "Manager")
            {
                btnBuy.Visibility = Visibility.Collapsed;
                grpAdd.Visibility = Visibility.Collapsed;
            }

            LoadData();
        }

        private void LoadData()
        {
            LoadStatus();
            LoadMembers();
        }

        // Ô lọc trạng thái gói
        private void LoadStatus()
        {
            cbxStatus.ItemsSource = new List<string> { "Tất cả", "Còn hạn", "Hết hạn", "Chưa có gói", "Đã hủy" };
            cbxStatus.SelectedIndex = 0;
        }

        // Hiện danh sách member (có lọc theo từ khóa và trạng thái gói)
        private void LoadMembers()
        {
            // 1. Lọc theo từ khóa
            var query = _db.Members.AsQueryable();
            string keyword = txtSearch.Text.Trim();
            if (keyword != "")
            {
                query = query.Where(m => m.FullName.Contains(keyword)
                    || (m.Phone ?? "").Contains(keyword)
                    || (m.Email ?? "").Contains(keyword));
            }
            var members = query.OrderBy(m => m.MemberId).ToList();
            var subs = _db.Subscriptions.Include(s => s.Package).ToList();

            // 2. Với mỗi member: lấy gói hiện tại (hoặc gần nhất) để hiện trạng thái
            var rows = members.Select(m =>
            {
                var sub = Rules.CurrentOrLatest(subs.Where(x => x.MemberId == m.MemberId));
                return new
                {
                    m.MemberId,
                    m.FullName,
                    m.Phone,
                    HasAccount = m.UserId != null ? "Có" : "Chưa có",
                    PackageName = sub?.Package.PackageName ?? "—",
                    EndDate = sub?.EndDate,
                    Status = Rules.SubscriptionText(sub)
                };
            }).ToList();

            // 3. Lọc theo trạng thái (phải lọc sau vì trạng thái do code tính, không phải cột trong DB)
            if (cbxStatus.SelectedItem is string status && status != "Tất cả")
            {
                rows = rows.Where(r => r.Status == status).ToList();
            }

            dgMembers.ItemsSource = rows;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadMembers();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cbxStatus.SelectedIndex = 0;
            LoadMembers();
        }

        // Chuyển sang màn Mua / gia hạn gói với member đang chọn
        private void btnBuyPackage_Click(object sender, RoutedEventArgs e)
        {
            if (GridHelper.SelectedId(dgMembers, "MemberId") is not int memberId)
            {
                MessageBox.Show("Vui lòng chọn một member trong danh sách.");
                return;
            }
            MainWindow.Instance?.ShowPage(new BuyPage(memberId));
        }

        // Đăng ký member tại quầy
        private void btnAddMember_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra dữ liệu nhập
            string fullName = txtFullName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (fullName == "" || phone == "")
            {
                MessageBox.Show("Vui lòng nhập Họ tên và Điện thoại.");
                return;
            }
            string? nameError = AccountHelper.FullNameError(fullName);
            if (nameError != null)
            {
                MessageBox.Show(nameError);
                return;
            }
            if (!Regex.IsMatch(phone, @"^0\d{9,10}$"))
            {
                MessageBox.Show("Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0.");
                return;
            }
            if (email != "" && !AccountHelper.IsValidEmail(email))
            {
                MessageBox.Show("Email chưa đúng định dạng (ví dụ: ten@mail.com). Có thể để trống.");
                return;
            }

            try
            {
                // 2. Mỗi số điện thoại chỉ thuộc về một member
                var owner = _db.Members.FirstOrDefault(m => m.Phone == phone);
                if (owner != null)
                {
                    string loai = owner.UserId != null ? "đã có tài khoản" : "đăng ký tại quầy";
                    MessageBox.Show("Số điện thoại " + phone + " đã thuộc về member \"" + owner.FullName + "\" (#" + owner.MemberId + ", " + loai + ").\n\n"
                        + "Mỗi số điện thoại chỉ dùng cho một member, hãy dùng hồ sơ đó trong danh sách.");
                    txtSearch.Text = phone;     // lọc sẵn danh sách để thấy hồ sơ cũ
                    cbxStatus.SelectedIndex = 0;
                    LoadMembers();
                    return;
                }

                // 3. Thêm member
                var member = new Member
                {
                    FullName = fullName,
                    Phone = phone,
                    Email = email == "" ? null : email,
                    CreatedAt = DateTime.Now
                };
                _db.Members.Add(member);
                _db.SaveChanges();
                AuditHelper.Log(_db, "CreateMember", "Đăng ký member " + fullName + " tại quầy", "Members", member.MemberId);

                LoadMembers();
                ClearForm();
                MessageBox.Show("Đã đăng ký member mới tại quầy.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
            }
        }

        // Xóa các ô nhập của form đăng ký member
        private void ClearForm()
        {
            txtFullName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
        }
    }
}
