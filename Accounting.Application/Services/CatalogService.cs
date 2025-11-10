// Accounting.Application/Services/CatalogService.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Accounting.Domain.Entities;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Application.Services
{
    /// <summary>
    /// Các hàm danh mục dùng chung: Đơn vị tính, Vật tư...
    /// </summary>
    public class CatalogService
    {
        private readonly AccountingDbContext _db;
        public CatalogService(AccountingDbContext db) => _db = db;

        /// <summary>
        /// Tạo nhanh vật tư nếu chưa có (theo mã). Nếu đã tồn tại thì trả về bản ghi hiện có.
        /// dvtTen: tên ĐVT (vd: "Cái", "Kg", "Tờ"). Nếu null sẽ dùng "Cái".
        /// </summary>
        public async Task<VatTu> CreateVatTuQuickAsync(string ma, string? ten, string? dvtTen)
        {
            if (string.IsNullOrWhiteSpace(ma))
                throw new InvalidOperationException("Mã vật tư không được rỗng.");

            ma = ma.Trim();

            // Đã có thì trả về luôn
            var existed = await _db.VatTu.FirstOrDefaultAsync(x => x.Ma == ma);
            if (existed != null) return existed;

            // Đảm bảo có ĐVT
            var dvt = await EnsureDonViTinhAsync(dvtTen ?? "Cái");

            // Tạo mới
            var vt = new VatTu
            {
                Ma = ma,
                Ten = string.IsNullOrWhiteSpace(ten) ? ma : ten!.Trim(),
                DonViTinhId = dvt.Id
            };

            _db.VatTu.Add(vt);
            await _db.SaveChangesAsync();
            return vt;
        }
        public async Task<List<TaiKhoanNganHang>> ListBankAccountsAsync()
        {
            return await _db.TaiKhoanNganHang
                .OrderBy(x => x.Ma)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy hoặc tạo Đơn vị tính theo tên (so khớp không phân biệt hoa thường).
        /// Vì bảng ĐVT có cột Ma (bắt buộc & unique), sẽ tự sinh nếu chưa có.
        /// </summary>
        public async Task<DonViTinh> EnsureDonViTinhAsync(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
                ten = "Cái";
            ten = ten.Trim();

            var dvt = await _db.DonViTinh
                .FirstOrDefaultAsync(x => x.Ten.ToLower() == ten.ToLower());

            if (dvt != null) return dvt;

            // Tự sinh mã ngắn gọn, tối đa 20 ký tự
            // ví dụ: DVT_241027_1234
            var rand = Random.Shared.Next(1000, 9999);
            var ma = $"DVT_{DateTime.UtcNow:yyMMdd}_{rand}";
            if (ma.Length > 20) ma = ma[..20];

            dvt = new DonViTinh
            {
                Ma = ma,
                Ten = ten
            };

            _db.DonViTinh.Add(dvt);
            await _db.SaveChangesAsync();
            return dvt;
        }
    }
}
