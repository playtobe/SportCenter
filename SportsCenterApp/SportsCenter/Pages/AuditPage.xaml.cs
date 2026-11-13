using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for AuditPage.xaml
    /// </summary>
    public partial class AuditPage : UserControl
    {
        private SportsCenterContext _db;

        public AuditPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgAudit);
            pgAudit.PageChanged += (s, e) => LoadLogs();
            LoadData();
        }

        private void LoadData()
        {
            var users = new List<string> { "Tất cả" };
            users.AddRange(_db.AuditLogs.Where(a => a.User != null).Select(a => a.User!.Username).Distinct().OrderBy(u => u).ToList());
            cbxUser.ItemsSource = users;
            cbxUser.SelectedIndex = 0;

            var actions = new List<string> { "Tất cả" };
            // Ô lọc hiện tên tiếng Việt; khi lọc sẽ đổi ngược lại về mã lưu trong DB
            actions.AddRange(_db.AuditLogs.Select(a => a.Action).Distinct().ToList()
                .Select(Rules.ActionName).OrderBy(a => a));
            cbxAction.ItemsSource = actions;
            cbxAction.SelectedIndex = 0;

            LoadLogs();
        }

        private void LoadLogs()
        {
            var query = _db.AuditLogs.AsQueryable();

            string keyword = txtKeyword.Text.Trim();
            if (keyword != "")
            {
                query = query.Where(a => (a.Details ?? "").Contains(keyword) || (a.User != null && a.User.Username.Contains(keyword)));
            }
            if (cbxUser.SelectedItem is string user && user != "Tất cả")
            {
                query = query.Where(a => a.User != null && a.User.Username == user);
            }
            if (cbxAction.SelectedItem is string action && action != "Tất cả")
            {
                string code = Rules.ActionNames.FirstOrDefault(kv => kv.Value == action).Key ?? action;
                query = query.Where(a => a.Action == code);
            }
            if (dpFrom.SelectedDate is DateTime from)
            {
                query = query.Where(a => a.CreatedAt >= from.Date);
            }
            if (dpTo.SelectedDate is DateTime to)
            {
                query = query.Where(a => a.CreatedAt < to.Date.AddDays(1));
            }

            var rows = query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    At = a.CreatedAt,
                    User = a.User != null ? a.User.Username : "-",
                    a.Action,
                    a.Details
                })
                .ToList()
                .Select(r => new { r.At, r.User, Action = Rules.ActionName(r.Action), r.Details })
                .ToList();
            dgAudit.ItemsSource = pgAudit.Slice(rows);
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            pgAudit.Reset();
            LoadLogs();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtKeyword.Clear();
            cbxUser.SelectedIndex = 0;
            cbxAction.SelectedIndex = 0;
            dpFrom.SelectedDate = null;
            dpTo.SelectedDate = null;
            pgAudit.Reset();
            LoadLogs();
        }
    }
}
