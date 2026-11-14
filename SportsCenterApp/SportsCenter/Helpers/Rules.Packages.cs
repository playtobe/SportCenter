using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;

namespace SportsCenter
{
    // Luật về gói thành viên (P3, P4). Chủ file: Người 2.
    // Cùng một class Rules với các file Rules*.cs khác (partial), tách ra để mỗi người sửa file riêng, không đụng nhau khi push.
    public static partial class Rules
    {
        // Gói còn hạn: chưa hủy và StartDate <= hôm nay <= EndDate
        public static Subscription? ActiveSubscription(SportsCenterContext db, int memberId)
        {
            var today = Today;
            return db.Subscriptions
                .AsNoTracking()
                .Where(s => s.MemberId == memberId && s.Status != "Cancelled" && s.StartDate <= today && s.EndDate >= today)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();
        }

        // Gói còn hạn nếu có, không thì gói gần nhất (dùng để hiện trạng thái trong danh sách)
        public static Subscription? CurrentOrLatest(IEnumerable<Subscription> mine)
        {
            var today = Today;
            return mine.Where(s => s.Status != "Cancelled" && s.StartDate <= today && s.EndDate >= today)
                       .OrderByDescending(s => s.EndDate).FirstOrDefault()
                   ?? mine.OrderByDescending(s => s.EndDate).FirstOrDefault();
        }

        public static string SubscriptionText(Subscription? s)
        {
            if (s == null) return "Chưa có gói";
            if (s.Status == "Cancelled") return "Đã hủy";
            return s.EndDate < Today ? "Hết hạn" : "Còn hạn";
        }

        // P3: ngày bắt đầu của gói mới = ngày sau khi gói XA NHẤT chưa hết hạn kết thúc.
        // Tính cả gói đã mua nối tiếp mà chưa tới ngày bắt đầu, để mua nhiều lần thì các gói xếp nối đuôi nhau, không chồng lên nhau.
        // Không còn gói nào chưa hết hạn thì bắt đầu từ hôm nay.
        public static DateOnly NextStartDate(SportsCenterContext db, int memberId)
        {
            var today = Today;
            DateOnly? lastEnd = db.Subscriptions
                .AsNoTracking()
                .Where(s => s.MemberId == memberId && s.Status != "Cancelled" && s.EndDate >= today)
                .OrderByDescending(s => s.EndDate)
                .Select(s => (DateOnly?)s.EndDate)
                .FirstOrDefault();
            return lastEnd.HasValue ? lastEnd.Value.AddDays(1) : today;
        }

        // Các gói đã mua nối tiếp nhưng chưa tới ngày bắt đầu (gói hiện tại vẫn đang dùng cho tới khi hết hạn)
        public static List<Subscription> QueuedSubscriptions(SportsCenterContext db, int memberId)
        {
            var today = Today;
            return db.Subscriptions
                .AsNoTracking()
                .Include(s => s.Package)
                .Where(s => s.MemberId == memberId && s.Status != "Cancelled" && s.StartDate > today)
                .OrderBy(s => s.StartDate)
                .ToList();
        }
    }
}
