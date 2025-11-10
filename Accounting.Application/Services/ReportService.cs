using Accounting.Application.DTOs;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Accounting.Application.Services
{
    public class ReportService
    {
        private readonly AccountingDbContext _db;

        public async Task<List<BaoCaoTonKhoItemDto>> GetBaoCaoTonKhoAsync(
    DateTime tuNgay,
    DateTime denNgay,
    long? khoId)
        {
            // Chuẩn hoá ngày
            tuNgay = tuNgay.Date;
            denNgay = denNgay.Date;

            // Preload map Vật tư -> ĐVT (function trong DB không trả cột ĐVT)
            var vatTuWithDvt = await (
                from v in _db.VatTu
                join d in _db.DonViTinh on v.DonViTinhId equals d.Id into gj
                from d in gj.DefaultIfEmpty()
                select new
                {
                    v.Id,
                    v.DonViTinhId,
                    DonViTinhMa = d != null ? d.Ma : null,
                    DonViTinhTen = d != null ? d.Ten : null
                }
            ).ToDictionaryAsync(x => x.Id);

            var result = new List<BaoCaoTonKhoItemDto>();

            var conn = _db.Database.GetDbConnection();
            var shouldClose = conn.State != ConnectionState.Open;

            try
            {
                if (shouldClose)
                    await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "[acc].[sp_bao_cao_ton_kho]";
                cmd.CommandType = CommandType.StoredProcedure;

                var pTuNgay = cmd.CreateParameter();
                pTuNgay.ParameterName = "@TuNgay";
                pTuNgay.DbType = DbType.Date;
                pTuNgay.Value = tuNgay;
                cmd.Parameters.Add(pTuNgay);

                var pDenNgay = cmd.CreateParameter();
                pDenNgay.ParameterName = "@DenNgay";
                pDenNgay.DbType = DbType.Date;
                pDenNgay.Value = denNgay;
                cmd.Parameters.Add(pDenNgay);

                var pKhoId = cmd.CreateParameter();
                pKhoId.ParameterName = "@KhoId";
                pKhoId.DbType = DbType.Int64;
                pKhoId.Value = (object?)khoId ?? DBNull.Value;
                cmd.Parameters.Add(pKhoId);

                using var reader = await cmd.ExecuteReaderAsync();

                // Cache ordinal
                int ordVatTuId = reader.GetOrdinal("vat_tu_id");
                int ordVatTuMa = reader.GetOrdinal("vat_tu_ma");
                int ordVatTuTen = reader.GetOrdinal("vat_tu_ten");
                int ordKhoId = reader.GetOrdinal("kho_id");
                int ordKhoMa = reader.GetOrdinal("kho_ma");
                int ordKhoTen = reader.GetOrdinal("kho_ten");
                int ordSlTonDau = reader.GetOrdinal("sl_ton_dau");
                int ordGtTonDau = reader.GetOrdinal("gt_ton_dau");
                int ordSlNhap = reader.GetOrdinal("sl_nhap");
                int ordGtNhap = reader.GetOrdinal("gt_nhap");
                int ordSlXuat = reader.GetOrdinal("sl_xuat");
                int ordGtXuat = reader.GetOrdinal("gt_xuat");
                int ordSlTonCuoi = reader.GetOrdinal("sl_ton_cuoi");
                int ordGtTonCuoi = reader.GetOrdinal("gt_ton_cuoi");

                while (await reader.ReadAsync())
                {
                    var vatTuId = reader.GetInt64(ordVatTuId);

                    vatTuWithDvt.TryGetValue(vatTuId, out var vtInfo);

                    long? dvtId = vtInfo?.DonViTinhId;
                    string? dvtMa = vtInfo?.DonViTinhMa;
                    string? dvtTen = vtInfo?.DonViTinhTen;

                    var dto = new BaoCaoTonKhoItemDto(
                        VatTuId: vatTuId,
                        VatTuMa: reader.GetString(ordVatTuMa),
                        VatTuTen: reader.GetString(ordVatTuTen),
                        KhoId: reader.GetInt64(ordKhoId),
                        KhoMa: reader.GetString(ordKhoMa),
                        KhoTen: reader.GetString(ordKhoTen),
                        DonViTinhId: dvtId,
                        DonViTinhMa: dvtMa,
                        DonViTinhTen: dvtTen,
                        SoLuongTonDau: reader.GetDecimal(ordSlTonDau),
                        GiaTriTonDau: reader.GetDecimal(ordGtTonDau),
                        SoLuongNhap: reader.GetDecimal(ordSlNhap),
                        GiaTriNhap: reader.GetDecimal(ordGtNhap),
                        SoLuongXuat: reader.GetDecimal(ordSlXuat),
                        GiaTriXuat: reader.GetDecimal(ordGtXuat),
                        SoLuongTonCuoi: reader.GetDecimal(ordSlTonCuoi),
                        GiaTriTonCuoi: reader.GetDecimal(ordGtTonCuoi)
                    );

                    result.Add(dto);
                }
            }
            finally
            {
                if (shouldClose && conn.State == System.Data.ConnectionState.Open)
                    await conn.CloseAsync();
            }

            return result;
        }

        public async Task<List<BaoCaoCongNoTongHopItemDto>> GetBaoCaoCongNoTongHopAsync(
    DateTime? tuNgay,
    DateTime? denNgay)
        {
            tuNgay ??= new DateTime(1900, 1, 1);
            denNgay ??= DateTime.Today;

            var from = tuNgay.Value.Date;
            var to = denNgay.Value.Date;

            // Preload KH / NCC để thêm MST & SĐT
            var khMap = await _db.KhachHang
                .Where(k => !k.DaXoa)
                .ToDictionaryAsync(
                    k => k.Id,
                    k => new { k.MaSoThue, k.SoDienThoai });

            var nccMap = await _db.NhaCungCap
                .Where(n => !n.DaXoa)
                .ToDictionaryAsync(
                    n => n.Id,
                    n => new { n.MaSoThue, n.SoDienThoai });

            var result = new List<BaoCaoCongNoTongHopItemDto>();

            var conn = _db.Database.GetDbConnection();
            var shouldClose = conn.State != System.Data.ConnectionState.Open;

            try
            {
                if (shouldClose)
                    await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "[acc].[sp_bao_cao_cong_no_tong_hop]";
                cmd.CommandType = CommandType.StoredProcedure;

                var pTuNgay = cmd.CreateParameter();
                pTuNgay.ParameterName = "@TuNgay";
                pTuNgay.DbType = DbType.Date;
                pTuNgay.Value = from;
                cmd.Parameters.Add(pTuNgay);

                var pDenNgay = cmd.CreateParameter();
                pDenNgay.ParameterName = "@DenNgay";
                pDenNgay.DbType = DbType.Date;
                pDenNgay.Value = to;
                cmd.Parameters.Add(pDenNgay);

                using var reader = await cmd.ExecuteReaderAsync();

                int ordLoai = reader.GetOrdinal("loai_doi_tuong");
                int ordId = reader.GetOrdinal("doi_tuong_id");
                int ordMa = reader.GetOrdinal("doi_tuong_ma");
                int ordTen = reader.GetOrdinal("doi_tuong_ten");
                int ordNoDau = reader.GetOrdinal("no_dau");
                int ordPsTang = reader.GetOrdinal("phat_sinh_tang");
                int ordPsGiam = reader.GetOrdinal("phat_sinh_giam");
                int ordNoCuoi = reader.GetOrdinal("no_cuoi");

                while (await reader.ReadAsync())
                {
                    var loai = reader.GetString(ordLoai); // "KH" hoặc "NCC"
                    var id = reader.GetInt64(ordId);

                    string? maSoThue = null;
                    string? dienThoai = null;

                    if (loai == "KH" && khMap.TryGetValue(id, out var kh))
                    {
                        maSoThue = kh.MaSoThue;
                        dienThoai = kh.SoDienThoai;
                    }
                    else if (loai == "NCC" && nccMap.TryGetValue(id, out var ncc))
                    {
                        maSoThue = ncc.MaSoThue;
                        dienThoai = ncc.SoDienThoai;
                    }

                    var dto = new BaoCaoCongNoTongHopItemDto(
                        LoaiDoiTuong: loai,
                        DoiTuongId: id,
                        DoiTuongMa: reader.GetString(ordMa),
                        DoiTuongTen: reader.GetString(ordTen),
                        MaSoThue: maSoThue,
                        DienThoai: dienThoai,
                        NoDauKy: reader.GetDecimal(ordNoDau),
                        PhatSinhTang: reader.GetDecimal(ordPsTang),
                        PhatSinhGiam: reader.GetDecimal(ordPsGiam),
                        NoCuoiKy: reader.GetDecimal(ordNoCuoi)
                    );

                    result.Add(dto);
                }
            }
            finally
            {
                if (shouldClose && conn.State == System.Data.ConnectionState.Open)
                    await conn.CloseAsync();
            }

            return result;
        }


        public ReportService(AccountingDbContext db)
        {
            _db = db;
        }

        public async Task<List<BaoCaoLoiNhuanTheoKyDto>> GetLoiNhuanTheoThangAsync(
            DateTime tuNgay,
            DateTime denNgay)
        {
            tuNgay = tuNgay.Date;
            denNgay = denNgay.Date;

            // 3 loại chi phí: 5% doanh thu mỗi loại
            const decimal rateChiPhi = 0.05m;   // 5%
            // Giá vốn: 40% doanh thu
            const decimal rateGiaVon = 0.40m;   // 40%

            // ===== 1) DOANH THU THEO HÓA ĐƠN BÁN =====
            var doanhThuRaw = await _db.HoaDonBan
                .Where(h => !h.DaXoa
                            && h.NgayHoaDon.Date >= tuNgay
                            && h.NgayHoaDon.Date <= denNgay)
                .GroupBy(h => new { h.NgayHoaDon.Year, h.NgayHoaDon.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    DoanhThu = g.Sum(x => x.TongTien)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var list = new List<BaoCaoLoiNhuanTheoKyDto>();

            foreach (var x in doanhThuRaw)
            {
                var doanhThu = x.DoanhThu;

                // Giá vốn = 40% doanh thu
                var giaVon = doanhThu * rateGiaVon;

                // 3 loại chi phí = 5% doanh thu mỗi loại
                var cpQuanLy = doanhThu * rateChiPhi;
                var cpBanHang = doanhThu * rateChiPhi;
                var cpPhatSinh = doanhThu * rateChiPhi;

                var tongChiPhi = giaVon + cpQuanLy + cpBanHang + cpPhatSinh;
                var loiNhuan = doanhThu - tongChiPhi;

                var kyFrom = new DateTime(x.Year, x.Month, 1);
                var kyTo = kyFrom.AddMonths(1).AddDays(-1);

                list.Add(new BaoCaoLoiNhuanTheoKyDto(
                    KieuKy: "MONTH",
                    Nam: x.Year,
                    Thang: x.Month,
                    Quy: null,
                    TuNgay: kyFrom,
                    DenNgay: kyTo,
                    DoanhThu: doanhThu,
                    GiaVon: giaVon,
                    ChiPhiQuanLy: cpQuanLy,
                    ChiPhiBanHang: cpBanHang,
                    ChiPhiPhatSinh: cpPhatSinh,
                    TongChiPhi: tongChiPhi,
                    LoiNhuan: loiNhuan
                ));
            }

            return list;
        }
    }
}
