using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : UserControl
    {
        private SportsCenterContext _db;
        private readonly CultureInfo _vi = new("vi-VN");

        public DashboardPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgFill);
            GridHelper.Setup(dgExpiring);
            pgFill.PageChanged += (s, e) => LoadFill();
            pgExpiring.PageChanged += (s, e) => LoadExpiring();

            dpFrom.SelectedDate = DateTime.Today.AddDays(-180);
            dpTo.SelectedDate = DateTime.Today;
            LoadReport();
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (dpFrom.SelectedDate == null || dpTo.SelectedDate == null || dpFrom.SelectedDate > dpTo.SelectedDate)
            {
                Msg.Warn("Khoảng ngày không hợp lệ.");
                return;
            }
            pgFill.Reset();
            pgExpiring.Reset();
            LoadReport();
        }

        private void LoadReport()
        {
            DateTime from = dpFrom.SelectedDate!.Value.Date;
            DateTime to = dpTo.SelectedDate!.Value.Date.AddDays(1);
            var today = Rules.Today;

            // Doanh thu chỉ tính các hóa đơn có Status = Paid
            var paid = _db.Payments.Where(p => p.Status == "Paid" && p.PaidAt >= from && p.PaidAt < to)
                .Select(p => new { p.PaidAt, p.Amount }).ToList();

            txtMembers.Text = _db.Members.Count().ToString();
            txtActiveSubs.Text = _db.Subscriptions
                .Count(s => s.Status != "Cancelled" && s.StartDate <= today && s.EndDate >= today).ToString();
            txtRevenue.Text = paid.Sum(p => p.Amount).ToString("N0", _vi) + " ₫";
            txtRegistrations.Text = _db.Registrations.Count(r => r.Status == "Registered").ToString();

            // 6 tháng gần nhất tính đến tháng của "Đến ngày"; tháng chưa có doanh thu thì để trống
            var months = new List<(string Label, decimal Value)>();
            var last = new DateTime(dpTo.SelectedDate!.Value.Year, dpTo.SelectedDate!.Value.Month, 1);
            for (int i = 5; i >= 0; i--)
            {
                var m = last.AddMonths(-i);
                decimal value = paid.Where(p => p.PaidAt.Year == m.Year && p.PaidAt.Month == m.Month).Sum(p => p.Amount);
                months.Add(("T" + m.Month, value));
            }
            DrawChart(months);

            LoadFill();
            LoadExpiring();
        }

        // Vẽ biểu đồ cột bằng Canvas: cột mảnh, có đường kẻ và nhãn trục, tháng gần nhất tô đậm
        private void DrawChart(List<(string Label, decimal Value)> months)
        {
            canvasChart.Children.Clear();

            const double left = 52, baseline = 190, plotHeight = 150, slot = 82, barWidth = 34;
            decimal max = Math.Max(1, months.Max(m => m.Value));
            decimal[] steps = { 500_000, 1_000_000, 2_000_000, 5_000_000, 10_000_000, 20_000_000, 50_000_000 };
            decimal step = steps.FirstOrDefault(s => Math.Ceiling(max / s) <= 5);
            if (step == 0) step = 100_000_000;
            int n = (int)Math.Ceiling(max / step);
            decimal top = n * step;

            var lineBrush = (Brush)FindResource("Line");
            var mutedBrush = (Brush)FindResource("Muted");
            var accentBrush = (Brush)FindResource("Accent");

            // Đường kẻ ngang và nhãn trục (đơn vị triệu đồng)
            for (int i = 0; i <= n; i++)
            {
                double y = baseline - plotHeight * i / n;
                canvasChart.Children.Add(new Line
                {
                    X1 = left, X2 = left + slot * 6, Y1 = y, Y2 = y,
                    Stroke = lineBrush,
                    StrokeDashArray = i == 0 ? null : new DoubleCollection { 3, 4 }
                });
                AddText(i == 0 ? "0" : FormatMillions(i * step), 0, y - 8, mutedBrush, 11, left - 8, TextAlignment.Right);
            }

            for (int i = 0; i < months.Count; i++)
            {
                double x = left + i * slot + (slot - barWidth) / 2;
                bool current = i == months.Count - 1;
                decimal value = months[i].Value;

                if (value > 0)
                {
                    double h = (double)(value / top) * plotHeight;
                    canvasChart.Children.Add(CreateBar(x, baseline - h, barWidth, h, accentBrush, current ? 1 : 0.45));
                    AddText(FormatMillions(value), x - 15, baseline - h - 20, Brushes.Black, 11, barWidth + 30, TextAlignment.Center, true);
                }
                AddText(months[i].Label, x - 15, baseline + 6, current ? Brushes.Black : mutedBrush, 12, barWidth + 30, TextAlignment.Center);
            }
        }

        private static Rectangle CreateBar(double x, double y, double w, double h, Brush fill, double opacity)
        {
            var bar = new Rectangle { Width = w, Height = h, Fill = fill, Opacity = opacity, RadiusX = 4, RadiusY = 4 };
            Canvas.SetLeft(bar, x);
            Canvas.SetTop(bar, y);
            return bar;
        }

        private string FormatMillions(decimal value) =>
            (value / 1_000_000m).ToString("0.##", _vi) + " tr";

        private void AddText(string text, double x, double y, Brush brush, double size, double width, TextAlignment align, bool bold = false)
        {
            var tb = new TextBlock
            {
                Text = text,
                Foreground = brush,
                FontSize = size,
                Width = width,
                TextAlignment = align,
                FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal
            };
            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);
            canvasChart.Children.Add(tb);
        }

        private void LoadFill()
        {
            var rows = _db.Classes
                .Where(c => c.Status == "Open")
                .OrderBy(c => c.ClassCode)
                .Select(c => new
                {
                    c.ClassCode,
                    c.ClassName,
                    Count = c.Registrations.Count(r => r.Status == "Registered"),
                    c.Capacity
                })
                .ToList()
                .Select(c => new
                {
                    c.ClassCode,
                    c.ClassName,
                    Registered = $"{c.Count}/{c.Capacity}",
                    FillPercent = (c.Capacity == 0 ? 0 : c.Count * 100 / c.Capacity) + "%"
                })
                .ToList();
            dgFill.ItemsSource = pgFill.Slice(rows);
        }

        private void LoadExpiring()
        {
            var today = Rules.Today;
            var limit = today.AddDays(7);
            var rows = _db.Subscriptions
                .Where(s => s.Status != "Cancelled" && s.StartDate <= today && s.EndDate >= today && s.EndDate <= limit)
                .OrderBy(s => s.EndDate)
                .Select(s => new { Member = s.Member.FullName, s.Package.PackageName, s.EndDate })
                .ToList();
            dgExpiring.ItemsSource = pgExpiring.Slice(rows);
        }
    }
}
