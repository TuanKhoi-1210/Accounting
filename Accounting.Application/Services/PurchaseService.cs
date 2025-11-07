using Accounting.Application.DTOs;
using Accounting.Domain.Entities;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Application.Services
{
    public class PurchaseService
    {
        private readonly AccountingDbContext _db;
        public PurchaseService(AccountingDbContext db) => _db = db;

        // ========= ĐƠN MUA =========

        // Danh sách đơn mua (kèm dòng)
        public Task<List<DonMua>> ListDonMuaAsync() =>
            _db.DonMua
               .Include(x => x.Dong!)
               .OrderByDescending(x => x.NgayDon)
               .ToListAsync();

        // Tạo đơn mua + (tuỳ chọn) các dòng
        // Tạo đơn mua + (tuỳ chọn) các dòng
        // Tạo đơn mua + (tuỳ chọn) các dòng
        public async Task<DonMua> CreateDonMuaAsync(DonMuaCreateDto dto, IEnumerable<DonMuaDongDto>? lines = null)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.SoCt))
                throw new InvalidOperationException("Số chứng từ không được rỗng.");

            if (await _db.DonMua.AnyAsync(x => x.SoCt == dto.SoCt))
                throw new InvalidOperationException($"Số chứng từ {dto.SoCt} đã tồn tại.");

            if (!await _db.NhaCungCap.AsNoTracking().AnyAsync(x => x.Id == dto.NhaCungCapId))
                throw new InvalidOperationException($"Không tìm thấy Nhà cung cấp Id={dto.NhaCungCapId}");

            var don = new DonMua
            {
                SoCt = dto.SoCt.Trim(),
                NhaCungCapId = dto.NhaCungCapId,
                NgayDon = dto.NgayDon,                   // DTO: DateTime (không nullable)
                TienTe = string.IsNullOrWhiteSpace(dto.TienTe) ? "VND" : dto.TienTe!,
                TyGia = dto.TyGia ?? 1m,
                // đảm bảo không bao giờ null khi EF đọc lại
                TienHang = 0m,
                TienThue = 0m,
                TongTien = 0m,
                CoHopDongLon = dto.CoHopDongLon,
                GhiChu = dto.GhiChu,
                TrangThai = "nhap"
            };

            _db.DonMua.Add(don);
            await _db.SaveChangesAsync();

            if (lines != null && lines.Any())
            {
                foreach (var l in lines)
                    _db.DonMuaDong.Add(await MapDonMuaDongAsync(don.Id, l));

                await _db.SaveChangesAsync();
                await RecalcDonMuaTotalsAsync(don.Id);
            }

            return await _db.DonMua.Include(x => x.Dong).FirstAsync(x => x.Id == don.Id);
        }




        // Thêm một dòng cho đơn mua
        public async Task<DonMua> AddDongDonMuaAsync(long donMuaId, DonMuaDongDto l)
        {
            if (!await _db.DonMua.AnyAsync(x => x.Id == donMuaId))
                throw new KeyNotFoundException($"Không tìm thấy Đơn mua Id={donMuaId}");

            _db.DonMuaDong.Add(await MapDonMuaDongAsync(donMuaId, l));
            await _db.SaveChangesAsync();
            await RecalcDonMuaTotalsAsync(donMuaId);

            return await _db.DonMua.Include(x => x.Dong).FirstAsync(x => x.Id == donMuaId);
        }

        // Map + tính thuế/tiền dòng (kiểm tra dữ liệu rõ ràng)
        private async Task<DonMuaDong> MapDonMuaDongAsync(long donMuaId, DonMuaDongDto l)
        {
            if (l == null) throw new InvalidOperationException("Dòng đơn rỗng.");
            if (l.VatTuId <= 0) throw new InvalidOperationException($"VatTuId không hợp lệ: {l.VatTuId}");
            if (l.SoLuong < 0) throw new InvalidOperationException("SoLuong phải ≥ 0.");
            if (l.DonGia < 0) throw new InvalidOperationException("DonGia phải ≥ 0.");

            // kiểm tra vật tư tồn tại
            var vt = await _db.VatTu.AsNoTracking()
                     .SingleOrDefaultAsync(x => x.Id == l.VatTuId)
                     ?? throw new InvalidOperationException($"Không tìm thấy Vật tư Id={l.VatTuId}");

            // Thuế suất: DTO là int (không nullable). Nếu = 0 → coi như không áp thuế.
            long? thueSuatId = l.ThueSuatId > 0 ? l.ThueSuatId : (long?)null;
            decimal tyLeThue = 0m;
            if (thueSuatId.HasValue)
            {
                var thue = await _db.ThueSuat.AsNoTracking()
                           .SingleOrDefaultAsync(x => x.Id == thueSuatId.Value)
                           ?? throw new InvalidOperationException($"Không tìm thấy Thuế suất Id={thueSuatId.Value}");
                tyLeThue = (decimal)thue.TyLe; // giả sử TyLe là decimal/float trong DB
            }

            var soLuong = l.SoLuong;                // decimal non-null
            var donGia = l.DonGia;                 // decimal non-null
            var thanhTien = Math.Round(soLuong * donGia, 2, MidpointRounding.AwayFromZero);
            var tienThue = Math.Round(thanhTien * tyLeThue, 2, MidpointRounding.AwayFromZero);

            return new DonMuaDong
            {
                DonMuaId = donMuaId,
                VatTuId = l.VatTuId,
                KichThuoc = l.KichThuoc,
                LoaiGiay = l.LoaiGiay,
                DinhLuongGsm = l.DinhLuongGsm,
                MauIn = l.MauIn,
                GiaCong = l.GiaCong,
                SoLuong = soLuong,        // decimal
                DonGia = donGia,         // decimal
                ThueSuatId = thueSuatId,  // long? (có thể null nếu ThueSuatId==0)
                TienThue = tienThue,      // decimal? trong entity -> gán số thực
                ThanhTien = thanhTien     // decimal? trong entity -> gán số thực
            };
        }



        // Tính lại tổng đơn mua
        private async Task RecalcDonMuaTotalsAsync(long donMuaId)
        {
            var lines = await _db.DonMuaDong
                                 .Where(x => x.DonMuaId == donMuaId)
                                 .Select(x => new { x.ThanhTien, x.TienThue })
                                 .ToListAsync();

            var tienHang = lines.Sum(x => x.ThanhTien ?? 0m);
            var tienThue = lines.Sum(x => x.TienThue ?? 0m);

            var don = await _db.DonMua.FirstAsync(x => x.Id == donMuaId);
            don.TienHang = tienHang;
            don.TienThue = tienThue;
            don.TongTien = tienHang + tienThue;

            await _db.SaveChangesAsync();
        }


        // ========= PHIẾU NHẬP (tạo từ đơn mua) =========

        public async Task<PhieuNhap> CreatePhieuNhapTuDonAsync(PhieuNhapCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var don = await _db.DonMua
                               .Include(x => x.Dong!)
                               .FirstAsync(x => x.Id == dto.DonMuaId);

            var pn = new PhieuNhap
            {
                SoCt = dto.SoCt,
                DonMuaId = dto.DonMuaId,
                NhaCungCapId = dto.NhaCungCapId,
                KhoId = dto.KhoId,
                NgayNhap = dto.NgayNhap,
                GhiChu = dto.GhiChu,
                NgayTao = DateTime.Now,
                NguoiTao = "system"
            };
            _db.PhieuNhap.Add(pn);
            await _db.SaveChangesAsync(); // có Id

            // tạo chi tiết nhập theo dòng đơn
            var dongNhap = don.Dong!.Select(d => new PhieuNhapDong
            {
                PhieuNhapId = pn.Id,
                VatTuId = d.VatTuId,
                SoLo = null,
                SoLuong = d.SoLuong,
                DonGia = d.DonGia,
                GiaTri = decimal.Round(d.SoLuong * d.DonGia, 2)
            });

            _db.PhieuNhapDong.AddRange(dongNhap);

            // cập nhật trạng thái đơn (ví dụ: hoàn_thanh)
            don.TrangThai = "hoan_thanh";

            await _db.SaveChangesAsync();
            return await _db.PhieuNhap.Include(x => x.Dong).FirstAsync(x => x.Id == pn.Id);
        }

        // ========= HÓA ĐƠN MUA (tạo từ đơn mua) =========

        public async Task<HoaDonMua> CreateHoaDonMuaTuDonAsync(HoaDonMuaCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var don = await _db.DonMua
                               .Include(x => x.Dong!)
                               .FirstAsync(x => x.Id == dto.DonMuaId);

            var hd = new HoaDonMua
            {
                SoCt = dto.SoCt,
                DonMuaId = dto.DonMuaId,
                NhaCungCapId = dto.NhaCungCapId,
                NgayHoaDon = dto.NgayHoaDon,
                HanThanhToan = dto.HanThanhToan,
                TienHang = don.TienHang,
                TienThue = don.TienThue,
                TongTien = don.TongTien,
                TrangThai = "con_no",
                NgayTao = DateTime.Now,
                NguoiTao = "system"
            };
            _db.HoaDonMua.Add(hd);
            await _db.SaveChangesAsync();

            // tạo dòng hóa đơn theo dòng đơn
            var dongHd = don.Dong!.Select(d => new HoaDonMuaDong
            {
                HoaDonMuaId = hd.Id,
                VatTuId = d.VatTuId,
                SoLuong = d.SoLuong,
                DonGia = d.DonGia,
                ThueSuatId = d.ThueSuatId,
                TienThue = d.TienThue,
                ThanhTien = d.ThanhTien
            });

            _db.HoaDonMuaDong.AddRange(dongHd);
            await _db.SaveChangesAsync();

            return await _db.HoaDonMua
                            .Include(x => x.Dong)
                            .FirstAsync(x => x.Id == hd.Id);
        }
    }
}
