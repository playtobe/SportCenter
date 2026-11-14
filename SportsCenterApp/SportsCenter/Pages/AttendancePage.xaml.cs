using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    // Một dòng điểm danh: tick = Có mặt, bỏ tick = Vắng.
    // Có INotifyPropertyChanged để nút "Có mặt tất cả / Vắng tất cả" cập nhật được giao diện.
    public class AttendanceRow : INotifyPropertyChanged
    {
        public int RegistrationId { get; set; }

        public string Student { get; set; } = "";

        private bool _present;

        public bool Present
        {
            get => _present;
            set
            {
                if (_present == value) return;
                _present = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Present)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// Interaction logic for AttendancePage.xaml
    /// </summary>
    public partial class AttendancePage : UserControl
    {
        private SportsCenterContext _db;
        private bool _loading;

        // Giữ toàn bộ các dòng để chuyển trang không mất lựa chọn đã bấm
        private List<AttendanceRow> _rows = new();

        public AttendancePage()
        {
            InitializeComponent();
            _db = new();
            pgAttendance.PageChanged += (s, e) => icRows.ItemsSource = pgAttendance.Slice(_rows);
            LoadClasses();
        }

        private void LoadClasses()
        {
            // Chỉ coach điểm danh buổi học, và chỉ lớp của mình. Người không phải coach (kể cả lễ tân) không thấy lớp nào.
            int coachId = Session.CurrentCoach?.CoachId ?? 0;
            var query = _db.Classes.AsNoTracking().Include(c => c.Slot)
                .Where(c => c.Status != "Cancelled" && c.CoachId == coachId);

            var classes = query.OrderBy(c => c.ClassCode).ToList()
                .Select(c => new { c.ClassId, Text = c.ClassCode + " · " + Rules.ScheduleText(c) })
                .ToList();

            _loading = true;
            cbxClass.DisplayMemberPath = "Text";
            cbxClass.SelectedValuePath = "ClassId";
            cbxClass.ItemsSource = classes;
            _loading = false;

            if (classes.Count > 0) cbxClass.SelectedIndex = 0;
            else LoadRows();
        }

        // Chọn lớp xong thì liệt kê các buổi học thật của lớp (mới nhất trước) và chọn sẵn buổi gần nhất,
        // coach không phải tự chọn ngày nên không thể chọn sai thứ
        private void cbxClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading || cbxClass.SelectedValue is not int classId) return;

            var c = _db.Classes.AsNoTracking().First(x => x.ClassId == classId);
            var sessions = Rules.SessionDates(c)
                .Select(d => new
                {
                    Date = d,
                    Text = $"{Rules.DayNames[c.DayOfWeek]} {d:dd/MM/yyyy}" + (d == Rules.Today ? " (hôm nay)" : "")
                })
                .ToList();

            _loading = true;
            cbxSession.DisplayMemberPath = "Text";
            cbxSession.SelectedValuePath = "Date";
            cbxSession.ItemsSource = sessions;
            if (sessions.Count > 0) cbxSession.SelectedIndex = 0;
            _loading = false;

            LoadRows();
        }

        private void cbxSession_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading) return;
            LoadRows();
        }

        private void LoadRows()
        {
            foreach (var old in _rows) old.PropertyChanged -= Row_PropertyChanged;
            _rows = new();

            if (cbxClass.SelectedValue is int classId && cbxSession.SelectedValue is DateOnly date)
            {
                var regs = _db.Registrations.AsNoTracking()
                    .Where(r => r.ClassId == classId && r.Status == "Registered")
                    .Select(r => new { r.RegistrationId, r.Member.FullName })
                    .OrderBy(r => r.FullName)
                    .ToList();
                var saved = _db.Attendances.AsNoTracking()
                    .Where(a => a.SessionDate == date && regs.Select(r => r.RegistrationId).Contains(a.RegistrationId))
                    .ToList();

                // Chưa điểm danh thì mặc định Có mặt, coach chỉ cần bấm vào những bạn vắng
                _rows = regs.Select(r => new AttendanceRow
                {
                    RegistrationId = r.RegistrationId,
                    Student = r.FullName,
                    Present = saved.FirstOrDefault(a => a.RegistrationId == r.RegistrationId)?.Status != "Absent"
                }).ToList();
            }

            foreach (var row in _rows) row.PropertyChanged += Row_PropertyChanged;

            pgAttendance.Reset();
            icRows.ItemsSource = pgAttendance.Slice(_rows);
            UpdateSummary();
        }

        private void Row_PropertyChanged(object? sender, PropertyChangedEventArgs e) => UpdateSummary();

        // Dòng tổng kết "Có mặt 2/3 · Vắng 1" và thông báo khi chưa có gì để điểm danh
        private void UpdateSummary()
        {
            int total = _rows.Count;
            int present = _rows.Count(r => r.Present);
            txtSummary.Text = total == 0 ? "" : $"Có mặt {present}/{total} · Vắng {total - present}";

            if (cbxClass.Items.Count == 0)
                txtEmpty.Text = "Chưa có lớp nào để điểm danh.";
            else if (cbxSession.Items.Count == 0)
                txtEmpty.Text = "Lớp này chưa có buổi học nào để điểm danh (chưa tới ngày bắt đầu).";
            else if (total == 0)
                txtEmpty.Text = "Lớp này chưa có học viên đăng ký.";
            else
                txtEmpty.Text = "";
            txtEmpty.Visibility = txtEmpty.Text == "" ? Visibility.Collapsed : Visibility.Visible;

            btnSave.IsEnabled = btnAllPresent.IsEnabled = btnAllAbsent.IsEnabled = total > 0;
        }

        private void btnAllPresent_Click(object sender, RoutedEventArgs e)
        {
            foreach (var row in _rows) row.Present = true;
        }

        private void btnAllAbsent_Click(object sender, RoutedEventArgs e)
        {
            foreach (var row in _rows) row.Present = false;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbxClass.SelectedValue is not int classId || cbxSession.SelectedValue is not DateOnly date)
            {
                Msg.Warn("Vui lòng chọn lớp và buổi học.");
                return;
            }
            if (_rows.Count == 0)
            {
                Msg.Warn("Lớp này chưa có học viên.");
                return;
            }

            var c = _db.Classes.AsNoTracking().First(x => x.ClassId == classId);
            if (Rules.IsoDay(date) != c.DayOfWeek)
            {
                Msg.Warn($"Ngày này không phải {Rules.DayNames[c.DayOfWeek]}, ngày học của lớp {c.ClassCode}.");
                return;
            }

            try
            {
                foreach (var row in _rows)
                {
                    string status = row.Present ? "Present" : "Absent";
                    var existing = _db.Attendances.FirstOrDefault(a => a.RegistrationId == row.RegistrationId && a.SessionDate == date);
                    if (existing != null)
                    {
                        existing.Status = status;
                        existing.CheckedInBy = Session.CurrentUser?.UserId;
                    }
                    else
                    {
                        _db.Attendances.Add(new Attendance
                        {
                            RegistrationId = row.RegistrationId,
                            SessionDate = date,
                            Status = status,
                            CheckedInBy = Session.CurrentUser?.UserId
                        });
                    }
                }
                _db.SaveChanges();
                AuditHelper.Log(_db, "Attendance", $"Điểm danh {c.ClassCode} ngày {date:dd/MM/yyyy}", "Classes", c.ClassId);
                Msg.Info("Đã lưu điểm danh lớp " + c.ClassCode + ".");
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }
    }
}
