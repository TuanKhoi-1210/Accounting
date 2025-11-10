using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accounting.Application.Dtos;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Application.Services
{
    public class CongNoService
    {
        private readonly AccountingDbContext _db;

        public CongNoService(AccountingDbContext db)
        {
            _db = db;
        }

        // ===== 1) CÔNG NỢ PHẢI TRẢ – NHÀ CUNG CẤP =====
        public async Task<List<CongNoNccSummaryDto>> GetCongNoNhaCungCapAsync(bool chiConNo = false)
        {
            var query =
                from hdm in _db.HoaDonMua
                join ncc in _db.NhaCungCap on hdm.NhaCungCapId equals ncc.Id
                group new { hdm, ncc } by new { ncc.Id, ncc.Ten } into g
                select new CongNoNccSummaryDto
                {
                    NhaCungCapId = g.Key.Id,
                    TenNhaCungCap = g.Key.Ten,
                    // ép về decimal? rồi ?? 0m để tránh null
                    TongPhaiTra = g.Sum(x => (decimal?)x.hdm.TongTien) ?? 0m,
                    DaThanhToan = g.Sum(x => (decimal?)x.hdm.SoTienDaThanhToan) ?? 0m
                };

            if (chiConNo)
            {
                query = query.Where(x => x.ConNo != 0);
            }

            return await query
                .OrderBy(x => x.TenNhaCungCap)
                .ToListAsync();
        }

        // ===== 2) CÔNG NỢ PHẢI THU – KHÁCH HÀNG =====
        public async Task<List<CongNoKhSummaryDto>> GetCongNoKhachHangAsync(bool chiConNo = false)
        {
            var query =
                from hdb in _db.HoaDonBan
                join db in _db.DonBan on hdb.DonBanId equals db.Id
                join kh in _db.KhachHang on db.KhachHangId equals kh.Id
                group new { hdb, kh } by new { kh.Id, kh.Ten } into g
                select new CongNoKhSummaryDto
                {
                    KhachHangId = g.Key.Id,
                    TenKhachHang = g.Key.Ten,
                    TongPhaiThu = g.Sum(x => (decimal?)x.hdb.TongTien) ?? 0m,
                    DaThanhToan = g.Sum(x => (decimal?)x.hdb.SoTienDaThanhToan) ?? 0m
                };

            if (chiConNo)
            {
                query = query.Where(x => x.ConNo != 0);
            }

            return await query
                .OrderBy(x => x.TenKhachHang)
                .ToListAsync();
        }

        public async Task<List<NhacNoKhDto>> GetNhacNoKhachHangChiTietAsync(DateTime today)
        {
            // Lấy các hóa đơn bán còn nợ
            var query =
                from hdb in _db.HoaDonBan
                join db in _db.DonBan on hdb.DonBanId equals db.Id
                join kh in _db.KhachHang on db.KhachHangId equals kh.Id
                where hdb.TongTien > hdb.SoTienDaThanhToan
                select new
                {
                    hdb.Id,
                    hdb.SoHoaDon,
                    hdb.NgayHoaDon,
                    hdb.TongTien,
                    hdb.SoTienDaThanhToan,
                    KhachHangId = kh.Id,
                    TenKhachHang = kh.Ten
                };

            var tmp = await query.ToListAsync();
            var list = new List<NhacNoKhDto>();

            foreach (var x in tmp)
            {
                // TẠM THỜI: hạn thanh toán = ngày hóa đơn + 30 ngày
                var due = x.NgayHoaDon.AddDays(30);

                var dto = new NhacNoKhDto
                {
                    HoaDonId = x.Id,
                    KhachHangId = x.KhachHangId,
                    TenKhachHang = x.TenKhachHang,
                    SoHoaDon = x.SoHoaDon,
                    NgayHoaDon = x.NgayHoaDon,
                    HanThanhToan = due,
                    TongTien = x.TongTien,
                    DaThanhToan = x.SoTienDaThanhToan
                };

                var diff = (due.Date - today.Date).Days;
                if (diff < 0)
                {
                    dto.SoNgayQuaHan = -diff;
                    dto.TrangThaiHan = "qua_han";
                }
                else if (diff == 0)
                {
                    dto.TrangThaiHan = "den_han";
                }
                else
                {
                    dto.TrangThaiHan = "trong_han";
                }

                list.Add(dto);
            }

            return list
                .OrderByDescending(x => x.TrangThaiHan == "qua_han")
                .ThenByDescending(x => x.TrangThaiHan == "den_han")
                .ThenBy(x => x.HanThanhToan ?? x.NgayHoaDon)
                .ToList();
        }


        public async Task<KhachHangDetailDto?> GetKhachHangDetailAsync(long id)
        {
            var kh = await _db.KhachHang.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id);
            if (kh == null) return null;

            return new KhachHangDetailDto
            {
                Id = kh.Id,
                Ten = kh.Ten,
                MaSoThue = kh.MaSoThue,
                SoDienThoai = kh.SoDienThoai,
                Email = kh.Email,
                DiaChi = kh.DiaChi,
                NguoiLienHe = null
            };
        }

        public async Task UpdateThanhToanHoaDonMuaAsync(UpdateThanhToanHoaDonDto dto)
        {
            var hd = await _db.HoaDonMua.FindAsync(dto.HoaDonId);
            if (hd == null)
                throw new Exception("Không tìm thấy hóa đơn mua.");

            hd.SoTienDaThanhToan = dto.SoTienDaThanhToan;

            if (hd.SoTienDaThanhToan <= 0)
                hd.TrangThaiCongNo = "chua_tt";
            else if (hd.SoTienDaThanhToan < hd.TongTien)
                hd.TrangThaiCongNo = "con_no";
            else
                hd.TrangThaiCongNo = "da_tt";

            await _db.SaveChangesAsync();
        }

        // ===== 4) CẬP NHẬT THANH TOÁN HĐ BÁN =====
        public async Task UpdateThanhToanHoaDonBanAsync(UpdateThanhToanHoaDonDto dto)
        {
            var hd = await _db.HoaDonBan.FindAsync(dto.HoaDonId);
            if (hd == null)
                throw new Exception("Không tìm thấy hóa đơn bán.");

            hd.SoTienDaThanhToan = dto.SoTienDaThanhToan;

            if (hd.SoTienDaThanhToan <= 0)
                hd.TrangThaiCongNo = "chua_tt";
            else if (hd.SoTienDaThanhToan < hd.TongTien)
                hd.TrangThaiCongNo = "con_no";
            else
                hd.TrangThaiCongNo = "da_tt";

            await _db.SaveChangesAsync();
        }
        public async Task<NhaCungCapDetailDto?> GetNhaCungCapDetailAsync(long id)
        {
            var ncc = await _db.NhaCungCap.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ncc == null) return null;

            return new NhaCungCapDetailDto
            {
                Id = ncc.Id,
                Ten = ncc.Ten,
                MaSoThue = ncc.MaSoThue,      // đổi theo tên property thực tế
                SoDienThoai = ncc.SoDienThoai,   // đổi theo tên property thực tế
                Email = ncc.Email,
                DiaChi = ncc.DiaChi
            };
        }

        public async Task<List<NhacNoKhDto>> GetNhacNoNhaCungCapChiTietAsync(DateTime today)
        {
            // Lấy các hóa đơn mua còn nợ
            var query =
                from hdm in _db.HoaDonMua
                join dm in _db.DonMua on hdm.DonMuaId equals dm.Id
                join ncc in _db.NhaCungCap on dm.NhaCungCapId equals ncc.Id
                where hdm.TongTien > hdm.SoTienDaThanhToan
                select new
                {
                    hdm.Id,
                    hdm.SoCt,
                    hdm.NgayHoaDon,
                    hdm.TongTien,
                    hdm.SoTienDaThanhToan,
                    NhaCungCapId = ncc.Id,
                    TenNhaCungCap = ncc.Ten
                };

            var tmp = await query.ToListAsync();
            var list = new List<NhacNoKhDto>();

            foreach (var x in tmp)
            {
                // Giả sử hạn thanh toán = ngày hóa đơn + 30 ngày
                var due = x.NgayHoaDon.AddDays(30);

                var dto = new NhacNoKhDto
                {
                    HoaDonId = x.Id,
                    KhachHangId = x.NhaCungCapId,      // tái sử dụng DTO
                    TenKhachHang = x.TenNhaCungCap,    // tên NCC
                    SoHoaDon = x.SoCt,
                    NgayHoaDon = x.NgayHoaDon,
                    HanThanhToan = due,
                    TongTien = x.TongTien ?? 0m,
                    DaThanhToan = x.SoTienDaThanhToan
                };

                var diff = (due.Date - today.Date).Days;
                if (diff < 0)
                {
                    dto.SoNgayQuaHan = -diff;
                    dto.TrangThaiHan = "qua_han";
                }
                else if (diff == 0)
                {
                    dto.TrangThaiHan = "den_han";
                }
                else
                {
                    dto.TrangThaiHan = "trong_han";
                }

                list.Add(dto);
            }

            return list
                .OrderByDescending(x => x.TrangThaiHan == "qua_han")
                .ThenByDescending(x => x.TrangThaiHan == "den_han")
                .ThenBy(x => x.HanThanhToan ?? x.NgayHoaDon)
                .ToList();
        }

    }
}
