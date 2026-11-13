using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for CoachPage.xaml
    /// </summary>
    public partial class CoachPage : UserControl
    {
        private SportsCenterContext _db;
        private bool _loading;

        public CoachPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgStudents);
            pgStudents.PageChanged += (s, e) => LoadStudents();
            LoadClasses();
        }

        private void LoadClasses()
        {
            int coachId = Session.CurrentCoach?.CoachId ?? 0;
            var classes = _db.Classes.AsNoTracking()
                .Where(c => c.CoachId == coachId && c.Status != "Cancelled")
                .OrderBy(c => c.ClassCode)
                .Select(c => new { c.ClassId, Text = c.ClassCode + " · " + c.ClassName })
                .ToList();

            _loading = true;
            cbxClass.DisplayMemberPath = "Text";
            cbxClass.SelectedValuePath = "ClassId";
            cbxClass.ItemsSource = classes;
            _loading = false;

            if (classes.Count == 0)
            {
                txtSchedule.Text = txtRoom.Text = txtCapacity.Text = "";
                dgStudents.ItemsSource = null;
                Msg.Info("Bạn chưa phụ trách lớp nào.");
                return;
            }
            cbxClass.SelectedIndex = 0;
        }

        private void cbxClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading) return;
            pgStudents.Reset();
            LoadStudents();
        }

        private void LoadStudents()
        {
            if (cbxClass.SelectedValue is not int classId) return;

            var c = _db.Classes.AsNoTracking()
                .Include(x => x.Room).Include(x => x.Slot)
                .First(x => x.ClassId == classId);

            var members = _db.Registrations.AsNoTracking()
                .Where(r => r.ClassId == classId && r.Status == "Registered")
                .Select(r => r.Member)
                .OrderBy(m => m.FullName)
                .ToList();
            var subs = _db.Subscriptions.AsNoTracking().ToList();

            txtSchedule.Text = Rules.ScheduleText(c);
            txtRoom.Text = c.Room.RoomName;
            txtCapacity.Text = $"{members.Count}/{c.Capacity}";

            var rows = members.Select(m => new
            {
                m.MemberId,
                Student = m.FullName,
                m.Phone,
                PackageStatus = Rules.SubscriptionText(Rules.CurrentOrLatest(subs.Where(s => s.MemberId == m.MemberId)))
            }).ToList();

            dgStudents.ItemsSource = pgStudents.Slice(rows);
        }
    }
}
