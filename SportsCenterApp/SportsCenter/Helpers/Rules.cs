using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;

namespace SportsCenter
{
    // Các luật nghiệp vụ dùng chung giữa nhiều màn hình (gói, đặt lớp, trùng lịch, hóa đơn)
    public static class Rules
    {
        public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

        public static readonly string[] DayNames =
            { "", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy", "Chủ Nhật" };

        public static readonly string[] DayShort = { "", "T2", "T3", "T4", "T5", "T6", "T7", "CN" };

        public static readonly Dictionary<string, string> RoleNames = new()
        {
            ["Manager"] = "Quản lý trung tâm",
            ["Receptionist"] = "Nhân viên lễ tân",
            ["Coach"] = "Huấn luyện viên",
            ["Member"] = "Thành viên"
        };

        // Phương thức thanh toán: khóa lưu trong DB -> tên hiển thị
        public static readonly Dictionary<string, string> MethodNames = new()
        {
            ["Cash"] = "Tiền mặt",
            ["BankTransfer"] = "Chuyển khoản",
            ["Card"] = "Thẻ"
        };

        public static readonly List<object> MethodList =
            MethodNames.Select(kv => (object)new { Key = kv.Key, Name = kv.Value }).ToList();

        // Hành động ghi trong Lịch sử hoạt động: mã lưu trong DB -> tên hiển thị
        public static readonly Dictionary<string, string> ActionNames = new()
        {
            ["Login"] = "Đăng nhập",
            ["ChangePassword"] = "Đổi mật khẩu",
            ["CreateUser"] = "Tạo tài khoản",
            ["SetRole"] = "Đổi vai trò",
            ["ToggleUser"] = "Khóa/Mở tài khoản",
            ["CreateMember"] = "Đăng ký tại quầy",
            ["LinkMember"] = "Liên kết hồ sơ",
            ["CreatePackage"] = "Tạo gói",
            ["TogglePackage"] = "Mở/Ngừng bán gói",
            ["CreateSport"] = "Thêm bộ môn",
            ["DeleteSport"] = "Xóa bộ môn",
            ["CreateRoom"] = "Thêm phòng",
            ["DeleteRoom"] = "Xóa phòng",
            ["CreateClass"] = "Tạo lớp",
            ["CancelClass"] = "Hủy lớp/đăng ký",
            ["RegisterClass"] = "Đăng ký lớp",
            ["Attendance"] = "Điểm danh",
            ["Payment"] = "Thanh toán"
        };

        public static string ActionName(string code) =>
            ActionNames.TryGetValue(code, out var name) ? name : code;

        // 1 = Thứ Hai ... 7 = Chủ Nhật
        public static int IsoDay(DateOnly d) => ((int)d.DayOfWeek + 6) % 7 + 1;

        public static int IsoDay(DateTime d) => ((int)d.DayOfWeek + 6) % 7 + 1;

        public static string ScheduleText(int day, string slotName, TimeOnly start, TimeOnly end) =>
            $"{DayShort[day]} · {slotName} ({start:HH\\:mm}-{end:HH\\:mm})";

        public static string ScheduleText(Class c) =>
            ScheduleText(c.DayOfWeek, c.Slot.SlotName, c.Slot.StartTime, c.Slot.EndTime);

        // ---------------------------------------------------------------- Gói thành viên

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

        // ---------------------------------------------------------------- Lịch học, ca trống

        // Các cặp (thứ, ca) đã bị chiếm bởi lớp dùng cùng phòng hoặc cùng HLV
        public static HashSet<(int Day, int SlotId)> BusySet(SportsCenterContext db, int roomId, int coachId)
        {
            return db.Classes
                .Where(c => c.Status != "Cancelled" && (c.RoomId == roomId || c.CoachId == coachId))
                .Select(c => new { c.DayOfWeek, c.SlotId })
                .ToList()
                .Select(x => ((int)x.DayOfWeek, x.SlotId))
                .ToHashSet();
        }

        // Trả về thông báo nếu trùng phòng (C2) hoặc trùng HLV (C3), không trùng thì trả về null
        public static string? ClassConflictMessage(SportsCenterContext db, int roomId, int coachId, int day, int slotId)
        {
            byte d = (byte)day;

            var sameRoom = db.Classes.Include(c => c.Room).FirstOrDefault(c =>
                c.Status != "Cancelled" && c.DayOfWeek == d && c.SlotId == slotId && c.RoomId == roomId);
            if (sameRoom != null)
                return $"Phòng {sameRoom.Room.RoomName} đã có lớp {sameRoom.ClassCode} ở ca này.";

            var sameCoach = db.Classes.FirstOrDefault(c =>
                c.Status != "Cancelled" && c.DayOfWeek == d && c.SlotId == slotId && c.CoachId == coachId);
            if (sameCoach != null)
                return $"HLV đã dạy lớp {sameCoach.ClassCode} cùng khung giờ.";

            return null;
        }

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

        // ---------------------------------------------------------------- Đặt lớp

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

        // C4: lớp đang có học viên đăng ký (Registered) thì không hủy được.
        // Trả về thông báo lý do, hoặc null nếu hủy được.
        public static string? CancelClassBlockedReason(SportsCenterContext db, Class c)
        {
            int n = db.Registrations.Count(r => r.ClassId == c.ClassId && r.Status == "Registered");
            if (n == 0) return null;
            return $"Lớp {c.ClassCode} đang có {n} học viên đăng ký nên chưa thể hủy.\n\n"
                + "Hãy hủy đăng ký của các học viên (hoặc nhờ lễ tân hỗ trợ) rồi hủy lớp.";
        }

        // ---------------------------------------------------------------- Hóa đơn

        // Mã hóa đơn tiếp theo dạng INV-0001
        public static string NextInvoiceNo(SportsCenterContext db)
        {
            int n = db.Payments.Count() + 1;
            string no;
            do
            {
                no = "INV-" + n.ToString("D4");
                n++;
            } while (db.Payments.Any(p => p.InvoiceNo == no));
            return no;
        }
    }
}
