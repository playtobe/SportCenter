using SportsCenter.Entities;
using SportsCenter.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for SportsRoomsPage.xaml
    /// Màn Bộ môn và phòng tập (Manager): thêm / xóa bộ môn, thêm / xóa phòng.
    /// </summary>
    public partial class SportsRoomsPage : UserControl
    {
        private SportsCenterContext _db;

        public SportsRoomsPage()
        {
            InitializeComponent();
            _db = new();

            LoadSports();
            LoadRooms();
        }

        // ================================================================ BỘ MÔN

        private void LoadSports()
        {
            var sports = _db.Sports
                .OrderBy(x => x.SportName)
                .Select(x => new SportViewModel()
                {
                    SportId = x.SportId,
                    SportName = x.SportName,
                    ClassCount = x.Classes.Count
                }).ToList();

            dgSports.ItemsSource = pgSports.Slice(sports);
        }

        // Bấm sang trang khác ở thanh phân trang của bảng Bộ môn thì tải lại bảng Bộ môn
        private void pgSports_PageChanged(object? sender, EventArgs e)
        {
            LoadSports();
        }

        private void btnAddSport_Click(object sender, RoutedEventArgs e)
        {
            string name = txtSportName.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Msg.Warn("Vui lòng nhập tên bộ môn.");
                return;
            }
            if (_db.Sports.Any(x => x.SportName == name))
            {
                Msg.Warn("Bộ môn này đã có.");
                return;
            }

            try
            {
                Sport sport = new Sport();
                sport.SportName = name;

                _db.Sports.Add(sport);
                _db.SaveChanges();
                AuditHelper.Log(_db, "CreateSport", $"Thêm bộ môn {name}", "Sports", sport.SportId);

                txtSportName.Text = "";
                LoadSports();
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        private void btnDeleteSport_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgSports.SelectedItem as SportViewModel;
            if (selected == null)
            {
                Msg.Warn("Vui lòng chọn một bộ môn.");
                return;
            }
            // Bộ môn đang có lớp thì không xóa được (luật C4)
            if (_db.Classes.Any(x => x.SportId == selected.SportId))
            {
                Msg.Warn("Bộ môn đang có lớp, không thể xóa.");
                return;
            }
            if (!Msg.Confirm("Xóa bộ môn " + selected.SportName + "?")) return;

            try
            {
                var sport = _db.Sports.Find(selected.SportId);
                if (sport == null) return;

                _db.Sports.Remove(sport);
                _db.SaveChanges();
                AuditHelper.Log(_db, "DeleteSport", $"Xóa bộ môn {sport.SportName}", "Sports", sport.SportId);

                LoadSports();
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        // ================================================================ PHÒNG TẬP

        private void LoadRooms()
        {
            var rooms = _db.Rooms
                .OrderBy(x => x.RoomName)
                .Select(x => new RoomViewModel()
                {
                    RoomId = x.RoomId,
                    RoomName = x.RoomName,
                    Capacity = x.Capacity,
                    ClassCount = x.Classes.Count
                }).ToList();

            dgRooms.ItemsSource = pgRooms.Slice(rooms);
        }

        // Bấm sang trang khác ở thanh phân trang của bảng Phòng thì tải lại bảng Phòng
        private void pgRooms_PageChanged(object? sender, EventArgs e)
        {
            LoadRooms();
        }

        private void btnAddRoom_Click(object sender, RoutedEventArgs e)
        {
            string name = txtRoomName.Text.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(txtRoomCapacity.Text))
            {
                Msg.Warn("Vui lòng nhập tên phòng và sức chứa.");
                return;
            }
            if (!int.TryParse(txtRoomCapacity.Text.Trim(), out int capacity) || capacity <= 0)
            {
                Msg.Warn("Sức chứa phải là số lớn hơn 0.");
                return;
            }
            if (_db.Rooms.Any(x => x.RoomName == name))
            {
                Msg.Warn("Phòng này đã có.");
                return;
            }

            try
            {
                Room room = new Room();
                room.RoomName = name;
                room.Capacity = capacity;
                room.IsActive = true;

                _db.Rooms.Add(room);
                _db.SaveChanges();
                AuditHelper.Log(_db, "CreateRoom", $"Thêm phòng {name}", "Rooms", room.RoomId);

                txtRoomName.Text = "";
                LoadRooms();
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        private void btnDeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgRooms.SelectedItem as RoomViewModel;
            if (selected == null)
            {
                Msg.Warn("Vui lòng chọn một phòng.");
                return;
            }
            // Phòng đang có lớp thì không xóa được (luật C4)
            if (_db.Classes.Any(x => x.RoomId == selected.RoomId))
            {
                Msg.Warn("Phòng đang có lớp, không thể xóa.");
                return;
            }
            if (!Msg.Confirm("Xóa phòng " + selected.RoomName + "?")) return;

            try
            {
                var room = _db.Rooms.Find(selected.RoomId);
                if (room == null) return;

                _db.Rooms.Remove(room);
                _db.SaveChanges();
                AuditHelper.Log(_db, "DeleteRoom", $"Xóa phòng {room.RoomName}", "Rooms", room.RoomId);

                LoadRooms();
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }
    }
}
