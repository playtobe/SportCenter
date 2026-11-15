using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using SportsCenter.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for CoachPage.xaml
    /// Màn Lớp của tôi (Coach): chọn một lớp mình dạy, xem lịch, phòng, sĩ số và danh sách học viên.
    /// </summary>
    public partial class CoachPage : UserControl
    {
        private SportsCenterContext _db;

        // true trong lúc code tự đổ dữ liệu vào ô Chọn lớp, để sự kiện SelectionChanged bỏ qua
        private bool _loading;

        public CoachPage()
        {
            InitializeComponent();
            _db = new();
            LoadClasses();
        }

        // Đổ các lớp (chưa hủy) của coach đang đăng nhập vào ô Chọn lớp
        private void LoadClasses()
        {
            int coachId = Session.CurrentCoach?.CoachId ?? 0;

            var classes = _db.Classes
                .Where(x => x.CoachId == coachId && x.Status != "Cancelled")
                .OrderBy(x => x.ClassCode)
                .Select(x => new OptionViewModel()
                {
                    Id = x.ClassId,
                    Text = x.ClassCode + " · " + x.ClassName
                }).ToList();

            _loading = true;
            cbxClass.ItemsSource = classes;
            cbxClass.DisplayMemberPath = nameof(OptionViewModel.Text);
            cbxClass.SelectedValuePath = nameof(OptionViewModel.Id);
            _loading = false;

            if (classes.Count == 0)
            {
                txtSchedule.Text = "";
                txtRoom.Text = "";
                txtCapacity.Text = "";
                dgStudents.ItemsSource = null;
                Msg.Info("Bạn chưa phụ trách lớp nào.");
                return;
            }

            // Chọn sẵn lớp đầu tiên -> sự kiện SelectionChanged chạy -> hiện học viên
            cbxClass.SelectedIndex = 0;
        }

        private void cbxClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading) return;
            pgStudents.Reset();
            LoadStudents();
        }

        // Hiện thông tin lớp đang chọn và danh sách học viên đang đăng ký lớp đó
        private void LoadStudents()
        {
            if (cbxClass.SelectedValue == null) return;
            int classId = (int)cbxClass.SelectedValue;

            // 1. Thông tin lớp: lịch, phòng
            var cls = _db.Classes
                .Include(x => x.Room)
                .Include(x => x.Slot)
                .First(x => x.ClassId == classId);

            // 2. Các member đang đăng ký lớp này
            var members = _db.Registrations
                .Where(x => x.ClassId == classId && x.Status == "Registered")
                .Select(x => x.Member)
                .OrderBy(x => x.FullName)
                .ToList();

            // 3. Các gói thành viên (để ghi "Còn hạn" / "Hết hạn" / "Chưa có gói" cho từng học viên)
            var subscriptions = _db.Subscriptions.ToList();

            txtSchedule.Text = Rules.ScheduleText(cls);
            txtRoom.Text = cls.Room.RoomName;
            txtCapacity.Text = $"{members.Count}/{cls.Capacity}";

            // 4. Gắn vào ViewModel rồi đưa lên bảng
            var students = members.Select(m => new StudentViewModel()
            {
                MemberId = m.MemberId,
                FullName = m.FullName,
                Phone = m.Phone,
                PackageStatus = Rules.SubscriptionText(
                    Rules.CurrentOrLatest(subscriptions.Where(s => s.MemberId == m.MemberId)))
            }).ToList();

            dgStudents.ItemsSource = pgStudents.Slice(students);
        }

        // Bấm sang trang khác ở thanh phân trang thì tải lại danh sách học viên
        private void pgStudents_PageChanged(object? sender, EventArgs e)
        {
            LoadStudents();
        }
    }
}
