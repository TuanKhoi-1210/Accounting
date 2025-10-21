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
        public async Task<DonMua> CreateDonMuaAsync(DonMuaCreateDto dto, IEnumerable<DonMuaDongDto>? lines = null)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // trùng số chứng từ?
            if (string.IsNullOrWhiteSpace(dto.SoCt))
                throw new InvalidOperationException("Số chứng từ không được rỗng.");

            if (await _db.DonMua.AnyAsync(x => x.SoCt == dto.SoCt))
                throw new InvalidOperationException($"Số chứng từ {dto.SoCt} đã tồn tại.");

            // tồn tại NCC?
            if (!await _db.NhaCungCap.AsNoTracking().AnyAsync(x => x.Id == dto.NhaCungCapId))
                throw new InvalidOperationException($"Không tìm thấy Nhà cung cấp Id={dto.NhaCungCapId}");

            var don = new DonMua
            {
                SoCt = dto.SoCt,
                NhaCungCapId = dto.NhaCungCapId,
                NgayDon = dto.NgayDon,
                TienTe = string.IsNullOrWhiteSpace(dto.TienTe) ? "VND" : dto.TienTe!,
                TyGia = dto.TyGia ?? 1.0000m,
                CoHopDongLon = dto.CoHopDongLon,
                GhiChu = dto.GhiChu,
                TrangThai = "nhap" // nhap/đang_thực_hiện/hoàn_thành...
            };

            _db.DonMua.Add(don);
            await _db.SaveChangesAsync(); // để có Id

            if (lines != null)
            {
                foreach (var l in lines)
                    _db.DonMuaDong.Add(await MapDonMuaDongAsync(don.Id, l));

                await _db.SaveChangesAsync();
                await RecalcDonMuaTotalsAsync(don.Id);
            }

            return await _db.DonMua
                           .Include(x => x.Dong)
                           .FirstAsync(x => x.Id == don.Id);
        }

        // Thêm một dòng cho đơn mua
        public async Task<DonMua> AddDongDonMuaAsync(int donMuaId, DonMuaDongDto l)
        {
            if (!await _db.DonMua.AnyAsync(x => x.Id == donMuaId))
                throw new KeyNotFoundException($"Không tìm thấy Đơn mua Id={donMuaId}");

            _db.DonMuaDong.Add(await MapDonMuaDongAsync(donMuaId, l));
            await _db.SaveChangesAsync();
            await RecalcDonMuaTotalsAsync(donMuaId);

            return await _db.DonMua.Include(x => x.Dong).FirstAsync(x => x.Id == donMuaId);
        }

        // Map + tính thuế/tiền dòng (kiểm tra dữ liệu rõ ràng)
        private async Task<DonMuaDong> MapDonMuaDongAsync(int donMuaId, DonMuaDongDto l)
        {
            try
            {
                if (l == null) throw new InvalidOperationException("Dòng đơn rỗng.");
                if (l.VatTuId <= 0) throw new InvalidOperationException($"VatTuId không hợp lệ: {l.VatTuId}");
                if (l.ThueSuatId <= 0) throw new InvalidOperationException($"ThueSuatId không hợp lệ: {l.ThueSuatId}");
                if (l.SoLuong <= 0) throw new InvalidOperationException("SoLuong phải > 0.");
                if (l.DonGia < 0) throw new InvalidOperationException("DonGia phải ≥ 0.");

                // Tồn tại Vật tư?
                var vt = await _db.VatTu.AsNoTracking()
                        .SingleOrDefaultAsync(x => x.Id == l.VatTuId)
                      ?? throw new InvalidOperationException($"Không tìm thấy Vật tư Id={l.VatTuId}");

                // Tồn tại Thuế suất?
                var thue = await _db.ThueSuat.AsNoTracking()
                          .SingleOrDefaultAsync(x => x.Id == l.ThueSuatId)
                        ?? throw new InvalidOperationException($"Không tìm thấy Thuế suất Id={l.ThueSuatId}");

                var thanhTien = Math.Round(l.SoLuong * l.DonGia, 2);
                var tienThue = Math.Round(thanhTien * (decimal)thue.TyLe, 2);

                return new DonMuaDong
                {
                    DonMuaId = donMuaId,
                    VatTuId = l.VatTuId,
                    KichThuoc = l.KichThuoc,
                    LoaiGiay = l.LoaiGiay,
                    DinhLuongGsm = l.DinhLuongGsm,
                    MauIn = l.MauIn,
                    GiaCong = l.GiaCong,
                    SoLuong = l.SoLuong,
                    DonGia = l.DonGia,
                    ThueSuatId = l.ThueSuatId,
                    TienThue = tienThue,
                    ThanhTien = thanhTien
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Lỗi map dòng: VatTuId={l?.VatTuId}, ThueSuatId={l?.ThueSuatId}, SL={l?.SoLuong}, DG={l?.DonGia}.",
                    ex);
            }
        }

        // Tính lại tổng đơn mua
        private async Task RecalcDonMuaTotalsAsync(int donMuaId)
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
