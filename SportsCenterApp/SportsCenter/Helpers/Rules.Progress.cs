using SportsCenter.Entities;

namespace SportsCenter
{
    // Luật tính số buổi học của một lớp: tổng số buổi, đã học (có mặt), nghỉ (vắng), chưa điểm danh, còn lại.
    // Cùng một class Rules với các file Rules*.cs khác (partial). Chủ file: Người 2.
    public static partial class Rules
    {
        // Đếm số buổi của lớp: mỗi tuần một buổi (đúng thứ của lớp), nằm trong StartDate..EndDate.
        // from, upTo: chỉ đếm trong khoảng này (không truyền thì đếm cả lớp).
        public static int CountSessions(Class c, DateOnly? from = null, DateOnly? upTo = null)
        {
            DateOnly first = c.StartDate;
            if (from != null && from.Value > first)
            {
                first = from.Value;
            }
            DateOnly last = c.EndDate;
            if (upTo != null && upTo.Value < last)
            {
                last = upTo.Value;
            }

            int count = 0;
            for (var d = first; d <= last; d = d.AddDays(1))
            {
                if (IsoDay(d) == c.DayOfWeek)
                {
                    count++;
                }
            }
            return count;
        }

        // Số buổi của một lần đăng ký lớp. Phải Include(Class) và Include(Attendances) trước khi gọi.
        // Total: tổng số buổi cả lớp. Present: đã học (có mặt). Absent: nghỉ (vắng).
        // NotMarked: buổi đã diễn ra từ lúc đăng ký mà chưa điểm danh. Remaining: buổi chưa diễn ra.
        public static (int Total, int Present, int Absent, int NotMarked, int Remaining) ProgressOf(Registration r)
        {
            var c = r.Class;
            int total = CountSessions(c);
            int present = r.Attendances.Count(a => a.Status == "Present");
            int absent = r.Attendances.Count(a => a.Status == "Absent");

            // Chỉ tính các buổi đã diễn ra kể từ ngày member đăng ký lớp
            int held = CountSessions(c, DateOnly.FromDateTime(r.RegisteredAt), Today);
            int notMarked = held - present - absent;
            if (notMarked < 0)
            {
                notMarked = 0;
            }

            int remaining = total - CountSessions(c, null, Today);
            return (total, present, absent, notMarked, remaining);
        }
    }
}
