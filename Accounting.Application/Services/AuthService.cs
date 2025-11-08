using System.Security.Cryptography;
using System.Text;
using Accounting.Domain.Entities;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Application.Services
{
    public class AuthService
    {
        private readonly AccountingDbContext _db;
        public AuthService(AccountingDbContext db) => _db = db;

        // ====== Đăng nhập: trả về user + 1 role dạng chuỗi ======
        public async Task<(UserAccount? user, string[] roles)> LoginAsync(string username, string password)
        {
            var user = await _db.Set<UserAccount>()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user is null) return (null, Array.Empty<string>());

            if (!Verify(password, user.PasswordSalt, user.PasswordHash))
                return (null, Array.Empty<string>());

            var role = string.IsNullOrWhiteSpace(user.Role) ? "Kế toán" : user.Role;
            return (user, new[] { role });
        }

        // ====== Tạo/Cập nhật admin mặc định (role = "Admin") ======
        public async Task UpsertAdminAsync(string username, string fullName, string password)
        {
            var admin = await _db.Set<UserAccount>()
                                 .FirstOrDefaultAsync(u => u.Username == username);

            var (salt, hash) = Hash(password);

            if (admin == null)
            {
                admin = new UserAccount
                {
                    Username = username,
                    FullName = fullName,
                    Role = "Admin",
                    PasswordSalt = salt,
                    PasswordHash = hash,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Add(admin);
            }
            else
            {
                admin.FullName = fullName;
                admin.Role = "Admin";
                admin.PasswordSalt = salt;
                admin.PasswordHash = hash;
                admin.IsActive = true;
                admin.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        // ====== Helpers PBKDF2 ======
        private static (byte[] salt, byte[] hash) Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password ?? string.Empty),
                                                 salt, 100_000, HashAlgorithmName.SHA256, 32);
            return (salt, hash);
        }

        private static bool Verify(string password, byte[] salt, byte[] hash)
        {
            var test = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password ?? string.Empty),
                                                 salt, 100_000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(test, hash);
        }
    }
}
