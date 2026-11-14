using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using SportsCenter.Entities;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for PaymentsPage.xaml
    /// </summary>
    public partial class PaymentsPage : UserControl
    {
        private SportsCenterContext _db;
        private int? _currentPaymentId;
        private string _invoiceText = "";

        public PaymentsPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgPayments);
            pgPayments.PageChanged += (s, e) => LoadPayments();

            LoadPayments();
        }

        private void LoadPayments()
        {
            var query = _db.Payments.AsQueryable();

            // Member chỉ xem hóa đơn của chính mình
            if (Session.Role == "Member")
            {
                int memberId = Session.CurrentMember?.MemberId ?? 0;
                query = query.Where(p => p.MemberId == memberId);
            }

            string keyword = txtSearch.Text.Trim();
            if (keyword != "")
            {
                query = query.Where(p => p.InvoiceNo.Contains(keyword) || p.Member.FullName.Contains(keyword));
            }
            if (dpFrom.SelectedDate is DateTime from)
            {
                query = query.Where(p => p.PaidAt >= from.Date);
            }
            if (dpTo.SelectedDate is DateTime to)
            {
                query = query.Where(p => p.PaidAt < to.Date.AddDays(1));
            }

            var rows = query
                .OrderByDescending(p => p.PaidAt)
                .Select(p => new
                {
                    p.PaymentId,
                    p.InvoiceNo,
                    Member = p.Member.FullName,
                    p.Amount,
                    p.PaidAt
                })
                .ToList();

            dgPayments.ItemsSource = pgPayments.Slice(rows);
            if (rows.Count > 0) dgPayments.SelectedIndex = 0;
            else ShowInvoice(null);
        }

        private void dgPayments_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ShowInvoice(GridHelper.SelectedId(dgPayments, "PaymentId"));

        private void ShowInvoice(int? paymentId)
        {
            _currentPaymentId = paymentId;
            if (paymentId == null)
            {
                txtInvoice.Text = "Chọn một hóa đơn trong danh sách.";
                _invoiceText = "";
                btnExport.IsEnabled = false;
                return;
            }

            var p = _db.Payments.AsNoTracking()
                .Include(x => x.Member)
                .Include(x => x.Subscription).ThenInclude(s => s!.Package)
                .First(x => x.PaymentId == paymentId);

            var sb = new StringBuilder();
            sb.AppendLine("HÓA ĐƠN " + p.InvoiceNo);
            sb.AppendLine("Member: " + p.Member.FullName);
            sb.AppendLine("Nội dung: " + (p.Subscription?.Package.PackageName ?? p.Note ?? ""));
            if (p.Subscription != null)
                sb.AppendLine($"Hiệu lực: {p.Subscription.StartDate:dd/MM/yyyy} → {p.Subscription.EndDate:dd/MM/yyyy}");
            sb.AppendLine("Phương thức: " + Rules.MethodNames.GetValueOrDefault(p.PaymentMethod, p.PaymentMethod));
            sb.AppendLine($"Số tiền: {p.Amount:N0} ₫");
            sb.Append($"Ngày thu: {p.PaidAt:dd/MM/yyyy HH:mm}");

            _invoiceText = sb.ToString();
            txtInvoice.Text = _invoiceText;
            btnExport.IsEnabled = true;
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            pgPayments.Reset();
            LoadPayments();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            dpFrom.SelectedDate = null;
            dpTo.SelectedDate = null;
            pgPayments.Reset();
            LoadPayments();
        }

        // Xuất hóa đơn ra file .txt
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPaymentId == null) return;

            var dialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt",
                FileName = _invoiceText.Split('\n')[0].Replace("HÓA ĐƠN ", "").Trim()
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                File.WriteAllText(dialog.FileName, _invoiceText, Encoding.UTF8);
                Msg.Info("Đã xuất hóa đơn ra file.");
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }
    }
}
