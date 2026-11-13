using System.Windows;

namespace SportsCenter
{
    // Gọi MessageBox ngắn gọn (giống cách Q2 báo lỗi / thành công)
    public static class Msg
    {
        public static void Warn(string message) =>
            MessageBox.Show(message, "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);

        public static void Info(string message) =>
            MessageBox.Show(message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

        public static void Error(Exception ex) =>
            MessageBox.Show(ex.InnerException?.Message ?? ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);

        public static bool Confirm(string message) =>
            MessageBox.Show(message, "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
