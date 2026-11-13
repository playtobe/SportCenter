using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for ClassesPage.xaml
    /// </summary>
    public partial class ClassesPage : UserControl
    {
        private SportsCenterContext _db;
        private bool _loading;

        public ClassesPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgClassList);
            pgClass.PageChanged += (s, e) => LoadClassList();
            LoadData();
        }

        private void LoadData()
        {
            _loading = true;
            LoadFilters();
            LoadFormCombos();
            dpStart.SelectedDate = DateTime.Today;
            dpEnd.SelectedDate = DateTime.Today.AddDays(90);
            _loading = false;

            RefreshFreeDays();
            LoadClassList();
        }

        private void LoadFilters()
        {
            var sports = _db.Sports.AsNoTracking().OrderBy(s => s.SportName).ToList();
            var filterSports = sports.ToList();
            filterSports.Insert(0, new Sport { SportName = "Tất cả" });
            cbxSportFilter.ItemsSource = filterSports;
            cbxSportFilter.DisplayMemberPath = nameof(Sport.SportName);
            cbxSportFilter.SelectedIndex = 0;

            var coaches = _db.Coaches.AsNoTracking().OrderBy(c => c.FullName).ToList();
            var filterCoaches = coaches.ToList();
            filterCoaches.Insert(0, new Coach { FullName = "Tất cả" });
            cbxCoachFilter.ItemsSource = filterCoaches;
            cbxCoachFilter.DisplayMemberPath = nameof(Coach.FullName);
            cbxCoachFilter.SelectedIndex = 0;
        }

        private void LoadFormCombos()
        {
            cbxSport.ItemsSource = _db.Sports.AsNoTracking().OrderBy(s => s.SportName).ToList();
            cbxSport.DisplayMemberPath = nameof(Sport.SportName);
            cbxSport.SelectedValuePath = nameof(Sport.SportId);
            cbxSport.SelectedIndex = -1;

            cbxCoach.ItemsSource = _db.Coaches.AsNoTracking().OrderBy(c => c.FullName).ToList();
            cbxCoach.DisplayMemberPath = nameof(Coach.FullName);
            cbxCoach.SelectedValuePath = nameof(Coach.CoachId);
            cbxCoach.SelectedIndex = 0;

            cbxRoom.ItemsSource = _db.Rooms.AsNoTracking().OrderBy(r => r.RoomName).ToList();
            cbxRoom.DisplayMemberPath = nameof(Room.RoomName);
            cbxRoom.SelectedValuePath = nameof(Room.RoomId);
            cbxRoom.SelectedIndex = 0;
        }

        private void LoadClassList()
        {
            var query = _db.Classes.AsQueryable();

            if (cbxSportFilter.SelectedItem is Sport sport && sport.SportId != 0)
            {
                query = query.Where(c => c.SportId == sport.SportId);
            }
            if (cbxCoachFilter.SelectedItem is Coach coach && coach.CoachId != 0)
            {
                query = query.Where(c => c.CoachId == coach.CoachId);
            }

            var rows = query
                .OrderBy(c => c.ClassId)
                .Select(c => new
                {
                    c.ClassId,
                    c.ClassCode,
                    c.ClassName,
                    SportName = c.Sport.SportName,
                    CoachName = c.Coach.FullName,
                    RoomName = c.Room.RoomName,
                    c.DayOfWeek,
                    SlotName = c.Slot.SlotName,
                    c.Slot.StartTime,
                    c.Slot.EndTime,
                    Registered = c.Registrations.Count(r => r.Status == "Registered"),
                    c.Capacity,
                    c.Status
                })
                .ToList()
                .Select(c => new
                {
                    c.ClassId,
                    c.ClassCode,
                    c.ClassName,
                    c.SportName,
                    c.CoachName,
                    c.RoomName,
                    Schedule = Rules.ScheduleText(c.DayOfWeek, c.SlotName, c.StartTime, c.EndTime),
                    Slots = $"{c.Registered}/{c.Capacity}",
                    Status = c.Status == "Open" ? "Mở" : "Đã hủy"
                })
                .ToList();

            dgClassList.ItemsSource = pgClass.Slice(rows);
        }

        // Thứ còn trống = thứ có ít nhất một ca chưa bị chiếm bởi lớp dùng cùng phòng hoặc cùng HLV
        private void RefreshFreeDays()
        {
            if (cbxCoach.SelectedValue is not int coachId || cbxRoom.SelectedValue is not int roomId) return;

            var busy = Rules.BusySet(_db, roomId, coachId);
            var slots = _db.TimeSlots.AsNoTracking().ToList();
            var days = Enumerable.Range(1, 7)
                .Where(d => slots.Any(s => !busy.Contains((d, s.SlotId))))
                .Select(d => new { Day = d, Name = Rules.DayNames[d] })
                .ToList();

            int? previous = cbxDay.SelectedValue as int?;

            _loading = true;
            cbxDay.DisplayMemberPath = "Name";
            cbxDay.SelectedValuePath = "Day";
            cbxDay.ItemsSource = days;
            if (days.Count > 0)
            {
                cbxDay.SelectedValue = (previous != null && days.Any(d => d.Day == previous)) ? previous : days[0].Day;
            }
            _loading = false;

            RefreshFreeSlots();

            bool none = days.Count == 0;
            cbxDay.IsEnabled = cbxSlot.IsEnabled = btnAddClass.IsEnabled = !none;
            txtNoSlot.Visibility = none ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RefreshFreeSlots()
        {
            if (cbxCoach.SelectedValue is not int coachId || cbxRoom.SelectedValue is not int roomId || cbxDay.SelectedValue is not int day)
            {
                cbxSlot.ItemsSource = null;
                return;
            }

            var busy = Rules.BusySet(_db, roomId, coachId);
            var slots = _db.TimeSlots.AsNoTracking().OrderBy(s => s.SlotId).ToList()
                .Where(s => !busy.Contains((day, s.SlotId)))
                .Select(s => new { s.SlotId, Text = $"{s.SlotName} ({s.StartTime:HH:mm}-{s.EndTime:HH:mm})" })
                .ToList();

            cbxSlot.DisplayMemberPath = "Text";
            cbxSlot.SelectedValuePath = "SlotId";
            cbxSlot.ItemsSource = slots;
            if (slots.Count > 0) cbxSlot.SelectedIndex = 0;
        }

        private void cbxCoachRoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading) return;
            RefreshFreeDays();
        }

        private void cbxDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading) return;
            RefreshFreeSlots();
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            pgClass.Reset();
            LoadClassList();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            cbxSportFilter.SelectedIndex = 0;
            cbxCoachFilter.SelectedIndex = 0;
            pgClass.Reset();
            LoadClassList();
        }

        private void btnCancelClass_Click(object sender, RoutedEventArgs e)
        {
            int? classId = GridHelper.SelectedId(dgClassList, "ClassId");
            if (classId == null)
            {
                Msg.Warn("Vui lòng chọn một lớp trong danh sách.");
                return;
            }

            var cl = _db.Classes.Find(classId);
            if (cl == null) return;
            if (cl.Status == "Cancelled")
            {
                Msg.Warn("Lớp này đã hủy rồi.");
                return;
            }
            // C4: lớp còn học viên đăng ký thì không cho hủy
            string? blocked = Rules.CancelClassBlockedReason(_db, cl);
            if (blocked != null)
            {
                Msg.Warn(blocked);
                return;
            }
            if (!Msg.Confirm($"Hủy lớp {cl.ClassCode}? Ca học của lớp sẽ được trả lại thành ca trống."))
            {
                return;
            }

            try
            {
                cl.Status = "Cancelled";
                _db.SaveChanges();
                AuditHelper.Log(_db, "CancelClass", $"Hủy lớp {cl.ClassCode}", "Classes", cl.ClassId);
                LoadClassList();
                RefreshFreeDays();
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        private void btnAddClass_Click(object sender, RoutedEventArgs e)
        {
            string className = txtClassName.Text.Trim();
            string classCode = txtClassCode.Text.Trim().ToUpper();

            if (className == "" || classCode == "" || txtCapacity.Text.Trim() == "")
            {
                Msg.Warn("Vui lòng nhập Tên lớp, Mã lớp và Sĩ số.");
                return;
            }
            if (!int.TryParse(txtCapacity.Text.Trim(), out int capacity) || capacity <= 0)
            {
                Msg.Warn("Sĩ số phải là số lớn hơn 0.");
                return;
            }
            if (cbxSport.SelectedValue is not int sportId)
            {
                Msg.Warn("Vui lòng chọn Bộ môn.");
                return;
            }
            if (cbxCoach.SelectedValue is not int coachId)
            {
                Msg.Warn("Vui lòng chọn Huấn luyện viên.");
                return;
            }
            if (cbxRoom.SelectedValue is not int roomId)
            {
                Msg.Warn("Vui lòng chọn Phòng.");
                return;
            }
            if (cbxDay.SelectedValue is not int day || cbxSlot.SelectedValue is not int slotId)
            {
                Msg.Warn("Vui lòng chọn Thứ và Ca học.");
                return;
            }
            if (dpStart.SelectedDate is not DateTime start || dpEnd.SelectedDate is not DateTime end)
            {
                Msg.Warn("Vui lòng chọn Từ ngày và Đến ngày.");
                return;
            }
            if (end < start)
            {
                Msg.Warn("Ngày kết thúc không được trước ngày bắt đầu.");
                return;
            }

            var room = _db.Rooms.Find(roomId)!;
            if (capacity > room.Capacity)
            {
                Msg.Warn($"Sĩ số không được vượt sức chứa của phòng ({room.Capacity}).");
                return;
            }
            if (_db.Classes.Any(c => c.ClassCode == classCode))
            {
                Msg.Warn("Mã lớp đã tồn tại.");
                return;
            }

            // Kiểm tra lại lần cuối (phòng khi có người khác vừa thêm lớp cùng ca)
            string? conflict = Rules.ClassConflictMessage(_db, roomId, coachId, day, slotId);
            if (conflict != null)
            {
                Msg.Warn(conflict);
                RefreshFreeDays();
                return;
            }

            try
            {
                var cl = new Class
                {
                    ClassCode = classCode,
                    ClassName = className,
                    SportId = sportId,
                    CoachId = coachId,
                    RoomId = roomId,
                    DayOfWeek = (byte)day,
                    SlotId = slotId,
                    Capacity = capacity,
                    StartDate = DateOnly.FromDateTime(start),
                    EndDate = DateOnly.FromDateTime(end),
                    Status = "Open"
                };
                _db.Classes.Add(cl);
                _db.SaveChanges();
                AuditHelper.Log(_db, "CreateClass", $"Tạo lớp {classCode}", "Classes", cl.ClassId);

                pgClass.Reset();
                LoadClassList();
                ClearForm();
                RefreshFreeDays();
                Msg.Info("Đã thêm lớp " + classCode + ".");
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        private void ClearForm()
        {
            txtClassName.Clear();
            txtClassCode.Clear();
            txtCapacity.Text = "15";
            cbxSport.SelectedIndex = -1;
            dpStart.SelectedDate = DateTime.Today;
            dpEnd.SelectedDate = DateTime.Today.AddDays(90);
        }

        private void btnClearForm_Click(object sender, RoutedEventArgs e) => ClearForm();
    }
}
