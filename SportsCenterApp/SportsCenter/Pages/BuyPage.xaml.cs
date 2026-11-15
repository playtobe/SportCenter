using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    public partial class BuyPage : UserControl
    {
        private SportsCenterContext _db;

        // Mở từ menu: chưa chọn sẵn member
        public BuyPage() : this(null)
        {
        }

        // Mở từ nút "Mua / gia hạn gói" của màn Member: member được chọn sẵn
        public BuyPage(int? memberId)
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgPackages);
            LoadData(memberId);
        }

        private void LoadData(int? memberId)
        {
            LoadMethod();
            LoadMember(memberId);
            LoadPackages();
            ShowCurrent();      // hiện sẵn thông tin ban đầu
            ShowPreview();
        }

        // Ô chọn phương thức thanh toán
        private void LoadMethod()
        {
            cbxMethod.ItemsSource = Rules.MethodList;
            cbxMethod.DisplayMemberPath = "Name";      // ô hiện chữ (vd "Tiền mặt")
            cbxMethod.SelectedValuePath = "Key";       // giá trị thật (vd "Cash")
            cbxMethod.SelectedIndex = 0;
        }

        private void LoadMember(int? memberId)
        {
            // Member đăng nhập thì tự mua cho mình, không cần ô chọn member
            if (Session.Role == "Member")
            {
                panelMember.Visibility = Visibility.Collapsed;
                return;
            }

            cbxMember.ItemsSource = _db.Members.OrderBy(m => m.MemberId).ToList();
            cbxMember.DisplayMemberPath = nameof(Member.FullName);
            cbxMember.SelectedValuePath = nameof(Member.MemberId);
            if (memberId != null)
            {
                cbxMember.SelectedValue = memberId;
            }
            if (cbxMember.SelectedIndex < 0)
            {
                cbxMember.SelectedIndex = 0;
            }
        }

        // Đang mua cho ai: Member thì là chính mình, nhân viên thì là người đang chọn (0 = chưa chọn)
        private int GetMemberId()
        {
            if (Session.Role == "Member")
            {
                return Session.CurrentMember?.MemberId ?? 0;
            }
            if (cbxMember.SelectedItem is not Member member)
            {
                return 0;
            }
            return member.MemberId;
        }

        // Chỉ hiện các gói đang bán
        private void LoadPackages()
        {
            dgPackages.ItemsSource = _db.MembershipPackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.PackageId)
                .Select(p => new { p.PackageId, p.PackageName, p.DurationDays, p.Price })
                .ToList();
        }

        // Dòng chữ: member đang có gói gì
        private void ShowCurrent()
        {
            int memberId = GetMemberId();
            var member = _db.Members.FirstOrDefault(m => m.MemberId == memberId);
            if (member == null)
            {
                txtCurrent.Text = "Chưa chọn member.";
                return;
            }

            var current = Rules.ActiveSubscription(_db, memberId);
            if (current == null)
            {
                txtCurrent.Text = member.FullName + " · chưa có gói còn hạn";
                return;
            }

            string pkgName = _db.MembershipPackages.Find(current.PackageId)!.PackageName;
            txtCurrent.Text = member.FullName + " · đang có " + pkgName + ", còn hạn đến " + current.EndDate.ToString("dd/MM/yyyy");

            // Các gói đã mua nối tiếp (chưa tới ngày bắt đầu)
            var queued = Rules.QueuedSubscriptions(_db, memberId);
            if (queued.Count > 0)
            {
                txtCurrent.Text += "\nĐã mua nối tiếp: " + string.Join("; ",
                    queued.Select(q => q.Package.PackageName + " (" + q.StartDate.ToString("dd/MM/yyyy") + " → " + q.EndDate.ToString("dd/MM/yyyy") + ")"));
            }
        }

        // Xem trước: gói đang chọn có hiệu lực từ ngày nào đến ngày nào, bao nhiêu tiền
        private void ShowPreview()
        {
            int memberId = GetMemberId();
            if (GridHelper.SelectedId(dgPackages, "PackageId") is not int packageId || memberId == 0)
            {
                txtPreview.Text = "Chọn một gói ở trên.";
                btnConfirm.IsEnabled = false;
                return;
            }

            var pkg = _db.MembershipPackages.Find(packageId)!;
            var start = Rules.NextStartDate(_db, memberId);
            var end = start.AddDays(pkg.DurationDays - 1);

            txtPreview.Text = pkg.PackageName + ": hiệu lực " + start.ToString("dd/MM/yyyy") + " → " + end.ToString("dd/MM/yyyy");
            if (start > Rules.Today)
            {
                txtPreview.Text += " (nối tiếp: dùng sau khi gói hiện có hết hạn)";
            }
            txtPreview.Text += "\nSố tiền: " + pkg.Price.ToString("N0") + " ₫";
            btnConfirm.IsEnabled = true;
        }

        private void cbxMember_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowCurrent();
            ShowPreview();
        }

        private void dgPackages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowPreview();
        }

        // Xác nhận đã thu tiền: tạo gói của member và hóa đơn
        private void btnConfirmPayment_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra
            int memberId = GetMemberId();
            if (memberId == 0)
            {
                MessageBox.Show("Vui lòng chọn member.");
                return;
            }
            if (GridHelper.SelectedId(dgPackages, "PackageId") is not int packageId)
            {
                MessageBox.Show("Vui lòng chọn một gói.");
                return;
            }
            if (cbxMethod.SelectedValue is not string method)
            {
                MessageBox.Show("Vui lòng chọn phương thức thanh toán.");
                return;
            }

            try
            {
                var pkg = _db.MembershipPackages.Find(packageId)!;
                if (!pkg.IsActive)
                {
                    MessageBox.Show("Gói này đã ngừng bán.");
                    return;
                }

                // 2. Tạo gói (bắt đầu hôm nay, hoặc nối tiếp sau gói hiện có) và hóa đơn
                var start = Rules.NextStartDate(_db, memberId);
                var sub = new Subscription
                {
                    MemberId = memberId,
                    PackageId = pkg.PackageId,
                    StartDate = start,
                    EndDate = start.AddDays(pkg.DurationDays - 1),
                    Status = "Active"
                };
                var pay = new Payment
                {
                    InvoiceNo = Rules.NextInvoiceNo(_db),
                    MemberId = memberId,
                    Subscription = sub,                     // gắn hóa đơn với gói; EF tự điền SubscriptionId khi lưu
                    Amount = pkg.Price,
                    PaymentMethod = method,
                    PaidAt = DateTime.Now,
                    ReceivedBy = Session.CurrentUser?.UserId,
                    Status = "Paid",
                    Note = "Mua " + pkg.PackageName
                };

                // Một lần SaveChanges lưu cả hai: lỗi thì không lưu gì cả
                _db.Subscriptions.Add(sub);
                _db.Payments.Add(pay);
                _db.SaveChanges();

                AuditHelper.Log(_db, "Payment", "Thu " + pay.InvoiceNo + " " + pay.Amount.ToString("N0") + " ₫", "Payments", pay.PaymentId);

                // 3. Làm mới màn hình
                ShowCurrent();
                ShowPreview();
                MessageBox.Show("Đã tạo gói và hóa đơn " + pay.InvoiceNo + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
