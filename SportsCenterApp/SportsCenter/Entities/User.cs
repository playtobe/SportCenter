namespace SportsCenter.Entities;

// =========================================================================
// THỰC THỂ TÀI KHOẢN NGƯỜI DÙNG (USER ENTITY)
// Bảng đại diện: Users trong CSDL SQL Server
// Người phụ trách: Người 1 (Tài khoản và Khung ứng dụng)
// =========================================================================
public partial class User
{
    // Khóa chính tự tăng (Primary Key)
    public int UserId { get; set; }

    // Tên đăng nhập duy nhất (Username: 3-30 ký tự)
    public string Username { get; set; } = null!;

    // Mật khẩu đã được băm SHA-256 (64 ký tự hex)
    public string PasswordHash { get; set; } = null!;

    // Họ và tên đầy đủ của người dùng (2-100 ký tự)
    public string FullName { get; set; } = null!;

    // Địa chỉ email (có thể null)
    public string? Email { get; set; }

    // Số điện thoại liên hệ (10-11 số, duy nhất)
    public string? Phone { get; set; }

    // Vai trò trong hệ thống: "Manager", "Receptionist", "Coach", "Member"
    public string Role { get; set; } = null!;

    // Trạng thái tài khoản: true = Đang hoạt động, false = Bị khóa
    public bool IsActive { get; set; }

    // Ngày giờ tạo tài khoản
    public DateTime CreatedAt { get; set; }

    // Quan hệ 1-N: Danh sách các nhật ký hoạt động do User này thực hiện
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    // Quan hệ 1-1: Hồ sơ huấn luyện viên (nếu Role là Coach)
    public virtual ICollection<Coach> Coaches { get; set; } = new List<Coach>();

    // Quan hệ 1-1: Hồ sơ hội viên (nếu Role là Member)
    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
