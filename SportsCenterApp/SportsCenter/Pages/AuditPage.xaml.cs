using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    public partial class AuditPage : UserControl
    {
        private SportsCenterContext _db;

        public AuditPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgAudit);
            LoadData();
        }

        private void LoadData()
        {
            LoadUsers();
            LoadActions();
            LoadLogs();
        }

        // Ô chọn người dùng (có mục "Tất cả")
        private void LoadUsers()
        {
            var users = _db.Users.OrderBy(u => u.Username).Select(u => u.Username).ToList();
            users.Insert(0, "Tất cả");
            cbxUser.ItemsSource = users;
            cbxUser.SelectedIndex = 0;
        }

        // Ô chọn hành động (có mục "Tất cả")
        private void LoadActions()
        {
            // DB lưu mã hành động (vd "Payment"), ô chọn hiện tên tiếng Việt (vd "Thanh toán")
            var actions = new List<string>();
            foreach (string code in _db.AuditLogs.Select(a => a.Action).Distinct().ToList())
            {
                actions.Add(Rules.ActionName(code));
            }
            actions.Sort();
            actions.Insert(0, "Tất cả");
            cbxAction.ItemsSource = actions;
            cbxAction.SelectedIndex = 0;
        }

        // Hiện lịch sử hoạt động (mới nhất ở trên), có lọc
        private void LoadLogs()
        {
            var query = _db.AuditLogs.Include(a => a.User).AsQueryable();

            string keyword = txtKeyword.Text.Trim();
            if (keyword != "")
            {
                query = query.Where(a => (a.Details ?? "").Contains(keyword)
                    || (a.User != null && a.User.Username.Contains(keyword)));
            }
            if (cbxUser.SelectedItem is string user && user != "Tất cả")
            {
                query = query.Where(a => a.User != null && a.User.Username == user);
            }
            if (dpFrom.SelectedDate is DateTime from)
            {
                query = query.Where(a => a.CreatedAt >= from.Date);
            }
            if (dpTo.SelectedDate is DateTime to)
            {
                // "Đến ngày 15" nghĩa là lấy hết ngày 15, nên so với 0 giờ ngày 16
                query = query.Where(a => a.CreatedAt < to.Date.AddDays(1));
            }

            var rows = query.OrderByDescending(a => a.CreatedAt).ToList()
                .Select(a => new
                {
                    At = a.CreatedAt,
                    User = a.User != null ? a.User.Username : "-",
                    Action = Rules.ActionName(a.Action),
                    a.Details
                }).ToList();

            // Lọc theo hành động: so sánh tên tiếng Việt, nên lọc sau khi đã đổi tên
            if (cbxAction.SelectedItem is string action && action != "Tất cả")
            {
                rows = rows.Where(r => r.Action == action).ToList();
            }

            dgAudit.ItemsSource = rows;
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtKeyword.Clear();
            cbxUser.SelectedIndex = 0;
            cbxAction.SelectedIndex = 0;
            dpFrom.SelectedDate = null;
            dpTo.SelectedDate = null;
            LoadLogs();
        }
    }
}
