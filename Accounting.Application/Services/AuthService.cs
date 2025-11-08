using System.Security.Cryptography;
using System.Text;
using Accounting.Domain.Entities;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Application.Services;

public class AuthService
{
    private readonly AccountingDbContext _db;
    public AuthService(AccountingDbContext db) => _db = db;

    // ====== API đăng nhập ======
    public async Task<(UserAccount? user, string[] roles)> LoginAsync(string username, string password)
    {
        var user = await _db.Set<UserAccount>()
            .Include(u => u.Roles).ThenInclude(r => r.Role)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null) return (null, Array.Empty<string>());

        if (!Verify(password, user.PasswordSalt, user.PasswordHash))
            return (null, Array.Empty<string>());

        var roleNames = user.Roles.Select(r => r.Role!.Name).ToArray();
        return (user, roleNames);
    }

    // ====== Tạo/Cập nhật admin mặc định ======
    public async Task UpsertAdminAsync(string username, string fullName, string password)
    {
        var admin = await _db.Set<UserAccount>()
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username);

        var (salt, hash) = Hash(password);

        if (admin == null)
        {
            admin = new UserAccount
            {
                Username = username,
                FullName = fullName,
                PasswordSalt = salt,
                PasswordHash = hash,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Add(admin);
            await _db.SaveChangesAsync();
        }
        else
        {
            admin.FullName = fullName;
            admin.PasswordSalt = salt;
            admin.PasswordHash = hash;
            admin.IsActive = true;
            admin.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        var adminRoleId = await _db.Set<Role>()
            .Where(r => r.Name == "Admin")
            .Select(r => r.Id)
            .FirstAsync();

        if (!admin.Roles.Any(r => r.RoleId == adminRoleId))
        {
            _db.Add(new UserRole { UserId = admin.Id, RoleId = adminRoleId });
            await _db.SaveChangesAsync();
        }
    }

    // ====== Helpers PBKDF2 ======
    private static (byte[] salt, byte[] hash) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt,
                                             100_000, HashAlgorithmName.SHA256, 32);
        return (salt, hash);
    }

    private static bool Verify(string password, byte[] salt, byte[] hash)
    {
        var test = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt,
                                             100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(test, hash);
    }
}
