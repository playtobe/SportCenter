using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SportsCenter
{
    /// <summary>
    /// Thời khóa biểu tuần của member, giống FAP: mỗi ô lớp có kèm kết quả điểm danh của đúng ngày đó.
    /// Interaction logic for MySchedulePage.xaml
    /// </summary>
    public partial class MySchedulePage : UserControl
    {
        private SportsCenterContext _db;
        private DateTime _weekStart;   // thứ Hai của tuần đang xem
        private bool _loading;

        public MySchedulePage()
        {
            InitializeComponent();
            _db = new();
            _weekStart = MondayOf(DateTime.Today);
            BuildSchedule();
        }

        private static DateTime MondayOf(DateTime d) => d.Date.AddDays(-(Rules.IsoDay(d) - 1));

        private static Brush Hex(string hex) => (Brush)new BrushConverter().ConvertFromString(hex)!;

        private void BuildSchedule()
        {
            var member = Session.CurrentMember;
            if (member == null)
            {
                txtInfo.Text = "Chỉ tài khoản Member mới có lịch cá nhân.";
                return;
            }
            txtInfo.Text = $"Thời khóa biểu của {member.FullName}. Ô trắng là ca chưa có lớp.";

            DateTime weekEnd = _weekStart.AddDays(6);
            txtWeek.Text = $"Tuần ({_weekStart:dd/MM} - {weekEnd:dd/MM})";

            var slots = _db.TimeSlots.AsNoTracking().OrderBy(s => s.SlotId).ToList();

            // Các lớp member từng đăng ký; lớp còn đăng ký thì hiện theo lịch, lớp đã hủy đăng ký chỉ hiện nếu đã có điểm danh
            var classIds = _db.Registrations.Where(r => r.MemberId == member.MemberId).Select(r => r.ClassId).Distinct().ToList();
            var activeIds = _db.Registrations.Where(r => r.MemberId == member.MemberId && r.Status == "Registered")
                .Select(r => r.ClassId).ToHashSet();
            var classes = _db.Classes.Include(c => c.Coach).Include(c => c.Room).AsNoTracking()
                .Where(c => classIds.Contains(c.ClassId)).ToList();

            // Kết quả điểm danh trong tuần: (lớp, ngày) -> Present / Absent
            var from = DateOnly.FromDateTime(_weekStart);
            var to = DateOnly.FromDateTime(weekEnd);
            var attendance = _db.Attendances.AsNoTracking()
                .Where(a => a.Registration.MemberId == member.MemberId && a.SessionDate >= from && a.SessionDate <= to)
                .Select(a => new { a.Registration.ClassId, a.SessionDate, a.Status })
                .ToList()
                .GroupBy(a => (a.ClassId, a.SessionDate))
                .ToDictionary(g => g.Key, g => g.First().Status);

            gridSchedule.Children.Clear();
            gridSchedule.RowDefinitions.Clear();
            gridSchedule.ColumnDefinitions.Clear();

            // Cột 0 là tên ca, cột 1..7 là Thứ Hai..Chủ Nhật
            gridSchedule.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            for (int d = 1; d <= 7; d++)
                gridSchedule.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            gridSchedule.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            foreach (var _ in slots)
                gridSchedule.RowDefinitions.Add(new RowDefinition { MinHeight = 96 });

            // Hàng tiêu đề: tên thứ và ngày, cột hôm nay được tô sáng
            AddCell(0, 0, HeaderText("Ca", null, false), false, false);
            for (int d = 1; d <= 7; d++)
            {
                DateTime day = _weekStart.AddDays(d - 1);
                bool today = day.Date == DateTime.Today;
                AddCell(0, d, HeaderText(Rules.DayNames[d], $"({day:dd/MM})", today), true, today);
            }

            int present = 0, absent = 0, pending = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                var label = new StackPanel();
                label.Children.Add(new TextBlock { Text = slot.SlotName, FontWeight = FontWeights.SemiBold });
                label.Children.Add(new TextBlock
                {
                    Text = $"{slot.StartTime:HH:mm}-{slot.EndTime:HH:mm}",
                    Foreground = (Brush)FindResource("Muted"),
                    FontSize = 12
                });
                AddCell(i + 1, 0, label, false, false);

                for (int d = 1; d <= 7; d++)
                {
                    DateTime day = _weekStart.AddDays(d - 1);
                    var date = DateOnly.FromDateTime(day);

                    var c = classes.FirstOrDefault(x => x.DayOfWeek == d && x.SlotId == slot.SlotId
                        && (attendance.ContainsKey((x.ClassId, date))
                            || (activeIds.Contains(x.ClassId) && x.Status != "Cancelled" && x.StartDate <= date && x.EndDate >= date)));
                    if (c == null)
                    {
                        AddCell(i + 1, d, null, true, day.Date == DateTime.Today);
                        continue;
                    }

                    // Buổi đã qua mà chưa có kết quả thì ghi "Chưa điểm danh"; buổi hôm nay và sau này thì chưa hiện gì
                    attendance.TryGetValue((c.ClassId, date), out string? status);
                    if (status == "Present") present++;
                    else if (status == "Absent") absent++;
                    else if (day.Date < DateTime.Today) pending++;

                    AddCell(i + 1, d, MakeClassCell(c, slot, status, day.Date < DateTime.Today), true, day.Date == DateTime.Today, true);
                }
            }

            txtSummary.Text = present + absent + pending == 0 && !classes.Any(c => activeIds.Contains(c.ClassId))
                ? "Tuần này không có buổi học nào."
                : $"Trong tuần: {present} buổi có mặt, {absent} buổi vắng, {pending} buổi chưa điểm danh.";
        }

        private StackPanel HeaderText(string title, string? sub, bool today)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                Foreground = today ? (Brush)FindResource("Accent") : Brushes.Black
            });
            if (sub != null)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = sub,
                    FontSize = 12,
                    Foreground = today ? (Brush)FindResource("Accent") : (Brush)FindResource("Muted")
                });
            }
            return panel;
        }

        // Nội dung một ô lớp: mã và tên lớp, kết quả điểm danh, giờ học, phòng, huấn luyện viên
        private UIElement MakeClassCell(Class c, TimeSlot slot, string? status, bool past)
        {
            var muted = (Brush)FindResource("Muted");
            var info = new StackPanel();
            info.Children.Add(new TextBlock { Text = c.ClassCode + " · " + c.ClassName, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });

            if (status == "Present") info.Children.Add(MakeBadge("✓ Có mặt", "#17803D", "#D9F1E1"));
            else if (status == "Absent") info.Children.Add(MakeBadge("✗ Vắng", "#B42318", "#FBE0DD"));
            else if (past) info.Children.Add(MakeBadge("Chưa điểm danh", "#5C6B66", "#E8EDEB"));

            info.Children.Add(new TextBlock { Text = $"{slot.StartTime:HH:mm}-{slot.EndTime:HH:mm}", Foreground = muted, FontSize = 12 });
            info.Children.Add(new TextBlock { Text = c.Room.RoomName, Foreground = muted, FontSize = 12, TextWrapping = TextWrapping.Wrap });
            info.Children.Add(new TextBlock { Text = c.Coach.FullName, Foreground = muted, FontSize = 12, TextWrapping = TextWrapping.Wrap });
            return info;
        }

        private static Border MakeBadge(string text, string foreground, string background) =>
            new Border
            {
                Background = Hex(background),
                CornerRadius = new CornerRadius(9),
                Padding = new Thickness(7, 1, 7, 1),
                Margin = new Thickness(0, 3, 0, 3),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock { Text = text, Foreground = Hex(foreground), FontSize = 11, FontWeight = FontWeights.SemiBold }
            };

        // header = ô tiêu đề; today = cột của hôm nay; hasClass = ô có lớp (tô xanh nhạt)
        private void AddCell(int row, int col, UIElement? content, bool body, bool today, bool hasClass = false)
        {
            string background = row == 0 ? (today ? "#D6EFEB" : "#F1F5F4")
                : col == 0 ? "#F1F5F4"
                : hasClass ? "#E3F3F0"
                : today ? "#F3FAF8" : "#FFFFFF";

            var border = new Border
            {
                Background = Hex(background),
                BorderBrush = (Brush)FindResource("Line"),
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(8, 6, 8, 6),
                Child = content
            };
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            gridSchedule.Children.Add(border);
        }

        private void ChangeWeek(DateTime newStart)
        {
            _weekStart = newStart;
            _loading = true;
            dpJump.SelectedDate = null;
            _loading = false;
            BuildSchedule();
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e) => ChangeWeek(_weekStart.AddDays(-7));

        private void btnNext_Click(object sender, RoutedEventArgs e) => ChangeWeek(_weekStart.AddDays(7));

        private void btnToday_Click(object sender, RoutedEventArgs e) => ChangeWeek(MondayOf(DateTime.Today));

        // Chọn một ngày bất kỳ để nhảy tới tuần chứa ngày đó
        private void dpJump_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_loading || dpJump.SelectedDate is not DateTime picked) return;
            _weekStart = MondayOf(picked);
            BuildSchedule();
        }
    }
}
