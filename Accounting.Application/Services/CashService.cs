using Accounting.Application.Dtos;
using Accounting.Application.DTOs;
using Accounting.Domain.Entities;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Application.Services
{
    public class CashService
    {
        private readonly AccountingDbContext _db;

        public CashService(AccountingDbContext db)
        {
            _db = db;
        }

        // ================== PHIẾU THU ==================

        // ================== PHIẾU THU ==================
        public async Task<PhieuThu> SavePhieuThuAsync(PhieuThuDto dto, string currentUser)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.NguoiNop))
                throw new InvalidOperationException("Người nộp không được để trống.");
            if (dto.SoTien <= 0)
                throw new InvalidOperationException("Số tiền phải lớn hơn 0.");

            if (dto.NgayCt == default)
                dto.NgayCt = DateTime.Today;

            if (string.IsNullOrWhiteSpace(dto.SoCt))
                dto.SoCt = await GenerateSoCtAsync("PT", isThu: true);

            dto.SoCt = dto.SoCt!.Trim();

            var soCtTrung = await _db.PhieuThu
                .AnyAsync(x => x.SoCt == dto.SoCt && x.Id != dto.Id && !x.DaXoa);

            if (soCtTrung)
                throw new InvalidOperationException($"Số chứng từ {dto.SoCt} đã tồn tại.");

            PhieuThu entity;

            if (dto.Id == 0)
            {
                // tạo mới
                entity = new PhieuThu
                {
                    SoCt = dto.SoCt,
                    NgayCt = dto.NgayCt,
                    NguoiNop = dto.NguoiNop.Trim(),
                    SoTien = dto.SoTien,
                    LyDo = dto.LyDo,
                    HoaDonBanId = dto.HoaDonBanId,   // 🔹 gán FK
                    NgayTao = DateTime.Now,
                    NguoiTao = currentUser,
                    DaXoa = false
                };
                _db.PhieuThu.Add(entity);
            }
            else
            {
                // cập nhật
                entity = await _db.PhieuThu.FirstOrDefaultAsync(x => x.Id == dto.Id && !x.DaXoa)
                    ?? throw new InvalidOperationException("Phiếu thu không tồn tại.");

                entity.SoCt = dto.SoCt;
                entity.NgayCt = dto.NgayCt;
                entity.NguoiNop = dto.NguoiNop.Trim();
                entity.SoTien = dto.SoTien;
                entity.LyDo = dto.LyDo;
                entity.HoaDonBanId = dto.HoaDonBanId;   // 🔹 update FK
                entity.NgayCapNhat = DateTime.Now;
                entity.NguoiCapNhat = currentUser;
            }

            // 🔸 CẬP NHẬT CÔNG NỢ HÓA ĐƠN (đặt ngay trước SaveChanges)
            if (dto.HoaDonBanId.HasValue)
            {
                var hd = await _db.HoaDonBan.FirstOrDefaultAsync(
                    x => x.Id == dto.HoaDonBanId && !x.DaXoa);

                if (hd != null)
                {
                    hd.SoTienDaThanhToan += dto.SoTien;
                    hd.TrangThaiCongNo = hd.SoTienDaThanhToan >= hd.TongTien
                        ? "da_tt"
                        : "con_no";
                }
            }

            await _db.SaveChangesAsync();
            return entity;
        }


        public async Task<List<PhieuThu>> ListPhieuThuAsync(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var q = _db.PhieuThu.Where(x => !x.DaXoa);

            if (tuNgay.HasValue)
            {
                var d = tuNgay.Value.Date;
                q = q.Where(x => x.NgayCt >= d);
            }

            if (denNgay.HasValue)
            {
                var d = denNgay.Value.Date;
                q = q.Where(x => x.NgayCt <= d);
            }

            return await q
                .OrderByDescending(x => x.NgayCt)
                .ThenByDescending(x => x.SoCt)
                .ToListAsync();
        }

        public async Task<PhieuThu?> GetPhieuThuAsync(long id)
        {
            return await _db.PhieuThu
                .FirstOrDefaultAsync(x => x.Id == id && !x.DaXoa);
        }

        public async Task DeletePhieuThuAsync(long id, string currentUser)
        {
            var entity = await _db.PhieuThu.FirstOrDefaultAsync(x => x.Id == id && !x.DaXoa)
                ?? throw new InvalidOperationException("Phiếu thu không tồn tại.");

            entity.DaXoa = true;
            entity.NgayCapNhat = DateTime.Now;
            entity.NguoiCapNhat = currentUser;

            await _db.SaveChangesAsync();
        }

        // ================== PHIẾU CHI ==================

        public async Task<PhieuChi> SavePhieuChiAsync(PhieuChiDto dto, string currentUser)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.NguoiNhan))
                throw new InvalidOperationException("Người nhận không được để trống.");
            if (dto.SoTien <= 0)
                throw new InvalidOperationException("Số tiền phải lớn hơn 0.");

            if (dto.NgayCt == default)
                dto.NgayCt = DateTime.Today;

            if (string.IsNullOrWhiteSpace(dto.SoCt))
                dto.SoCt = await GenerateSoCtAsync("PC", isThu: false);

            dto.SoCt = dto.SoCt!.Trim();

            var soCtTrung = await _db.PhieuChi
                .AnyAsync(x => x.SoCt == dto.SoCt && x.Id != dto.Id && !x.DaXoa);

            if (soCtTrung)
                throw new InvalidOperationException($"Số chứng từ {dto.SoCt} đã tồn tại.");

            PhieuChi entity;

            if (dto.Id == 0)
            {
                entity = new PhieuChi
                {
                    SoCt = dto.SoCt,
                    NgayCt = dto.NgayCt,
                    NguoiNhan = dto.NguoiNhan.Trim(),
                    SoTien = dto.SoTien,
                    LyDo = dto.LyDo,
                    HoaDonMuaId = dto.HoaDonMuaId,   // 🔹 gán FK
                    NgayTao = DateTime.Now,
                    NguoiTao = currentUser,
                    DaXoa = false
                };

                _db.PhieuChi.Add(entity);
            }
            else
            {
                entity = await _db.PhieuChi.FirstOrDefaultAsync(x => x.Id == dto.Id && !x.DaXoa)
                    ?? throw new InvalidOperationException("Phiếu chi không tồn tại.");

                entity.SoCt = dto.SoCt;
                entity.NgayCt = dto.NgayCt;
                entity.NguoiNhan = dto.NguoiNhan.Trim();
                entity.SoTien = dto.SoTien;
                entity.LyDo = dto.LyDo;
                entity.HoaDonMuaId = dto.HoaDonMuaId;   // 🔹 update FK
                entity.NgayCapNhat = DateTime.Now;
                entity.NguoiCapNhat = currentUser;
            }

            // 🔸 CẬP NHẬT CÔNG NỢ HÓA ĐƠN MUA
            // CẬP NHẬT CÔNG NỢ HÓA ĐƠN MUA
            if (dto.HoaDonMuaId.HasValue)
            {
                var hd = await _db.HoaDonMua
                    .FirstOrDefaultAsync(x => x.Id == dto.HoaDonMuaId);  // <-- bỏ !x.DaXoa

                if (hd != null)
                {
                    hd.SoTienDaThanhToan += dto.SoTien;
                    hd.TrangThaiCongNo = hd.SoTienDaThanhToan >= hd.TongTien
                        ? "da_tt"
                        : "con_no";
                }
            }

            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<decimal> GetSoDuQuyAsync(DateTime? denNgay = null)
        {
            var d = (denNgay ?? DateTime.Today).Date;

            var tongThu = await _db.PhieuThu
                .Where(x => !x.DaXoa && x.NgayCt <= d)
                .SumAsync(x => (decimal?)x.SoTien) ?? 0m;

            var tongChi = await _db.PhieuChi
                .Where(x => !x.DaXoa && x.NgayCt <= d)
                .SumAsync(x => (decimal?)x.SoTien) ?? 0m;

            return tongThu - tongChi;
        }
        public async Task<KiemKeQuy> SaveKiemKeQuyAsync(KiemKeQuyDto dto, string currentUser)
        {
            if (dto.NgayKk == default)
                dto.NgayKk = DateTime.Today;

            // Tính lại chênh lệch bảo đảm đúng
            dto.ChenhLech = dto.SoDuThucTe - dto.SoDuSo;

            KiemKeQuy entity;

            if (dto.Id == 0)
            {
                entity = new KiemKeQuy
                {
                    NgayKk = dto.NgayKk.Date,
                    SoDuSo = dto.SoDuSo,
                    SoDuThucTe = dto.SoDuThucTe,
                    ChenhLech = dto.ChenhLech,
                    GhiChu = dto.GhiChu,
                    NgayTao = DateTime.Now,
                    NguoiTao = currentUser,
                    DaXoa = false
                };
                _db.KiemKeQuy.Add(entity);
            }
            else
            {
                entity = await _db.KiemKeQuy.FirstOrDefaultAsync(x => x.Id == dto.Id && !x.DaXoa)
                    ?? throw new InvalidOperationException("Phiếu kiểm kê không tồn tại.");

                entity.NgayKk = dto.NgayKk.Date;
                entity.SoDuSo = dto.SoDuSo;
                entity.SoDuThucTe = dto.SoDuThucTe;
                entity.ChenhLech = dto.ChenhLech;
                entity.GhiChu = dto.GhiChu;
                entity.NgayCapNhat = DateTime.Now;
                entity.NguoiCapNhat = currentUser;
            }

            await _db.SaveChangesAsync();
            return entity;
        }


        public async Task<List<PhieuChi>> ListPhieuChiAsync(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var q = _db.PhieuChi.Where(x => !x.DaXoa);

            if (tuNgay.HasValue)
            {
                var d = tuNgay.Value.Date;
                q = q.Where(x => x.NgayCt >= d);
            }

            if (denNgay.HasValue)
            {
                var d = denNgay.Value.Date;
                q = q.Where(x => x.NgayCt <= d);
            }

            return await q
                .OrderByDescending(x => x.NgayCt)
                .ThenByDescending(x => x.SoCt)
                .ToListAsync();
        }

        public async Task<PhieuChi?> GetPhieuChiAsync(long id)
        {
            return await _db.PhieuChi
                .FirstOrDefaultAsync(x => x.Id == id && !x.DaXoa);
        }

        public async Task DeletePhieuChiAsync(long id, string currentUser)
        {
            var entity = await _db.PhieuChi.FirstOrDefaultAsync(x => x.Id == id && !x.DaXoa)
                ?? throw new InvalidOperationException("Phiếu chi không tồn tại.");

            entity.DaXoa = true;
            entity.NgayCapNhat = DateTime.Now;
            entity.NguoiCapNhat = currentUser;

            await _db.SaveChangesAsync();
        }

        // ================== SỔ QUỸ ==================

        public async Task<List<SoQuyItemDto>> GetSoQuyAsync(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var qThu = _db.PhieuThu.Where(x => !x.DaXoa);
            var qChi = _db.PhieuChi.Where(x => !x.DaXoa);

            if (tuNgay.HasValue)
            {
                var d = tuNgay.Value.Date;
                qThu = qThu.Where(x => x.NgayCt >= d);
                qChi = qChi.Where(x => x.NgayCt >= d);
            }

            if (denNgay.HasValue)
            {
                var d = denNgay.Value.Date;
                qThu = qThu.Where(x => x.NgayCt <= d);
                qChi = qChi.Where(x => x.NgayCt <= d);
            }

            var thu = await qThu.Select(x => new SoQuyItemDto
            {
                NgayCt = x.NgayCt,
                SoCt = x.SoCt,
                LoaiPhieu = "Thu tiền",
                NoiDung = x.LyDo,
                Thu = x.SoTien,
                Chi = 0
            }).ToListAsync();

            var chi = await qChi.Select(x => new SoQuyItemDto
            {
                NgayCt = x.NgayCt,
                SoCt = x.SoCt,
                LoaiPhieu = "Chi tiền",
                NoiDung = x.LyDo,
                Thu = 0,
                Chi = x.SoTien
            }).ToListAsync();

            return thu.Concat(chi)
                      .OrderBy(x => x.NgayCt)
                      .ThenBy(x => x.SoCt)
                      .ToList();
        }

        // ================== HÀM TẠO SỐ CHỨNG TỪ ==================

        private async Task<string> GenerateSoCtAsync(string prefix, bool isThu)
        {
            string? lastSo;

            if (isThu)
            {
                lastSo = await _db.PhieuThu
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.SoCt)
                    .FirstOrDefaultAsync();
            }
            else
            {
                lastSo = await _db.PhieuChi
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.SoCt)
                    .FirstOrDefaultAsync();
            }

            int next = 1;

            if (!string.IsNullOrWhiteSpace(lastSo) &&
                lastSo.StartsWith(prefix + "-", StringComparison.OrdinalIgnoreCase))
            {
                var part = lastSo[(prefix.Length + 1)..];
                if (int.TryParse(part, out var num))
                    next = num + 1;
            }

            return $"{prefix}-{next:000000}";
        }
    }
}
