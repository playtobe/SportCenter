using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for SportsRoomsPage.xaml
    /// </summary>
    public partial class SportsRoomsPage : UserControl
    {
        private SportsCenterContext _db;

        public SportsRoomsPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgSports);
            GridHelper.Setup(dgRooms);
            pgSports.PageChanged += (s, e) => LoadSports();
            pgRooms.PageChanged += (s, e) => LoadRooms();
            LoadSports();
            LoadRooms();
        }

        private void LoadSports()
        {
            var rows = _db.Sports
                .OrderBy(s => s.SportName)
                .Select(s => new { s.SportId, s.SportName, ClassCount = s.Classes.Count })
                .ToList();
            dgSports.ItemsSource = pgSports.Slice(rows);
        }

        private void LoadRooms()
        {
            var rows = _db.Rooms
                .OrderBy(r => r.RoomName)
                .Select(r => new { r.RoomId, r.RoomName, r.Capacity, ClassCount = r.Classes.Count })
                .ToList();
            dgRooms.ItemsSource = pgRooms.Slice(rows);
        }

        private void btnAddSport_Click(object sender, RoutedEventArgs e)
        {
            string name = txtSportName.Text.Trim();
            if (name == "")
            {
                Msg.Warn("Vui lòng nhập tên bộ môn.");
                return;
            }
            if (_db.Sports.Any(s => s.SportName == name))
            {
                Msg.Warn("Bộ môn này đã có.");
                return;
            }

            try
            {
                var sport = new Sport { SportName = name };
                _db.Sports.Add(sport);
                _db.SaveChanges();
                AuditHelper.Log(_db, "CreateSport", $"Thêm bộ môn {name}", "Sports", sport.SportId);
                txtSportName.Clear();
                LoadSports();
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        private void btnDeleteSport_Click(object sender, RoutedEventArgs e)
        {
            int? id = GridHelper.SelectedId(dgSports, "SportId");
            if (id == null)
            {
                Msg.Warn("Vui lòng chọn một bộ môn.");
                return;
            }
            if (_db.Classes.Any(c => c.SportId == id))
            {
                Msg.Warn("Bộ môn đang có lớp, không thể xóa.");
                return;
            }
            if (!Msg.Confirm("Xóa bộ môn đã chọn?")) return;

            try
            {
                var sport = _db.Sports.Find(id)!;
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

        private void btnAddRoom_Click(object sender, RoutedEventArgs e)
        {
            string name = txtRoomName.Text.Trim();
            if (name == "" || txtRoomCapacity.Text.Trim() == "")
            {
                Msg.Warn("Vui lòng nhập tên phòng và sức chứa.");
                return;
            }
            if (!int.TryParse(txtRoomCapacity.Text.Trim(), out int capacity) || capacity <= 0)
            {
                Msg.Warn("Sức chứa phải là số lớn hơn 0.");
                return;
            }
            if (_db.Rooms.Any(r => r.RoomName == name))
            {
                Msg.Warn("Phòng này đã có.");
                return;
            }

            try
            {
                var room = new Room { RoomName = name, Capacity = capacity, IsActive = true };
                _db.Rooms.Add(room);
                _db.SaveChanges();
                AuditHelper.Log(_db, "CreateRoom", $"Thêm phòng {name}", "Rooms", room.RoomId);
                txtRoomName.Clear();
                LoadRooms();
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        private void btnDeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            int? id = GridHelper.SelectedId(dgRooms, "RoomId");
            if (id == null)
            {
                Msg.Warn("Vui lòng chọn một phòng.");
                return;
            }
            if (_db.Classes.Any(c => c.RoomId == id))
            {
                Msg.Warn("Phòng đang có lớp, không thể xóa.");
                return;
            }
            if (!Msg.Confirm("Xóa phòng đã chọn?")) return;

            try
            {
                var room = _db.Rooms.Find(id)!;
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
