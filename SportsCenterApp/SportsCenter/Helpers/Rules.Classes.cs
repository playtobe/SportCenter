using Microsoft.EntityFrameworkCore;
using SportsCenter.Entities;

namespace SportsCenter
{
    // Luật về lớp học: ca trống, trùng phòng / trùng HLV (C2, C3) và hủy lớp (C4). Chủ file: Người 3.
    // Cùng một class Rules với các file Rules*.cs khác (partial), tách ra để mỗi người sửa file riêng, không đụng nhau khi push.
    public static partial class Rules
    {
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

        // C4: lớp đang có học viên đăng ký (Registered) thì không hủy được.
        // Trả về thông báo lý do, hoặc null nếu hủy được.
        public static string? CancelClassBlockedReason(SportsCenterContext db, Class c)
        {
            int n = db.Registrations.Count(r => r.ClassId == c.ClassId && r.Status == "Registered");
            if (n == 0) return null;
            return $"Lớp {c.ClassCode} đang có {n} học viên đăng ký nên chưa thể hủy.\n\n"
                + "Hãy hủy đăng ký của các học viên (hoặc nhờ lễ tân hỗ trợ) rồi hủy lớp.";
        }
    }
}
