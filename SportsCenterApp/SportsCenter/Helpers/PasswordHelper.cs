using System.Security.Cryptography;
using System.Text;

namespace SportsCenter
{
    // =========================================================================
    // LỚP HỖ TRỢ BĂM MẬT KHẨU (PASSWORD HELPER)
    // Tác dụng: Mã hóa mật khẩu thành chuỗi băm SHA-256 để lưu an toàn vào DB.
    // Chuẩn PRN: Viết tách bạch từng bước, dễ đọc, dễ hiểu, có comment chi tiết.
    // =========================================================================
    public static class PasswordHelper
    {
        // Hàm băm mật khẩu thành chuỗi Hex in hoa (SHA-256)
        // Ví dụ: mật khẩu "123456" -> băm ra chuỗi hex 64 ký tự khớp trong SQL
        public static string Hash(string password)
        {
            // Bước 1: Nếu mật khẩu rỗng hoặc null thì trả về rỗng
            if (string.IsNullOrEmpty(password))
            {
                return string.Empty;
            }

            // Bước 2: Chuyển chuỗi mật khẩu sang mảng byte UTF-8
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);

            // Bước 3: Sử dụng thuật toán SHA-256 để tính toán mã băm ra mảng byte
            byte[] hashBytes = SHA256.HashData(inputBytes);

            // Bước 4: Chuyển mảng byte sang chuỗi Hex dạng chữ in hoa (64 ký tự)
            string hexString = Convert.ToHexString(hashBytes);

            // Bước 5: Trả về kết quả chuỗi hex đã băm
            return hexString;
        }
    }
}
