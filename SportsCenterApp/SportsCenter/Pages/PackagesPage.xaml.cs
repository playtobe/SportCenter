using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    /// <summary>
    /// Interaction logic for PackagesPage.xaml
    /// </summary>
    public partial class PackagesPage : UserControl
    {
        private SportsCenterContext _db;

        public PackagesPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgPackages);
            pgPackages.PageChanged += (s, e) => LoadPackages();
            LoadPackages();
        }

        private void LoadPackages()
        {
            var rows = _db.MembershipPackages
                .OrderBy(p => p.PackageId)
                .Select(p => new
                {
                    p.PackageId,
                    p.PackageName,
                    p.DurationDays,
                    p.Price,
                    Status = p.IsActive ? "Đang bán" : "Ngừng bán"
                })
                .ToList();
            dgPackages.ItemsSource = pgPackages.Slice(rows);
        }

        // Gói đã có người mua thì không xóa, chỉ ngừng bán
        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            int? id = GridHelper.SelectedId(dgPackages, "PackageId");
            if (id == null)
            {
                Msg.Warn("Vui lòng chọn một gói trong danh sách.");
                return;
            }

            try
            {
                var pkg = _db.MembershipPackages.Find(id)!;
                pkg.IsActive = !pkg.IsActive;
                _db.SaveChanges();
                AuditHelper.Log(_db, "TogglePackage", $"{(pkg.IsActive ? "Mở bán" : "Ngừng bán")} {pkg.PackageName}", "MembershipPackages", pkg.PackageId);
                LoadPackages();
                GridHelper.SelectById(dgPackages, "PackageId", pkg.PackageId);
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string name = txtPackageName.Text.Trim();
            if (name == "" || txtDays.Text.Trim() == "" || txtPrice.Text.Trim() == "")
            {
                Msg.Warn("Vui lòng nhập Tên gói, Số ngày và Giá.");
                return;
            }
            if (!int.TryParse(txtDays.Text.Trim(), out int days) || days <= 0)
            {
                Msg.Warn("Số ngày phải là số lớn hơn 0.");
                return;
            }
            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price) || price < 0)
            {
                Msg.Warn("Giá phải là số không âm.");
                return;
            }

            try
            {
                var pkg = new MembershipPackage { PackageName = name, DurationDays = days, Price = price, IsActive = true };
                _db.MembershipPackages.Add(pkg);
                _db.SaveChanges();
                AuditHelper.Log(_db, "CreatePackage", $"Thêm gói {name}", "MembershipPackages", pkg.PackageId);

                txtPackageName.Clear();
                txtDays.Text = "30";
                txtPrice.Text = "500000";
                pgPackages.Reset();
                LoadPackages();
                Msg.Info("Đã thêm gói " + name + ".");
            }
            catch (Exception ex)
            {
                Msg.Error(ex);
            }
        }
    }
}
