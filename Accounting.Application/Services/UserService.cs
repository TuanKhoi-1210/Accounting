using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Accounting.Application.DTOs;
using Accounting.Domain.Entities;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Application.Services
{
    public class UserService
    {
        private readonly AccountingDbContext _db;

        public UserService(AccountingDbContext db)
        {
            _db = db;
        }

        // ========== Helper hash mật khẩu đơn giản ==========
        private static string HashPassword(string password)
        {
            // Đơn giản: SHA256, cậu có thể thay bằng BCrypt sau
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes); // .NET 5+
        }

        private static bool VerifyPassword(string raw, string hash)
        {
            return HashPassword(raw) == hash || raw == hash;
            // cho phép dùng plain "admin" thời gian đầu
        }

        private static UserDto Map(NguoiDung x) => new UserDto
        {
            Id = x.Id,
            TenDangNhap = x.TenDangNhap,
            HoTen = x.HoTen,
            VaiTro = x.VaiTro,
            DangHoatDong = x.DangHoatDong
        };

        // ========== LOGIN ==========
        public async Task<LoginResultDto> LoginAsync(string username, string password)
        {
            var user = await _db.NguoiDung
                .FirstOrDefaultAsync(u => u.TenDangNhap == username);

            if (user == null || !user.DangHoatDong)
            {
                return new LoginResultDto
                {
                    Success = false,
                    Error = "Tài khoản không tồn tại hoặc đã bị khóa."
                };
            }

            if (!VerifyPassword(password, user.MatKhauHash))
            {
                return new LoginResultDto
                {
                    Success = false,
                    Error = "Mật khẩu không đúng."
                };
            }

            return new LoginResultDto
            {
                Success = true,
                User = Map(user)
            };
        }

        // ========== CRUD USERS ==========

        public async Task<List<UserDto>> GetAllAsync()
        {
            return await _db.NguoiDung
                .OrderBy(u => u.TenDangNhap)
                .Select(u => Map(u))
                .ToListAsync();
        }

        public async Task<UserDto> SaveAsync(UserEditDto dto, string? currentUser)
        {
            NguoiDung entity;

            if (dto.Id.HasValue && dto.Id.Value > 0)
            {
                entity = await _db.NguoiDung.FindAsync(dto.Id.Value)
                         ?? throw new Exception("Không tìm thấy người dùng.");
            }
            else
            {
                entity = new NguoiDung
                {
                    TenDangNhap = dto.TenDangNhap,
                    NgayTao = DateTime.Now,
                    NguoiTao = currentUser
                };
                _db.NguoiDung.Add(entity);
            }

            entity.HoTen = dto.HoTen;
            entity.VaiTro = dto.VaiTro;
            entity.DangHoatDong = dto.DangHoatDong;
            entity.NgaySua = DateTime.Now;
            entity.NguoiSua = currentUser;

            if (!string.IsNullOrWhiteSpace(dto.MatKhauMoi))
            {
                entity.MatKhauHash = HashPassword(dto.MatKhauMoi);
            }

            await _db.SaveChangesAsync();
            return Map(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _db.NguoiDung.FindAsync(id);
            if (user == null) return;

            _db.NguoiDung.Remove(user);
            await _db.SaveChangesAsync();
        }
    }
}
