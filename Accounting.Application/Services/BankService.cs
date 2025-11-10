using Accounting.Application.DTOs;
using Accounting.Domain.Entities;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Application.Services
{
    public class BankService
    {
        private readonly AccountingDbContext _db;

        public BankService(AccountingDbContext db)
        {
            _db = db;
        }

        // ===== UNC THU =====
        public async Task<PhieuThuNganHang> SavePhieuThuAsync(PhieuThuNganHangDto dto, string currentUser)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.TaiKhoanNganHangId <= 0)
                throw new InvalidOperationException("Chưa chọn tài khoản ngân hàng.");
            if (string.IsNullOrWhiteSpace(dto.NguoiNop))
                throw new InvalidOperationException("Người nộp không được để trống.");
            if (dto.SoTien <= 0)
                throw new InvalidOperationException("Số tiền phải lớn hơn 0.");

            if (dto.NgayCt == default)
                dto.NgayCt = DateTime.Today;

            if (string.IsNullOrWhiteSpace(dto.SoCt))
                dto.SoCt = await GenerateSoCtAsync("UNC-THU");

            dto.SoCt = dto.SoCt!.Trim();

            // check trùng
            var existed = await _db.PhieuThuNganHang
                .FirstOrDefaultAsync(x => x.SoCt == dto.SoCt && x.Id != dto.Id && !x.DaXoa);
            if (existed != null)
                throw new InvalidOperationException($"Số chứng từ {dto.SoCt} đã tồn tại.");

            PhieuThuNganHang entity;
            if (dto.Id == 0)
            {
                entity = new PhieuThuNganHang
                {
                    NgayTao = DateTime.Now,
                    NguoiTao = currentUser,
                    DaXoa = false
                };
                _db.PhieuThuNganHang.Add(entity);
            }
            else
            {
                entity = await _db.PhieuThuNganHang.FirstAsync(x => x.Id == dto.Id && !x.DaXoa);
            }

            entity.SoCt = dto.SoCt;
            entity.NgayCt = dto.NgayCt.Date;
            entity.TaiKhoanNganHangId = dto.TaiKhoanNganHangId;
            entity.NguoiNop = dto.NguoiNop.Trim();
            entity.SoTien = dto.SoTien;
            entity.LyDo = dto.LyDo?.Trim();
            entity.NgayCapNhat = DateTime.Now;
            entity.NguoiCapNhat = currentUser;
            entity.HoaDonBanId = dto.HoaDonBanId;

            // 🔹 Cập nhật công nợ hóa đơn bán
            if (dto.HoaDonBanId.HasValue)
            {
                var hd = await _db.HoaDonBan.FirstOrDefaultAsync(x => x.Id == dto.HoaDonBanId.Value && !x.DaXoa);
                if (hd != null)
                {
                    hd.SoTienDaThanhToan += dto.SoTien;
                    hd.TrangThaiCongNo = hd.SoTienDaThanhToan >= hd.TongTien ? "da_tt" : "con_no";
                }
            }

            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<List<PhieuThuNganHang>> ListPhieuThuAsync(
            long? taiKhoanId = null, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var q = _db.PhieuThuNganHang
                .Where(x => !x.DaXoa);

            if (taiKhoanId.HasValue && taiKhoanId.Value > 0)
                q = q.Where(x => x.TaiKhoanNganHangId == taiKhoanId.Value);

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

        public async Task<PhieuThuNganHang?> GetPhieuThuAsync(long id)
        {
            return await _db.PhieuThuNganHang
                .FirstOrDefaultAsync(x => x.Id == id && !x.DaXoa);
        }

        // ===== UNC CHI =====
        public async Task<PhieuChiNganHang> SavePhieuChiAsync(PhieuChiNganHangDto dto, string currentUser)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.TaiKhoanNganHangId <= 0)
                throw new InvalidOperationException("Chưa chọn tài khoản ngân hàng.");
            if (string.IsNullOrWhiteSpace(dto.NguoiNhan))
                throw new InvalidOperationException("Người nhận không được để trống.");
            if (dto.SoTien <= 0)
                throw new InvalidOperationException("Số tiền phải lớn hơn 0.");

            if (dto.NgayCt == default)
                dto.NgayCt = DateTime.Today;

            if (string.IsNullOrWhiteSpace(dto.SoCt))
                dto.SoCt = await GenerateSoCtAsync("UNC-CHI");

            dto.SoCt = dto.SoCt!.Trim();

            var existed = await _db.PhieuChiNganHang
                .FirstOrDefaultAsync(x => x.SoCt == dto.SoCt && x.Id != dto.Id && !x.DaXoa);
            if (existed != null)
                throw new InvalidOperationException($"Số chứng từ {dto.SoCt} đã tồn tại.");

            PhieuChiNganHang entity;
            if (dto.Id == 0)
            {
                entity = new PhieuChiNganHang
                {
                    NgayTao = DateTime.Now,
                    NguoiTao = currentUser,
                    DaXoa = false
                };
                _db.PhieuChiNganHang.Add(entity);
            }
            else
            {
                entity = await _db.PhieuChiNganHang.FirstAsync(x => x.Id == dto.Id && !x.DaXoa);
            }

            entity.SoCt = dto.SoCt;
            entity.NgayCt = dto.NgayCt.Date;
            entity.TaiKhoanNganHangId = dto.TaiKhoanNganHangId;
            entity.NguoiNhan = dto.NguoiNhan.Trim();
            entity.SoTien = dto.SoTien;
            entity.LyDo = dto.LyDo?.Trim();
            entity.NgayCapNhat = DateTime.Now;
            entity.NguoiCapNhat = currentUser;
            entity.HoaDonMuaId = dto.HoaDonMuaId;

            // 🔹 Cập nhật công nợ hóa đơn mua
            if (dto.HoaDonMuaId.HasValue)
            {
                var hd = await _db.HoaDonMua.FirstOrDefaultAsync(x => x.Id == dto.HoaDonMuaId.Value);
                if (hd != null)
                {
                    hd.SoTienDaThanhToan += dto.SoTien;
                    hd.TrangThaiCongNo = hd.SoTienDaThanhToan >= hd.TongTien ? "da_tt" : "con_no";
                }
            }

            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<List<PhieuChiNganHang>> ListPhieuChiAsync(
            long? taiKhoanId = null, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var q = _db.PhieuChiNganHang
                .Where(x => !x.DaXoa);

            if (taiKhoanId.HasValue && taiKhoanId.Value > 0)
                q = q.Where(x => x.TaiKhoanNganHangId == taiKhoanId.Value);

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

        public async Task<PhieuChiNganHang?> GetPhieuChiAsync(long id)
        {
            return await _db.PhieuChiNganHang
                .FirstOrDefaultAsync(x => x.Id == id && !x.DaXoa);
        }

        // ===== SỔ PHỤ NGÂN HÀNG =====
        public async Task<List<SoNganHangItemDto>> GetSoNganHangAsync(
            long taiKhoanId, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var qThu = _db.PhieuThuNganHang.Where(x => !x.DaXoa && x.TaiKhoanNganHangId == taiKhoanId);
            var qChi = _db.PhieuChiNganHang.Where(x => !x.DaXoa && x.TaiKhoanNganHangId == taiKhoanId);

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

            var soQuyThu = await qThu
                .Select(x => new SoNganHangItemDto
                {
                    NgayCt = x.NgayCt,
                    SoCt = x.SoCt,
                    LoaiPhieu = "UNC Thu",
                    NoiDung = x.LyDo,
                    Thu = x.SoTien,
                    Chi = 0
                }).ToListAsync();

            var soQuyChi = await qChi
                .Select(x => new SoNganHangItemDto
                {
                    NgayCt = x.NgayCt,
                    SoCt = x.SoCt,
                    LoaiPhieu = "UNC Chi",
                    NoiDung = x.LyDo,
                    Thu = 0,
                    Chi = x.SoTien
                }).ToListAsync();

            return soQuyThu
                .Concat(soQuyChi)
                .OrderBy(x => x.NgayCt)
                .ThenBy(x => x.SoCt)
                .ToList();
        }

        public async Task<decimal> GetSoDuTaiKhoanAsync(long taiKhoanId, DateTime? denNgay = null)
        {
            var d = (denNgay ?? DateTime.Today).Date;

            var tongThu = await _db.PhieuThuNganHang
                .Where(x => !x.DaXoa && x.TaiKhoanNganHangId == taiKhoanId && x.NgayCt <= d)
                .SumAsync(x => (decimal?)x.SoTien) ?? 0m;

            var tongChi = await _db.PhieuChiNganHang
                .Where(x => !x.DaXoa && x.TaiKhoanNganHangId == taiKhoanId && x.NgayCt <= d)
                .SumAsync(x => (decimal?)x.SoTien) ?? 0m;

            return tongThu - tongChi;
        }
        // ===== HÓA ĐƠN BÁN CÒN NỢ =====
        public async Task<List<HoaDonBanNoDto>> ListHoaDonBanConNoAsync()
        {
            var query =
                from h in _db.HoaDonBan
                join d in _db.DonBan on h.DonBanId equals d.Id
                join kh in _db.KhachHang on d.KhachHangId equals kh.Id
                where !h.DaXoa && !d.DaXoa && !kh.DaXoa
                      && (h.TongTien - h.SoTienDaThanhToan) > 0m
                select new HoaDonBanNoDto
                {
                    Id = h.Id,
                    SoCt = h.SoHoaDon,
                    Ngay = h.NgayHoaDon,
                    DoiTuong = kh.Ten,
                    TongTien = h.TongTien,
                    DaThanhToan = h.SoTienDaThanhToan,
                    ConNo = h.TongTien - h.SoTienDaThanhToan
                };

            return await query
                .OrderByDescending(x => x.Ngay)
                .ThenByDescending(x => x.SoCt)
                .ToListAsync();
        }


        // ===== HÓA ĐƠN MUA CÒN NỢ =====
        public async Task<List<HoaDonMuaNoDto>> ListHoaDonMuaConNoAsync()
        {
            var query =
                from h in _db.HoaDonMua
                join d in _db.DonMua on h.DonMuaId equals d.Id
                join ncc in _db.NhaCungCap on d.NhaCungCapId equals ncc.Id
                where !ncc.DaXoa
                      && (((decimal?)h.TongTien ?? 0m) - ((decimal?)h.SoTienDaThanhToan ?? 0m)) > 0m
                select new HoaDonMuaNoDto
                {
                    Id = h.Id,
                    SoCt = h.SoCt, // đúng theo property bạn đang dùng
                    Ngay = h.NgayHoaDon,
                    DoiTuong = ncc.Ten,
                    TongTien = ((decimal?)h.TongTien ?? 0m),
                    DaThanhToan = ((decimal?)h.SoTienDaThanhToan ?? 0m),
                    ConNo = ((decimal?)h.TongTien ?? 0m) - ((decimal?)h.SoTienDaThanhToan ?? 0m)
                };

            return await query
                .OrderByDescending(x => x.Ngay)
                .ThenByDescending(x => x.SoCt)
                .ToListAsync();
        }



        private async Task<string> GenerateSoCtAsync(string prefix)
        {
            var today = DateTime.Today;
            var prefixWithDate = $"{prefix}{today:yyMMdd}";

            string? lastSo = await _db.PhieuThuNganHang
                .Where(x => x.SoCt.StartsWith(prefixWithDate))
                .OrderByDescending(x => x.Id)
                .Select(x => x.SoCt)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(lastSo))
            {
                lastSo = await _db.PhieuChiNganHang
                    .Where(x => x.SoCt.StartsWith(prefixWithDate))
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.SoCt)
                    .FirstOrDefaultAsync();
            }

            int next = 1;
            if (!string.IsNullOrWhiteSpace(lastSo) &&
                lastSo.StartsWith(prefixWithDate + "-", StringComparison.OrdinalIgnoreCase))
            {
                var part = lastSo[(prefixWithDate.Length + 1)..];
                if (int.TryParse(part, out var num))
                    next = num + 1;
            }

            return $"{prefixWithDate}-{next:0000}";
        }

    }
}
