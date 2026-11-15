using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Khung giao diện chính của ứng dụng (Application Shell)
    /// </summary>
    public partial class MainWindow : Window
    {
        // 1. Biến static Instance để các màn hình con có thể gọi chuyển trang: MainWindow.Instance?.ShowPage(...)
        public static MainWindow? Instance { get; private set; }

        // Lưu danh sách các nút menu tương ứng với từng loại trang
        private readonly Dictionary<Type, Button> _buttons = new();

        // Lưu quan hệ giữa loại trang và nhóm menu chứa nó (Expander)
        private readonly Dictionary<Type, Expander> _groupOf = new();

        // Danh sách các nhóm menu để xử lý đóng/mở (Accordion)
        private readonly List<Expander> _groups = new();

        // Trang mặc định được mở đầu tiên khi đăng nhập
        private Type? _firstPage;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this; // Gán thể hiện hiện tại vào static Instance

            // Bước 1: Lấy thông tin người dùng đang đăng nhập từ Session
            var user = Session.CurrentUser!;
            txtUserName.Text = user.FullName;
            txtUserLogin.Text = user.Username;

            // Hiển thị tên vai trò tiếng Việt (Quản lý, Lễ tân, Hội viên, Huấn luyện viên)
            txtRole.Text = Rules.RoleNames.ContainsKey(user.Role) ? Rules.RoleNames[user.Role] : user.Role;

            // Bước 2: Xây dựng menu tương ứng với vai trò của người dùng
            BuildMenu(user.Role);

            // Bước 3: Mở trang đầu tiên mặc định lên màn hình
            if (_firstPage != null)
            {
                UserControl initialPage = (UserControl)Activator.CreateInstance(_firstPage)!;
                ShowPage(initialPage);
            }
        }

        // =====================================================================
        // Xây dựng hệ thống menu động dựa theo vai trò (Role-based Navigation)
        // =====================================================================
        private void BuildMenu(string role)
        {
            switch (role)
            {
                case "Manager": // Vai trò Quản lý: Toàn quyền truy cập mọi tính năng
                    AddItem("Báo cáo", typeof(DashboardPage));
                    AddGroup("Quản lý lớp", ("Lớp học", typeof(ClassesPage)), ("Bộ môn, phòng", typeof(SportsRoomsPage)));
                    AddGroup("Thành viên và gói", ("Member", typeof(MembersPage)), ("Gói thành viên", typeof(PackagesPage)));
                    AddItem("Thanh toán", typeof(PaymentsPage));
                    AddGroup("Hệ thống", ("Tài khoản", typeof(UsersPage)), ("Lịch sử hoạt động", typeof(AuditPage)));
                    break;

                case "Receptionist": // Vai trò Lễ tân: Quản lý hội viên, bán gói, đặt lớp, thanh toán
                    AddGroup("Thành viên", ("Member", typeof(MembersPage)), ("Mua / gia hạn gói", typeof(BuyPage)));
                    AddItem("Đặt lớp", typeof(BookingPage));
                    AddItem("Thanh toán", typeof(PaymentsPage));
                    break;

                case "Member": // Vai trò Hội viên: Xem lịch tập, đặt lớp, đăng ký gói, xem thanh toán
                    AddItem("Lịch của tôi", typeof(MySchedulePage));
                    AddItem("Đặt lớp", typeof(BookingPage));
                    AddItem("Gói thành viên", typeof(BuyPage));
                    AddItem("Thanh toán", typeof(PaymentsPage));
                    break;

                case "Coach": // Vai trò Huấn luyện viên: Xem lớp dạy và điểm danh học viên
                    AddItem("Lớp của tôi", typeof(CoachPage));
                    AddItem("Điểm danh", typeof(AttendancePage));
                    break;
            }
        }

        // =====================================================================
        // Hàm tạo một nút bấm đơn trong menu
        // =====================================================================
        private Button MakeButton(string title, Type pageType)
        {
            // Tạo nút với tiêu đề và áp dụng Style MenuButton
            Button btn = new Button
            {
                Content = title,
                Style = (Style)FindResource("MenuButton")
            };

            // Bắt sự kiện khi click nút -> Khởi tạo trang và hiển thị vào contentHost
            btn.Click += (s, e) =>
            {
                UserControl pageInstance = (UserControl)Activator.CreateInstance(pageType)!;
                ShowPage(pageInstance);
            };

            // Lưu vào từ điển tra cứu
            _buttons[pageType] = btn;

            // Đặt trang đầu tiên xuất hiện nếu chưa có
            if (_firstPage == null)
            {
                _firstPage = pageType;
            }

            return btn;
        }

        // =====================================================================
        // Thêm một mục menu đơn lẻ (không có nhóm con)
        // =====================================================================
        private void AddItem(string title, Type pageType)
        {
            Button btn = MakeButton(title, pageType);
            panelMenu.Children.Add(btn);
        }

        // =====================================================================
        // Thêm một nhóm menu dạng xổ xuống (Expander)
        // =====================================================================
        private void AddGroup(string title, params (string Title, Type PageType)[] items)
        {
            StackPanel innerPanel = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
            Expander group = new Expander
            {
                Header = title,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 2, 0, 2),
                Content = innerPanel
            };

            // Tạo từng nút bấm bên trong nhóm
            foreach (var item in items)
            {
                Button btn = MakeButton(item.Title, item.PageType);
                innerPanel.Children.Add(btn);
                _groupOf[item.PageType] = group;
            }

            // Tự động thu gọn các nhóm khác khi mở nhóm này (Accordion effect)
            group.Expanded += (s, e) =>
            {
                foreach (Expander g in _groups)
                {
                    if (g != group)
                    {
                        g.IsExpanded = false;
                    }
                }
            };

            _groups.Add(group);
            panelMenu.Children.Add(group);
        }

        // =====================================================================
        // Hiển thị một trang con vào vùng nội dung chính (contentHost)
        // và tô sáng nút menu đang được chọn
        // =====================================================================
        public void ShowPage(UserControl page)
        {
            // Bước 1: Nạp UserControl vào Frame/ContentControl chính
            contentHost.Content = page;
            Type pageType = page.GetType();

            // Bước 2: Đánh dấu trạng thái "on" cho nút menu đang được chọn
            foreach (var kv in _buttons)
            {
                kv.Value.Tag = (kv.Key == pageType) ? "on" : null;
            }

            // Bước 3: Nếu trang nằm trong nhóm Expander, tự động mở rộng nhóm đó
            if (_groupOf.TryGetValue(pageType, out Expander? group) && group != null)
            {
                group.IsExpanded = true;
            }
        }

        // =====================================================================
        // Sự kiện khi bấm nút "Đổi mật khẩu" ở góc dưới thanh bên
        // =====================================================================
        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Mở cửa sổ Đổi mật khẩu dưới dạng Modal Dialog
            ChangePasswordWindow dlg = new ChangePasswordWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        // =====================================================================
        // Sự kiện khi bấm nút "Đăng xuất" (btnLogout)
        // =====================================================================
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Bước 1: Xóa thông tin phiên làm việc trong Session
            Session.Clear();

            // Bước 2: Mở lại màn hình Đăng nhập
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // Bước 3: Đóng màn hình chính
            this.Close();
        }
    }
}
