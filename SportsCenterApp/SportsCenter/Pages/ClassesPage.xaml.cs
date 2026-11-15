using SportsCenter.Entities;
using SportsCenter.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for ClassesPage.xaml
    /// Màn Quản lý lớp học (Manager): xem / lọc danh sách lớp, thêm lớp, hủy lớp.
    /// Luật: C1 (dữ liệu hợp lệ), C2 (trùng phòng), C3 (trùng HLV), C4 (hủy lớp).
    /// </summary>
    public partial class ClassesPage : UserControl
    {
        private SportsCenterContext _db;

        // true trong lúc code tự đổ dữ liệu vào ComboBox.
        // Khi đó các sự kiện SelectionChanged bỏ qua, chỉ chạy khi NGƯỜI DÙNG đổi lựa chọn.
        private bool _loading;

        public ClassesPage()
        {
            InitializeComponent();
            _db = new();

            _loading = true;
            LoadFilters();
            LoadFormCombos();
            dpStart.SelectedDate = DateTime.Today;
            dpEnd.SelectedDate = DateTime.Today.AddDays(90);
            _loading = false;

            RefreshFreeDays();
            LoadClassList();
        }

        // ================================================================ NẠP DỮ LIỆU

        // Hai ô lọc phía trên: thêm mục "Tất cả" có Id = 0
        private void LoadFilters()
        {
            var sports = _db.Sports.OrderBy(x => x.SportName).ToList();
            var filterSports = new List<Sport>()
            {
                new Sport { SportId = 0, SportName = "Tất cả" }
            };
            filterSports.AddRange(sports);

            cbxSportFilter.ItemsSource = filterSports;
            cbxSportFilter.DisplayMemberPath = nameof(Sport.SportName);
            cbxSportFilter.SelectedValuePath = nameof(Sport.SportId);
            cbxSportFilter.SelectedIndex = 0;

            var coaches = _db.Coaches.OrderBy(x => x.FullName).ToList();
            var filterCoaches = new List<Coach>()
            {
                new Coach { CoachId = 0, FullName = "Tất cả" }
            };
            filterCoaches.AddRange(coaches);

            cbxCoachFilter.ItemsSource = filterCoaches;
            cbxCoachFilter.DisplayMemberPath = nameof(Coach.FullName);
            cbxCoachFilter.SelectedValuePath = nameof(Coach.CoachId);
            cbxCoachFilter.SelectedIndex = 0;
        }

        // Ba ô chọn trong form Thêm lớp: Bộ môn, HLV, Phòng
        private void LoadFormCombos()
        {
            cbxSport.ItemsSource = _db.Sports.OrderBy(x => x.SportName).ToList();
            cbxSport.DisplayMemberPath = nameof(Sport.SportName);
            cbxSport.SelectedValuePath = nameof(Sport.SportId);
            cbxSport.SelectedIndex = -1;

            cbxCoach.ItemsSource = _db.Coaches.OrderBy(x => x.FullName).ToList();
            cbxCoach.DisplayMemberPath = nameof(Coach.FullName);
            cbxCoach.SelectedValuePath = nameof(Coach.CoachId);
            cbxCoach.SelectedIndex = 0;

            cbxRoom.ItemsSource = _db.Rooms.OrderBy(x => x.RoomName).ToList();
            cbxRoom.DisplayMemberPath = nameof(Room.RoomName);
            cbxRoom.SelectedValuePath = nameof(Room.RoomId);
            cbxRoom.SelectedIndex = 0;

            // Ô Thứ và ô Ca được đổ dữ liệu trong RefreshFreeDays / RefreshFreeSlots
        }

        // Bảng danh sách lớp, có xét hai ô lọc
        private void LoadClassList()
        {
            int sportId = (int)cbxSportFilter.SelectedValue;
            int coachId = (int)cbxCoachFilter.SelectedValue;

            var query = _db.Classes.AsQueryable();

            if (sportId > 0)
            {
                query = query.Where(x => x.SportId == sportId);
            }
            if (coachId > 0)
            {
                query = query.Where(x => x.CoachId == coachId);
            }

            var classes = query
                .OrderBy(x => x.ClassId)
                .Select(x => new ClassViewModel()
                {
                    ClassId = x.ClassId,
                    ClassCode = x.ClassCode,
                    ClassName = x.ClassName,
                    SportName = x.Sport.SportName,
                    CoachName = x.Coach.FullName,
                    RoomName = x.Room.RoomName,
                    Schedule = Rules.ScheduleText(x.DayOfWeek, x.Slot.SlotName, x.Slot.StartTime, x.Slot.EndTime),
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Registered = x.Registrations.Count(r => r.Status == "Registered"),
                    Capacity = x.Capacity,
                    Status = x.Status == "Open" ? "Mở" : "Đã hủy"
                }).ToList();

            dgClassList.ItemsSource = pgClass.Slice(classes);
        }

        // Bấm sang trang khác ở thanh phân trang thì tải lại danh sách lớp
        private void pgClass_PageChanged(object? sender, EventArgs e)
        {
            LoadClassList();
        }

        // ================================================================ LỌC

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            pgClass.Reset();      // lọc xong về trang 1
            LoadClassList();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            cbxSportFilter.SelectedIndex = 0;
            cbxCoachFilter.SelectedIndex = 0;
            pgClass.Reset();
            LoadClassList();
        }

        // ================================================================ CA TRỐNG (form Thêm lớp)
        // Đổi HLV hoặc Phòng -> RefreshFreeDays (tính lại thứ trống) -> RefreshFreeSlots (tính lại ca trống)
        // Đổi Thứ           -> RefreshFreeSlots

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

        // Ô Thứ chỉ hiện những thứ còn ít nhất một ca trống với HLV và phòng đang chọn
        private void RefreshFreeDays()
        {
            if (cbxCoach.SelectedValue == null || cbxRoom.SelectedValue == null) return;
            int coachId = (int)cbxCoach.SelectedValue;
            int roomId = (int)cbxRoom.SelectedValue;

            // Các cặp (thứ, ca) đã bị lớp khác chiếm: cùng phòng hoặc cùng HLV
            var busy = Rules.BusySet(_db, roomId, coachId);
            var slots = _db.TimeSlots.ToList();

            var freeDays = new List<OptionViewModel>();
            for (int day = 1; day <= 7; day++)
            {
                bool hasFreeSlot = slots.Any(s => !busy.Contains((day, s.SlotId)));
                if (hasFreeSlot)
                {
                    freeDays.Add(new OptionViewModel { Id = day, Text = Rules.DayNames[day] });
                }
            }

            // Thứ đang chọn mà vẫn còn trống thì giữ nguyên, không thì chọn thứ đầu tiên
            int? oldDay = cbxDay.SelectedValue as int?;

            _loading = true;
            cbxDay.DisplayMemberPath = nameof(OptionViewModel.Text);
            cbxDay.SelectedValuePath = nameof(OptionViewModel.Id);
            cbxDay.ItemsSource = freeDays;
            if (freeDays.Count > 0)
            {
                bool keepOld = oldDay != null && freeDays.Any(d => d.Id == oldDay);
                cbxDay.SelectedValue = keepOld ? oldDay : freeDays[0].Id;
            }
            _loading = false;

            RefreshFreeSlots();

            // HLV và phòng kín lịch cả tuần: khóa form và hiện dòng báo đỏ
            bool full = freeDays.Count == 0;
            cbxDay.IsEnabled = !full;
            cbxSlot.IsEnabled = !full;
            btnAddClass.IsEnabled = !full;
            txtNoSlot.Visibility = full ? Visibility.Visible : Visibility.Collapsed;
        }

        // Ô Ca chỉ hiện những ca còn trống của thứ đang chọn
        private void RefreshFreeSlots()
        {
            if (cbxCoach.SelectedValue == null || cbxRoom.SelectedValue == null || cbxDay.SelectedValue == null)
            {
                cbxSlot.ItemsSource = null;
                return;
            }
            int coachId = (int)cbxCoach.SelectedValue;
            int roomId = (int)cbxRoom.SelectedValue;
            int day = (int)cbxDay.SelectedValue;

            var busy = Rules.BusySet(_db, roomId, coachId);

            var freeSlots = new List<OptionViewModel>();
            foreach (var s in _db.TimeSlots.OrderBy(x => x.SlotId).ToList())
            {
                if (!busy.Contains((day, s.SlotId)))
                {
                    freeSlots.Add(new OptionViewModel
                    {
                        Id = s.SlotId,
                        Text = $"{s.SlotName} ({s.StartTime:HH:mm}-{s.EndTime:HH:mm})"
                    });
                }
            }

            cbxSlot.DisplayMemberPath = nameof(OptionViewModel.Text);
            cbxSlot.SelectedValuePath = nameof(OptionViewModel.Id);
            cbxSlot.ItemsSource = freeSlots;
            if (freeSlots.Count > 0) cbxSlot.SelectedIndex = 0;
        }

        // ================================================================ THÊM LỚP

        private void btnAddClass_Click(object sender, RoutedEventArgs e)
        {
            // 1. Các ô bắt buộc
            if (string.IsNullOrWhiteSpace(txtClassName.Text) || string.IsNullOrWhiteSpace(txtClassCode.Text) || string.IsNullOrWhiteSpace(txtCapacity.Text))
            {
                Msg.Warn("Vui lòng nhập Tên lớp, Mã lớp và Sĩ số.");
                return;
            }
            if (!int.TryParse(txtCapacity.Text.Trim(), out int capacity) || capacity <= 0)
            {
                Msg.Warn("Sĩ số phải là số lớn hơn 0.");
                return;
            }
            if (cbxSport.SelectedValue == null)
            {
                Msg.Warn("Vui lòng chọn Bộ môn.");
                return;
            }
            if (cbxCoach.SelectedValue == null)
            {
                Msg.Warn("Vui lòng chọn Huấn luyện viên.");
                return;
            }
            if (cbxRoom.SelectedValue == null)
            {
                Msg.Warn("Vui lòng chọn Phòng.");
                return;
            }
            if (cbxDay.SelectedValue == null || cbxSlot.SelectedValue == null)
            {
                Msg.Warn("Vui lòng chọn Thứ và Ca học.");
                return;
            }
            if (dpStart.SelectedDate == null || dpEnd.SelectedDate == null)
            {
                Msg.Warn("Vui lòng chọn Từ ngày và Đến ngày.");
                return;
            }

            string className = txtClassName.Text.Trim();
            string classCode = txtClassCode.Text.Trim().ToUpper();
            int sportId = (int)cbxSport.SelectedValue;
            int coachId = (int)cbxCoach.SelectedValue;
            int roomId = (int)cbxRoom.SelectedValue;
            int day = (int)cbxDay.SelectedValue;
            int slotId = (int)cbxSlot.SelectedValue;
            DateTime start = dpStart.SelectedDate.Value;
            DateTime end = dpEnd.SelectedDate.Value;

            // 2. Luật C1: ngày, sĩ số, mã lớp
            if (end < start)
            {
                Msg.Warn("Ngày kết thúc không được trước ngày bắt đầu.");
                return;
            }
            var room = _db.Rooms.Find(roomId);
            if (room != null && capacity > room.Capacity)
            {
                Msg.Warn($"Sĩ số không được vượt sức chứa của phòng ({room.Capacity}).");
                return;
            }
            if (_db.Classes.Any(x => x.ClassCode == classCode))
            {
                Msg.Warn("Mã lớp đã tồn tại.");
                return;
            }

            // 3. Luật C2, C3: kiểm tra lại trùng phòng / trùng HLV (phòng khi dữ liệu vừa thay đổi)
            string? conflict = Rules.ClassConflictMessage(_db, roomId, coachId, day, slotId);
            if (conflict != null)
            {
                Msg.Warn(conflict);
                RefreshFreeDays();
                return;
            }

            // 4. Lưu lớp mới
            try
            {
                Class newClass = new Class();
                newClass.ClassCode = classCode;
                newClass.ClassName = className;
                newClass.SportId = sportId;
                newClass.CoachId = coachId;
                newClass.RoomId = roomId;
                newClass.DayOfWeek = (byte)day;
                newClass.SlotId = slotId;
                newClass.Capacity = capacity;
                newClass.StartDate = DateOnly.FromDateTime(start);
                newClass.EndDate = DateOnly.FromDateTime(end);
                newClass.Status = "Open";

                _db.Classes.Add(newClass);
                _db.SaveChanges();
                AuditHelper.Log(_db, "CreateClass", $"Tạo lớp {classCode}", "Classes", newClass.ClassId);

                Msg.Info("Đã thêm lớp " + classCode + ".");

                pgClass.Reset();
                LoadClassList();
                ClearForm();
                RefreshFreeDays();      // ca vừa dùng không còn trong form nữa
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        // ================================================================ HỦY LỚP

        private void btnCancelClass_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgClassList.SelectedItem as ClassViewModel;
            if (selected == null)
            {
                Msg.Warn("Vui lòng chọn một lớp trong danh sách.");
                return;
            }

            var cls = _db.Classes.Find(selected.ClassId);
            if (cls == null) return;
            if (cls.Status == "Cancelled")
            {
                Msg.Warn("Lớp này đã hủy rồi.");
                return;
            }

            // Luật C4: lớp còn học viên đăng ký thì không cho hủy
            string? blocked = Rules.CancelClassBlockedReason(_db, cls);
            if (blocked != null)
            {
                Msg.Warn(blocked);
                return;
            }
            if (!Msg.Confirm($"Hủy lớp {cls.ClassCode}? Ca học của lớp sẽ được trả lại thành ca trống.")) return;

            try
            {
                // Không xóa dòng, chỉ đổi trạng thái
                cls.Status = "Cancelled";
                _db.SaveChanges();
                AuditHelper.Log(_db, "CancelClass", $"Hủy lớp {cls.ClassCode}", "Classes", cls.ClassId);

                LoadClassList();
                RefreshFreeDays();      // ca của lớp vừa hủy xuất hiện lại trong form
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        // ================================================================ NHẬP LẠI

        private void btnClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtClassName.Text = "";
            txtClassCode.Text = "";
            txtCapacity.Text = "15";
            cbxSport.SelectedIndex = -1;
            dpStart.SelectedDate = DateTime.Today;
            dpEnd.SelectedDate = DateTime.Today.AddDays(90);
        }
    }
}
