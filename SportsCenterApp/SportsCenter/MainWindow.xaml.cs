using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Các trang khác (vd danh sách member) gọi MainWindow.Instance.ShowPage(...) để chuyển màn hình
        public static MainWindow? Instance { get; private set; }

        private readonly Dictionary<Type, Button> _buttons = new();
        private readonly Dictionary<Type, Expander> _groupOf = new();
        private readonly List<Expander> _groups = new();
        private Type? _firstPage;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            var user = Session.CurrentUser!;
            txtUserName.Text = user.FullName;
            txtUserLogin.Text = user.Username;
            txtRole.Text = Rules.RoleNames[user.Role];

            BuildMenu(user.Role);
            ShowPage((UserControl)Activator.CreateInstance(_firstPage!)!);
        }

        // Menu theo role: mục đơn là nút thường, nhóm bấm để xổ xuống (mở nhóm này thì thu nhóm khác)
        private void BuildMenu(string role)
        {
            switch (role)
            {
                case "Manager":
                    AddItem("Báo cáo", typeof(DashboardPage));
                    AddGroup("Quản lý lớp", ("Lớp học", typeof(ClassesPage)), ("Bộ môn, phòng", typeof(SportsRoomsPage)));
                    AddGroup("Thành viên và gói", ("Member", typeof(MembersPage)), ("Gói thành viên", typeof(PackagesPage)));
                    AddItem("Thanh toán", typeof(PaymentsPage));
                    AddGroup("Hệ thống", ("Tài khoản", typeof(UsersPage)), ("Lịch sử hoạt động", typeof(AuditPage)));
                    break;

                case "Receptionist":
                    AddGroup("Thành viên", ("Member", typeof(MembersPage)), ("Mua / gia hạn gói", typeof(BuyPage)));
                    AddItem("Đặt lớp", typeof(BookingPage));
                    AddItem("Thanh toán", typeof(PaymentsPage));
                    break;

                case "Member":
                    AddItem("Lịch của tôi", typeof(MySchedulePage));
                    AddItem("Đặt lớp", typeof(BookingPage));
                    AddItem("Gói thành viên", typeof(BuyPage));
                    AddItem("Thanh toán", typeof(PaymentsPage));
                    break;

                case "Coach":
                    AddItem("Lớp của tôi", typeof(CoachPage));
                    AddItem("Điểm danh", typeof(AttendancePage));
                    break;
            }
        }

        private Button MakeButton(string title, Type pageType)
        {
            var btn = new Button { Content = title, Style = (Style)FindResource("MenuButton") };
            btn.Click += (s, e) => ShowPage((UserControl)Activator.CreateInstance(pageType)!);
            _buttons[pageType] = btn;
            _firstPage ??= pageType;
            return btn;
        }

        private void AddItem(string title, Type pageType) => panelMenu.Children.Add(MakeButton(title, pageType));

        private void AddGroup(string title, params (string Title, Type PageType)[] items)
        {
            var inner = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
            var group = new Expander { Header = title, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 2, 0, 2), Content = inner };

            foreach (var item in items)
            {
                inner.Children.Add(MakeButton(item.Title, item.PageType));
                _groupOf[item.PageType] = group;
            }

            group.Expanded += (s, e) =>
            {
                foreach (var g in _groups)
                {
                    if (g != group) g.IsExpanded = false;
                }
            };
            _groups.Add(group);
            panelMenu.Children.Add(group);
        }

        // Hiện một trang trong vùng nội dung và tô sáng mục menu tương ứng
        public void ShowPage(UserControl page)
        {
            contentHost.Content = page;
            Type type = page.GetType();

            foreach (var kv in _buttons)
            {
                kv.Value.Tag = kv.Key == type ? "on" : null;
            }
            if (_groupOf.TryGetValue(type, out var group))
            {
                group.IsExpanded = true;
            }
        }

        // Đổi mật khẩu là hộp thoại nhỏ, không phải một màn hình trong menu
        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            new ChangePasswordWindow { Owner = this }.ShowDialog();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Session.Clear();
            new LoginWindow().Show();
            Close();
        }
    }
}
