using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SportsCenter
{
    public partial class PackagesPage : UserControl
    {
        private SportsCenterContext _db;

        public PackagesPage()
        {
            InitializeComponent();
            _db = new();
            GridHelper.Setup(dgPackages);   // cài bảng: tiêu đề tiếng Việt, ẩn cột Id (của Người 5)
            LoadData();
        }

        private void LoadData()
        {
            LoadPackages();
        }

        // Hiện danh sách gói lên bảng
        private void LoadPackages()
        {
            dgPackages.ItemsSource = _db.MembershipPackages
                .OrderBy(p => p.PackageId)
                .Select(p => new
                {
                    p.PackageId,
                    p.PackageName,
                    p.DurationDays,
                    p.Price,
                    Status = p.IsActive ? "Đang bán" : "Ngừng bán"
                }).ToList();
        }

        // Gói đã có người mua thì không xóa, chỉ ngừng bán / mở bán lại
        private void btnTogglePackage_Click(object sender, RoutedEventArgs e)
        {
            if (GridHelper.SelectedId(dgPackages, "PackageId") is not int id)
            {
                MessageBox.Show("Vui lòng chọn một gói trong danh sách.");
                return;
            }

            try
            {
                var pkg = _db.MembershipPackages.Find(id)!;
                pkg.IsActive = !pkg.IsActive;
                _db.SaveChanges();
                AuditHelper.Log(_db, "TogglePackage", (pkg.IsActive ? "Mở bán " : "Ngừng bán ") + pkg.PackageName, "MembershipPackages", pkg.PackageId);
                LoadPackages();
                GridHelper.SelectById(dgPackages, "PackageId", pkg.PackageId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
            }
        }

        private void btnAddPackage_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra dữ liệu nhập
            string name = txtPackageName.Text.Trim();
            if (name == "" || txtDays.Text.Trim() == "" || txtPrice.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập Tên gói, Số ngày và Giá.");
                return;
            }
            if (!int.TryParse(txtDays.Text.Trim(), out int days) || days <= 0)
            {
                MessageBox.Show("Số ngày phải là số lớn hơn 0.");
                return;
            }
            if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price) || price < 0)
            {
                MessageBox.Show("Giá phải là số không âm.");
                return;
            }
            // Tên gói không được trùng (không phân biệt hoa thường và dấu: "Gói 3 tháng" == "goi 3 thang")
            foreach (var p in _db.MembershipPackages.ToList())
            {
                if (AccountHelper.Normalize(p.PackageName) == AccountHelper.Normalize(name))
                {
                    MessageBox.Show("Đã có gói tên \"" + name + "\".");
                    return;
                }
            }

            // 2. Thêm gói
            try
            {
                var pkg = new MembershipPackage
                {
                    PackageName = name,
                    DurationDays = days,
                    Price = price,
                    IsActive = true
                };
                _db.MembershipPackages.Add(pkg);
                _db.SaveChanges();
                AuditHelper.Log(_db, "CreatePackage", "Thêm gói " + name, "MembershipPackages", pkg.PackageId);

                // 3. Làm mới màn hình
                LoadData();
                ClearForm();
                MessageBox.Show("Đã thêm gói " + name + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
            }
        }

        // Trả các ô nhập về giá trị mặc định
        private void ClearForm()
        {
            txtPackageName.Clear();
            txtDays.Text = "30";
            txtPrice.Text = "500000";
        }
    }
}
