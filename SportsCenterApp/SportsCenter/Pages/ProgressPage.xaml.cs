using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using SportsCenter.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Màn Số buổi học.
    /// Member: các lớp mình đã đăng ký, đã học (có mặt), nghỉ (vắng), chưa điểm danh và tổng số buổi.
    /// Coach: chọn lớp mình dạy, xem từng học viên đã học và nghỉ bao nhiêu buổi.
    /// </summary>
    public partial class ProgressPage : UserControl
    {
        private SportsCenterContext _db;

        public ProgressPage()
        {
            InitializeComponent();
            _db = new();
            LoadData();
        }

        private void LoadData()
        {
            if (Session.Role == "Member")
            {
                boxCoach.Visibility = Visibility.Collapsed;
                LoadMine();
            }
            else
            {
                txtTitle.Text = "Số buổi học của học viên";
                boxMine.Visibility = Visibility.Collapsed;
                LoadClasses();
            }
        }

        // Member: các lớp mình đang đăng ký, kèm số buổi
        private void LoadMine()
        {
            int memberId = Session.CurrentMember?.MemberId ?? 0;

            var registrations = _db.Registrations
                .Include(r => r.Class).ThenInclude(c => c.Sport)
                .Include(r => r.Attendances)
                .Where(r => r.MemberId == memberId && r.Status == "Registered" && r.Class.Status != "Cancelled")
                .OrderBy(r => r.Class.ClassCode)
                .ToList();

            var rows = new List<MyProgressViewModel>();
            foreach (var r in registrations)
            {
                var p = Rules.ProgressOf(r);
                rows.Add(new MyProgressViewModel
                {
                    ClassId = r.ClassId,
                    ClassName = r.Class.ClassCode + " · " + r.Class.ClassName,
                    SportName = r.Class.Sport.SportName,
                    Total = p.Total,
                    Present = p.Present,
                    Absent = p.Absent,
                    NotMarked = p.NotMarked,
                    Remaining = p.Remaining
                });
            }

            dgMine.ItemsSource = rows;
            txtMineNote.Text = rows.Count == 0 ? "Bạn chưa đăng ký lớp nào." : "";
        }

        // Coach: đổ các lớp (chưa hủy) mình dạy vào ô Chọn lớp
        private void LoadClasses()
        {
            int coachId = Session.CurrentCoach?.CoachId ?? 0;

            var classes = _db.Classes
                .Where(c => c.CoachId == coachId && c.Status != "Cancelled")
                .OrderBy(c => c.ClassCode)
                .Select(c => new OptionViewModel
                {
                    Id = c.ClassId,
                    Text = c.ClassCode + " · " + c.ClassName
                }).ToList();

            cbxClass.ItemsSource = classes;
            cbxClass.DisplayMemberPath = nameof(OptionViewModel.Text);
            cbxClass.SelectedValuePath = nameof(OptionViewModel.Id);

            if (classes.Count == 0)
            {
                txtSummary.Text = "Bạn chưa phụ trách lớp nào.";
                dgStudents.ItemsSource = null;
                return;
            }

            // Chọn sẵn lớp đầu tiên -> SelectionChanged chạy -> hiện học viên
            cbxClass.SelectedIndex = 0;
        }

        private void cbxClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStudents();
        }

        // Coach: từng học viên đang đăng ký lớp đã chọn, kèm số buổi
        private void LoadStudents()
        {
            if (cbxClass.SelectedValue is not int classId)
            {
                return;
            }

            var cls = _db.Classes.First(c => c.ClassId == classId);
            var registrations = _db.Registrations
                .Include(r => r.Member)
                .Include(r => r.Class)
                .Include(r => r.Attendances)
                .Where(r => r.ClassId == classId && r.Status == "Registered")
                .OrderBy(r => r.Member.FullName)
                .ToList();

            var rows = new List<StudentProgressViewModel>();
            foreach (var r in registrations)
            {
                var p = Rules.ProgressOf(r);
                rows.Add(new StudentProgressViewModel
                {
                    MemberId = r.MemberId,
                    FullName = r.Member.FullName,
                    Phone = r.Member.Phone,
                    Total = p.Total,
                    Present = p.Present,
                    Absent = p.Absent,
                    NotMarked = p.NotMarked
                });
            }

            int held = Rules.CountSessions(cls, null, Rules.Today);
            txtSummary.Text = rows.Count + " học viên · tổng " + Rules.CountSessions(cls) + " buổi · đã diễn ra " + held + " buổi";
            dgStudents.ItemsSource = rows;
        }
    }
}
