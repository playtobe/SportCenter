using SportsCenter.Entities;

namespace SportsCenter
{
    // Ghi lịch sử hoạt động (bảng AuditLogs)
    public static class AuditHelper
    {
        public static void Log(SportsCenterContext db, string action, string details, string? entityName = null, int? entityId = null)
        {
            db.AuditLogs.Add(new AuditLog
            {
                UserId = Session.CurrentUser?.UserId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();
        }
    }
}
