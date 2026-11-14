using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;

namespace SportsCenter
{
    // Luật về đặt lớp, hủy đăng ký và buổi học để điểm danh (B1-B8). Chủ file: Người 4.
    // Cùng một class Rules với các file Rules*.cs khác (partial), tách ra để mỗi người sửa file riêng, không đụng nhau khi push.
    public static partial class Rules
    {
        // Giờ bắt đầu buổi học gần nhất (kể từ bây giờ); lớp phải được Include(Slot)
        public static DateTime? NextSession(Class c)
        {
            for (int i = 0; i < 8; i++)
            {
                var date = Today.AddDays(i);
                if (IsoDay(date) == c.DayOfWeek)
                {
                    var start = date.ToDateTime(c.Slot.StartTime);
                    if (start > DateTime.Now) return start;
                }
            }
            return null;
        }

        // Các buổi học của lớp có thể điểm danh, mới nhất trước: đúng thứ của lớp, nằm trong StartDate..EndDate và không muộn hơn hôm nay.
        // Màn Điểm danh cho chọn trong danh sách này nên coach không thể chọn sai ngày.
        public static List<DateOnly> SessionDates(Class c, int max = 12)
        {
            var list = new List<DateOnly>();
            var d = Today;
            while (IsoDay(d) != c.DayOfWeek) d = d.AddDays(-1);   // lùi về buổi gần nhất (đúng thứ của lớp)
            while (d > c.EndDate) d = d.AddDays(-7);              // lớp đã kết thúc thì tính từ buổi cuối
            while (d >= c.StartDate && list.Count < max)
            {
                list.Add(d);
                d = d.AddDays(-7);
            }
            return list;
        }

        // Đăng ký lớp cho member. Trả về thông báo lỗi, hoặc null nếu thành công
        public static string? RegisterClass(SportsCenterContext db, int memberId, int classId)
        {
            var c = db.Classes.AsNoTracking().Include(x => x.Slot).FirstOrDefault(x => x.ClassId == classId);
            if (c == null) return "Không tìm thấy lớp.";

            if (ActiveSubscription(db, memberId) == null)
                return "Member chưa có gói còn hạn nên chưa đăng ký lớp được.";

            if (c.Status != "Open" || c.EndDate < Today)
                return "Lớp đã đóng, đã hủy hoặc đã kết thúc.";

            if (db.Registrations.Any(r => r.MemberId == memberId && r.ClassId == classId && r.Status == "Registered"))
                return "Member đã đăng ký lớp này rồi.";

            int count = db.Registrations.Count(r => r.ClassId == classId && r.Status == "Registered");
            if (count >= c.Capacity)
                return $"Lớp đã đầy ({c.Capacity}/{c.Capacity}).";

            var clash = db.Registrations
                .Where(r => r.MemberId == memberId && r.Status == "Registered")
                .Select(r => r.Class)
                .FirstOrDefault(x => x.Status != "Cancelled" && x.DayOfWeek == c.DayOfWeek && x.SlotId == c.SlotId);
            if (clash != null)
                return $"Trùng lịch với lớp {clash.ClassCode} (cùng thứ và cùng ca).";

            db.Registrations.Add(new Registration
            {
                MemberId = memberId,
                ClassId = classId,
                RegisteredAt = DateTime.Now,
                Status = "Registered",
                RegisteredBy = Session.CurrentUser?.UserId
            });
            db.SaveChanges();
            AuditHelper.Log(db, "RegisterClass", $"Đăng ký member #{memberId} vào {c.ClassCode}", "Registrations", classId);
            return null;
        }

        // B6: mốc hủy lớp. Chỉ được hủy khi buổi học gần nhất còn cách hơn số giờ này (muốn đổi mốc thì sửa ở đây)
        public const int CancelHoursBefore = 24;

        // Hủy đăng ký: chỉ được hủy khi buổi học gần nhất còn cách hơn CancelHoursBefore giờ
        public static string? CancelRegistration(SportsCenterContext db, int memberId, int classId)
        {
            var reg = db.Registrations.FirstOrDefault(r => r.MemberId == memberId && r.ClassId == classId && r.Status == "Registered");
            if (reg == null) return "Member chưa đăng ký lớp này.";

            var c = db.Classes.AsNoTracking().Include(x => x.Slot).First(x => x.ClassId == classId);
            var next = NextSession(c);
            if (next != null && next.Value - DateTime.Now < TimeSpan.FromHours(CancelHoursBefore))
                return $"Chỉ được hủy trước giờ học hơn {CancelHoursBefore} giờ.";

            reg.Status = "Cancelled";
            reg.CancelledAt = DateTime.Now;
            db.SaveChanges();
            AuditHelper.Log(db, "CancelClass", $"Hủy đăng ký member #{memberId} khỏi {c.ClassCode}", "Registrations", classId);
            return null;
        }
    }
}
