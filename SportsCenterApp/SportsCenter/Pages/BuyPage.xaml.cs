using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for BuyPage.xaml
    /// </summary>
    public partial class BuyPage : UserControl
    {
        private SportsCenterContext _db;

        public BuyPage() : this(null)
        {
        }

        // memberId: member được chọn sẵn (khi bấm "Mua / gia hạn gói" từ màn Member)
        public BuyPage(int? memberId)
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgPackages);
            pgPackages.PageChanged += (s, e) => LoadPackages();

            cbxMethod.ItemsSource = Rules.MethodList;
            cbxMethod.DisplayMemberPath = "Name";
            cbxMethod.SelectedValuePath = "Key";
            cbxMethod.SelectedIndex = 0;

            LoadMemberPicker(memberId);
            LoadPackages();
            ShowCurrent();
            ShowPreview();
        }

        private void LoadMemberPicker(int? presetMemberId)
        {
            if (Session.Role == "Member")
            {
                panelMember.Visibility = Visibility.Collapsed;
                return;
            }

            cbxMember.ItemsSource = _db.Members.AsNoTracking().OrderBy(m => m.MemberId).ToList();
            cbxMember.DisplayMemberPath = nameof(Member.FullName);
            cbxMember.SelectedValuePath = nameof(Member.MemberId);
            if (presetMemberId != null)
                cbxMember.SelectedValue = presetMemberId;
            if (cbxMember.SelectedIndex < 0)
                cbxMember.SelectedIndex = 0;
        }

        // Member tự mua cho mình; nhân viên mua cho member đang chọn
        private int GetMemberId()
        {
            if (Session.Role == "Member") return Session.CurrentMember?.MemberId ?? 0;
            return cbxMember.SelectedValue as int? ?? 0;
        }

        private void LoadPackages()
        {
            var rows = _db.MembershipPackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.PackageId)
                .Select(p => new { p.PackageId, p.PackageName, p.DurationDays, p.Price })
                .ToList();
            dgPackages.ItemsSource = pgPackages.Slice(rows);
        }

        private void ShowCurrent()
        {
            int memberId = GetMemberId();
            var member = _db.Members.AsNoTracking().FirstOrDefault(m => m.MemberId == memberId);
            if (member == null)
            {
                txtCurrent.Text = "Chưa chọn member.";
                return;
            }

            var cur = Rules.ActiveSubscription(_db, memberId);
            if (cur != null)
            {
                string pkg = _db.MembershipPackages.Find(cur.PackageId)!.PackageName;
                txtCurrent.Text = $"{member.FullName} · đang có {pkg}, còn hạn đến {cur.EndDate:dd/MM/yyyy}";

                // Gói đã mua nối tiếp: chưa dùng, tự có hiệu lực khi gói hiện tại hết hạn
                var queued = Rules.QueuedSubscriptions(_db, memberId);
                if (queued.Count > 0)
                {
                    txtCurrent.Text += "\nĐã mua nối tiếp: " + string.Join("; ",
                        queued.Select(q => $"{q.Package.PackageName} ({q.StartDate:dd/MM/yyyy} → {q.EndDate:dd/MM/yyyy})"));
                }
                return;
            }

            var last = _db.Subscriptions.AsNoTracking()
                .Where(s => s.MemberId == memberId && s.Status != "Cancelled")
                .OrderByDescending(s => s.EndDate).FirstOrDefault();
            txtCurrent.Text = last != null
                ? $"{member.FullName} · gói gần nhất đã hết hạn ngày {last.EndDate:dd/MM/yyyy}"
                : $"{member.FullName} · chưa từng mua gói";
        }

        // Gói mới bắt đầu từ hôm nay, hoặc nối tiếp sau gói xa nhất chưa hết hạn (xem Rules.NextStartDate)
        private void ShowPreview()
        {
            int? packageId = GridHelper.SelectedId(dgPackages, "PackageId");
            int memberId = GetMemberId();
            if (packageId == null || memberId == 0)
            {
                txtPreview.Text = "Chọn một gói ở trên.";
                btnConfirm.IsEnabled = false;
                return;
            }

            var pkg = _db.MembershipPackages.AsNoTracking().First(p => p.PackageId == packageId);
            var start = Rules.NextStartDate(_db, memberId);
            var end = start.AddDays(pkg.DurationDays - 1);

            txtPreview.Text = $"{pkg.PackageName}: hiệu lực {start:dd/MM/yyyy} → {end:dd/MM/yyyy}"
                + (start > Rules.Today ? " (nối tiếp: dùng sau khi gói hiện có hết hạn)" : "")
                + $"\nSố tiền: {pkg.Price:N0} ₫";
            btnConfirm.IsEnabled = true;
        }

        private void cbxMember_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowCurrent();
            ShowPreview();
        }

        private void dgPackages_SelectionChanged(object sender, SelectionChangedEventArgs e) => ShowPreview();

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            int memberId = GetMemberId();
            int? packageId = GridHelper.SelectedId(dgPackages, "PackageId");
            if (memberId == 0)
            {
                Msg.Warn("Vui lòng chọn member.");
                return;
            }
            if (packageId == null)
            {
                Msg.Warn("Vui lòng chọn một gói.");
                return;
            }

            try
            {
                var pkg = _db.MembershipPackages.Find(packageId)!;
                if (!pkg.IsActive)
                {
                    Msg.Warn("Gói này đã ngừng bán.");
                    return;
                }

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
                    Subscription = sub,
                    Amount = pkg.Price,
                    PaymentMethod = (string)cbxMethod.SelectedValue,
                    PaidAt = DateTime.Now,
                    ReceivedBy = Session.CurrentUser?.UserId,
                    Status = "Paid",
                    Note = "Mua " + pkg.PackageName
                };

                // Một lần SaveChanges = một transaction: tạo gói và hóa đơn cùng lúc, lỗi thì không lưu gì cả
                _db.Subscriptions.Add(sub);
                _db.Payments.Add(pay);
                _db.SaveChanges();

                AuditHelper.Log(_db, "Payment", $"Thu {pay.InvoiceNo} {pay.Amount:N0} ₫", "Payments", pay.PaymentId);
                ShowCurrent();
                ShowPreview();
                Msg.Info($"Đã tạo gói và hóa đơn {pay.InvoiceNo}.");
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }
    }
}
