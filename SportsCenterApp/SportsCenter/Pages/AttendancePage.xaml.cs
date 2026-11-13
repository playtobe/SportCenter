using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    // Một dòng điểm danh: tick = Có mặt, bỏ tick = Vắng
    public class AttendanceRow
    {
        public int RegistrationId { get; set; }

        public string Student { get; set; } = "";

        public bool Present { get; set; }
    }

    /// <summary>
    /// Interaction logic for AttendancePage.xaml
    /// </summary>
    public partial class AttendancePage : UserControl
    {
        private SportsCenterContext _db;
        private bool _loading;

        // Giữ toàn bộ các dòng để chuyển trang không mất lựa chọn đã tick
        private List<AttendanceRow> _rows = new();

        public AttendancePage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgAttendance, editable: true);
            pgAttendance.PageChanged += (s, e) => dgAttendance.ItemsSource = pgAttendance.Slice(_rows);
            LoadClasses();
        }

        private void LoadClasses()
        {
            // Coach chỉ điểm danh lớp của mình; lễ tân điểm danh mọi lớp
            var query = _db.Classes.AsNoTracking().Include(c => c.Slot).Where(c => c.Status != "Cancelled");
            if (Session.Role == "Coach")
            {
                int coachId = Session.CurrentCoach?.CoachId ?? 0;
                query = query.Where(c => c.CoachId == coachId);
            }

            var classes = query.OrderBy(c => c.ClassCode).ToList()
                .Select(c => new { c.ClassId, Text = c.ClassCode + " · " + Rules.ScheduleText(c) })
                .ToList();

            _loading = true;
            cbxClass.DisplayMemberPath = "Text";
            cbxClass.SelectedValuePath = "ClassId";
            cbxClass.ItemsSource = classes;
            _loading = false;

            if (classes.Count > 0) cbxClass.SelectedIndex = 0;
        }

        // Ngày mặc định: buổi học gần nhất (không muộn hơn hôm nay) của lớp đang chọn
        private void cbxClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading || cbxClass.SelectedValue is not int classId) return;

            var c = _db.Classes.AsNoTracking().First(x => x.ClassId == classId);
            var date = DateTime.Today;
            while (Rules.IsoDay(date) != c.DayOfWeek) date = date.AddDays(-1);

            _loading = true;
            dpDate.SelectedDate = date;
            _loading = false;

            LoadRows();
        }

        private void dpDate_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_loading) return;
            LoadRows();
        }

        private void LoadRows()
        {
            if (cbxClass.SelectedValue is not int classId || dpDate.SelectedDate is not DateTime day) return;
            var date = DateOnly.FromDateTime(day);

            var regs = _db.Registrations.AsNoTracking()
                .Where(r => r.ClassId == classId && r.Status == "Registered")
                .Select(r => new { r.RegistrationId, r.Member.FullName })
                .OrderBy(r => r.FullName)
                .ToList();
            var saved = _db.Attendances.AsNoTracking()
                .Where(a => a.SessionDate == date && regs.Select(r => r.RegistrationId).Contains(a.RegistrationId))
                .ToList();

            // Chưa điểm danh thì mặc định tick Có mặt
            _rows = regs.Select(r => new AttendanceRow
            {
                RegistrationId = r.RegistrationId,
                Student = r.FullName,
                Present = saved.FirstOrDefault(a => a.RegistrationId == r.RegistrationId)?.Status != "Absent"
            }).ToList();

            pgAttendance.Reset();
            dgAttendance.ItemsSource = pgAttendance.Slice(_rows);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbxClass.SelectedValue is not int classId || dpDate.SelectedDate is not DateTime day)
            {
                Msg.Warn("Vui lòng chọn lớp và ngày.");
                return;
            }
            if (_rows.Count == 0)
            {
                Msg.Warn("Lớp này chưa có học viên.");
                return;
            }

            var c = _db.Classes.AsNoTracking().First(x => x.ClassId == classId);
            if (Rules.IsoDay(day) != c.DayOfWeek)
            {
                Msg.Warn($"Ngày này không phải {Rules.DayNames[c.DayOfWeek]}, ngày học của lớp {c.ClassCode}.");
                return;
            }

            try
            {
                var date = DateOnly.FromDateTime(day);
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
