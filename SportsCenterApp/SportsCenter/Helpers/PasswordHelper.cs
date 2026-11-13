using System.Security.Cryptography;
using System.Text;

namespace SportsCenter
{
    public static class PasswordHelper
    {
        // SHA-256, chuỗi hex IN HOA (khớp với dữ liệu mẫu trong SportsCenter.sql)
        public static string Hash(string password) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }
}
