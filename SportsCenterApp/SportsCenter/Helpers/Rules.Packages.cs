using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;

namespace SportsCenter
{
    // Luật về gói thành viên (P3, P4). Cùng một class Rules với các file Rules*.cs khác (partial).
    public static partial class Rules
    {
        // Gói đang dùng: chưa hủy, đã bắt đầu và chưa hết hạn. Không có thì trả null.
        public static Subscription? ActiveSubscription(SportsCenterContext db, int memberId)
        {
            var today = Today;
            return db.Subscriptions
                .AsNoTracking()
                .Where(s => s.MemberId == memberId && s.Status != "Cancelled" && s.StartDate <= today && s.EndDate >= today)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();
        }

        // Gói còn hạn nếu có, không thì gói gần nhất (để hiện trạng thái trong danh sách Member)
        public static Subscription? CurrentOrLatest(IEnumerable<Subscription> mine)
        {
            var today = Today;
            var current = mine
                .Where(s => s.Status != "Cancelled" && s.StartDate <= today && s.EndDate >= today)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();
            if (current != null)
            {
                return current;
            }
            return mine.OrderByDescending(s => s.EndDate).FirstOrDefault();
        }

        public static string SubscriptionText(Subscription? s)
        {
            if (s == null) return "Chưa có gói";
            if (s.Status == "Cancelled") return "Đã hủy";
            if (s.EndDate < Today) return "Hết hạn";
            return "Còn hạn";
        }

        // P3: ngày bắt đầu của gói mới = ngày sau khi gói XA NHẤT chưa hết hạn kết thúc
        // (tính cả gói đã mua nối tiếp). Không còn gói nào thì bắt đầu từ hôm nay.
        public static DateOnly NextStartDate(SportsCenterContext db, int memberId)
        {
            var today = Today;
            var last = db.Subscriptions
                .AsNoTracking()
                .Where(s => s.MemberId == memberId && s.Status != "Cancelled" && s.EndDate >= today)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefault();
            if (last == null)
            {
                return today;
            }
            return last.EndDate.AddDays(1);
        }

        // Các gói đã mua nối tiếp nhưng chưa tới ngày bắt đầu
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
