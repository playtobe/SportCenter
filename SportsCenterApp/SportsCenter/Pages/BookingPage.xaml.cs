using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for BookingPage.xaml
    /// </summary>
    public partial class BookingPage : UserControl
    {
        private SportsCenterContext _db;

        public BookingPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgClasses);
            pgClasses.PageChanged += (s, e) => LoadClasses();

            var sports = _db.Sports.AsNoTracking().OrderBy(s => s.SportName).ToList();
            sports.Insert(0, new Sport { SportName = "Tất cả" });
            cbxSportFilter.ItemsSource = sports;
            cbxSportFilter.DisplayMemberPath = nameof(Sport.SportName);
            cbxSportFilter.SelectedIndex = 0;

            LoadMemberPicker();
            ShowNote();
            LoadClasses();
        }

        private void LoadMemberPicker()
        {
            if (Session.Role == "Member")
            {
                panelMember.Visibility = Visibility.Collapsed;
                return;
            }

            cbxMember.ItemsSource = _db.Members.AsNoTracking().OrderBy(m => m.MemberId).ToList();
            cbxMember.DisplayMemberPath = nameof(Member.FullName);
            cbxMember.SelectedValuePath = nameof(Member.MemberId);
            cbxMember.SelectedIndex = 0;
        }

        // Member tự đặt cho mình; nhân viên đặt hộ member đang chọn
        private int GetMemberId()
        {
            if (Session.Role == "Member") return Session.CurrentMember?.MemberId ?? 0;
            return cbxMember.SelectedValue as int? ?? 0;
        }

        private void ShowNote()
        {
            int memberId = GetMemberId();
            var member = _db.Members.AsNoTracking().FirstOrDefault(m => m.MemberId == memberId);
            if (member == null)
            {
                txtNote.Text = "Chưa chọn member.";
                return;
            }

            var cur = Rules.ActiveSubscription(_db, memberId);
            txtNote.Text = cur != null
                ? $"{member.FullName} · gói còn hạn đến {cur.EndDate:dd/MM/yyyy}"
                : $"{member.FullName} · chưa có gói còn hạn, chưa đăng ký lớp được";
        }

        private void LoadClasses()
        {
            int memberId = GetMemberId();
            var query = _db.Classes
                .Include(c => c.Sport).Include(c => c.Coach).Include(c => c.Room).Include(c => c.Slot)
                .AsNoTracking()
                .Where(c => c.Status != "Cancelled");

            string keyword = txtSearch.Text.Trim();
            if (keyword != "")
            {
                query = query.Where(c => c.ClassCode.Contains(keyword) || c.ClassName.Contains(keyword));
            }
            if (cbxSportFilter.SelectedItem is Sport sport && sport.SportId != 0)
            {
                query = query.Where(c => c.SportId == sport.SportId);
            }

            var counts = _db.Registrations
                .Where(r => r.Status == "Registered")
                .GroupBy(r => r.ClassId)
                .Select(g => new { ClassId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.ClassId, x => x.Count);
            var mine = _db.Registrations
                .Where(r => r.MemberId == memberId && r.Status == "Registered")
                .Select(r => r.ClassId)
                .ToHashSet();

            var rows = query.OrderBy(c => c.ClassId).ToList().Select(c =>
            {
                int n = counts.GetValueOrDefault(c.ClassId);
                return new
                {
                    c.ClassId,
                    c.ClassCode,
                    c.ClassName,
                    SportName = c.Sport.SportName,
                    CoachName = c.Coach.FullName,
                    RoomName = c.Room.RoomName,
                    Schedule = Rules.ScheduleText(c),
                    Slots = $"{n}/{c.Capacity}" + (n >= c.Capacity ? " · đầy" : ""),
                    MyStatus = mine.Contains(c.ClassId) ? "Đã đăng ký" : ""
                };
            }).ToList();

            dgClasses.ItemsSource = pgClasses.Slice(rows);
        }

        private void cbxMember_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowNote();
            LoadClasses();
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            pgClasses.Reset();
            LoadClasses();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cbxSportFilter.SelectedIndex = 0;
            pgClasses.Reset();
            LoadClasses();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            int memberId = GetMemberId();
            int? classId = GridHelper.SelectedId(dgClasses, "ClassId");
            if (memberId == 0)
            {
                Msg.Warn("Vui lòng chọn member.");
                return;
            }
            if (classId == null)
            {
                Msg.Warn("Vui lòng chọn một lớp trong danh sách.");
                return;
            }

            try
            {
                string? error = Rules.RegisterClass(_db, memberId, classId.Value);
                if (error != null)
                {
                    Msg.Warn(error);
                    return;
                }
                LoadClasses();
                Msg.Info("Đăng ký lớp thành công.");
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            int memberId = GetMemberId();
            int? classId = GridHelper.SelectedId(dgClasses, "ClassId");
            if (memberId == 0 || classId == null)
            {
                Msg.Warn("Vui lòng chọn một lớp trong danh sách.");
                return;
            }

            try
            {
                string? error = Rules.CancelRegistration(_db, memberId, classId.Value);
                if (error != null)
                {
                    Msg.Warn(error);
                    return;
                }
                LoadClasses();
                Msg.Info("Đã hủy đăng ký lớp.");
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }
    }
}
