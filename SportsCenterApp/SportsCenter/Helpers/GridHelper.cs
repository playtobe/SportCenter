using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SportsCenter
{
    // Cấu hình DataGrid dùng chung: đặt tiêu đề cột tiếng Việt, định dạng số / ngày, ẩn cột Id
    public static class GridHelper
    {
        private static readonly Dictionary<string, string> Headers = new()
        {
            ["ClassCode"] = "Mã lớp",
            ["ClassName"] = "Tên lớp",
            ["SportName"] = "Bộ môn",
            ["CoachName"] = "HLV",
            ["RoomName"] = "Phòng",
            ["Schedule"] = "Lịch",
            ["Slots"] = "Sĩ số",
            ["Status"] = "Trạng thái",
            ["ClassCount"] = "Số lớp",
            ["Capacity"] = "Sức chứa",
            ["PackageName"] = "Gói",
            ["DurationDays"] = "Thời hạn (ngày)",
            ["Price"] = "Giá (₫)",
            ["Username"] = "Tên đăng nhập",
            ["FullName"] = "Họ tên",
            ["Role"] = "Vai trò",
            ["At"] = "Thời gian",
            ["User"] = "Người dùng",
            ["Action"] = "Hành động",
            ["Details"] = "Chi tiết",
            ["Phone"] = "Điện thoại",
            ["HasAccount"] = "Tài khoản",
            ["EndDate"] = "Hạn đến",
            ["MyStatus"] = "Của bạn",
            ["InvoiceNo"] = "Hóa đơn",
            ["Member"] = "Member",
            ["Student"] = "Học viên",
            ["Amount"] = "Số tiền (₫)",
            ["PaidAt"] = "Ngày thu",
            ["Registered"] = "Đăng ký",
            ["FillPercent"] = "Lấp đầy",
            ["PackageStatus"] = "Gói thành viên",
            ["Present"] = "Có mặt",
            ["Date"] = "Ngày học"
        };

        // Độ rộng tương đối của cột (mặc định 1): cột có chữ dài thì rộng hơn để không bị cắt
        private static readonly Dictionary<string, double> Weights = new()
        {
            ["Schedule"] = 1.9,
            ["ClassName"] = 1.6,
            ["PaidAt"] = 1.5,
            ["At"] = 1.5,
            ["Details"] = 3,
            ["PackageName"] = 1.2,
            ["FullName"] = 1.3,
            ["Student"] = 1.4
        };

        // editable = true: chỉ cho sửa cột bool (dùng cho màn Điểm danh)
        public static void Setup(DataGrid dg, bool editable = false)
        {
            dg.AutoGenerateColumns = true;
            dg.IsReadOnly = !editable;
            dg.AutoGeneratingColumn += (s, e) =>
            {
                string name = e.PropertyName;

                // Ẩn các cột khóa (ClassId, MemberId...) nhưng vẫn giữ giá trị để lấy khi chọn dòng
                if (name.Length > 2 && name.EndsWith("Id"))
                {
                    e.Cancel = true;
                    return;
                }

                string header = Headers.TryGetValue(name, out var h) ? h : name;
                Type type = Nullable.GetUnderlyingType(e.PropertyType) ?? e.PropertyType;

                if (editable && type == typeof(bool))
                {
                    // Cột checkbox bấm một lần là đổi (cột checkbox mặc định phải bấm hai lần)
                    var factory = new FrameworkElementFactory(typeof(CheckBox));
                    factory.SetBinding(CheckBox.IsCheckedProperty,
                        new Binding(name) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    factory.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 0, 0, 0));
                    e.Column = new DataGridTemplateColumn
                    {
                        Header = header,
                        Width = new DataGridLength(120),
                        CellTemplate = new DataTemplate { VisualTree = factory }
                    };
                    return;
                }

                e.Column.Header = header;
                if (Weights.TryGetValue(name, out double weight))
                {
                    e.Column.Width = new DataGridLength(weight, DataGridLengthUnitType.Star);
                }
                if (editable)
                {
                    e.Column.IsReadOnly = true;
                }

                if (e.Column is DataGridTextColumn col && col.Binding is Binding b)
                {
                    if (type == typeof(decimal))
                        b.StringFormat = "N0";
                    else if (type == typeof(DateTime))
                        b.StringFormat = "dd/MM/yyyy HH:mm";
                    else if (type == typeof(DateOnly))
                        b.StringFormat = "dd/MM/yyyy";
                }
            };
        }

        // Chọn lại dòng có Id này (dùng sau khi tải lại danh sách, để người dùng không phải chọn lại)
        public static void SelectById(DataGrid dg, string propertyName, int id)
        {
            foreach (var item in dg.Items)
            {
                if (item.GetType().GetProperty(propertyName)?.GetValue(item) as int? == id)
                {
                    dg.SelectedItem = item;
                    return;
                }
            }
        }

        // Lấy giá trị cột Id (vd "ClassId") của dòng đang chọn; không chọn gì thì trả về null
        public static int? SelectedId(DataGrid dg, string propertyName)
        {
            object? item = dg.SelectedItem;
            if (item == null) return null;
            return item.GetType().GetProperty(propertyName)?.GetValue(item) as int?;
        }
    }
}
