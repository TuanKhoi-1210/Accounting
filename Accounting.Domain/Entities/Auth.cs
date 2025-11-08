namespace Accounting.Domain.Entities { 

public class UserAccount
{
    public long Id { get; set; }
    public string Username { get; set; } = "";
    public string? FullName { get; set; }
    public string Role { get; set; } = "Kế toán";   // dùng role dạng chuỗi
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
}