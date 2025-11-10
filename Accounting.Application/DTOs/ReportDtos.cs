namespace Accounting.Application.DTOs
{
    /// <summary>
    /// Dòng báo cáo tồn kho: theo Vật tư + Kho + ĐVT.
    /// 
    /// Mapping dữ liệu:
    /// - Từ phiếu nhập: acc.phieu_nhap / acc.phieu_nhap_dong
    /// - Từ phiếu xuất: acc.phieu_xuat / acc.phieu_xuat_dong
    /// - Vật tư: acc.vat_tu
    /// - Kho: acc.kho
    /// - Đơn vị tính: acc.don_vi_tinh
    /// </summary>
    public sealed record BaoCaoTonKhoItemDto(
        long VatTuId,
        string VatTuMa,
        string VatTuTen,
        long KhoId,
        string KhoMa,
        string KhoTen,
        long? DonViTinhId,
        string? DonViTinhMa,
        string? DonViTinhTen,
        decimal SoLuongTonDau,
        decimal GiaTriTonDau,
        decimal SoLuongNhap,
        decimal GiaTriNhap,
        decimal SoLuongXuat,
        decimal GiaTriXuat,
        decimal SoLuongTonCuoi,
        decimal GiaTriTonCuoi
    );

    /// <summary>
    /// Dòng báo cáo công nợ tổng hợp (KH hoặc NCC).
    /// 
    /// - loaiDoiTuong: "KH" = khách hàng, "NCC" = nhà cung cấp.
    /// - Dữ liệu lấy từ:
    ///   + Hóa đơn bán: acc.hoa_don_ban / acc.don_ban / acc.khach_hang
    ///   + Hóa đơn mua: acc.hoa_don_mua / acc.nha_cung_cap
    ///   + Hoặc sử dụng view: acc.view_cong_no_khach_hang, acc.view_cong_no_nha_cung_cap
    /// </summary>
    public sealed record BaoCaoCongNoTongHopItemDto(
        string LoaiDoiTuong,   // "KH" hoặc "NCC"
        long DoiTuongId,
        string DoiTuongMa,
        string DoiTuongTen,
        string? MaSoThue,
        string? DienThoai,
        decimal NoDauKy,
        decimal PhatSinhTang,   // Phát sinh tăng nợ trong kỳ (bán hàng / mua hàng)
        decimal PhatSinhGiam,   // Phát sinh giảm nợ trong kỳ (thanh toán)
        decimal NoCuoiKy
    );

    /// <summary>
    /// Báo cáo lợi nhuận theo kỳ (mức tổng hợp).
    /// 
    /// Thiết kế linh hoạt:
    /// - Cho phép nhiều kiểu kỳ: "DAY", "MONTH", "QUARTER", "YEAR"
    /// - Nhưng ở phase hiện tại bạn dùng chủ yếu theo tháng:
    ///   + KieuKy = "MONTH"
    ///   + Nam, Thang có giá trị, Quy có thể null
    /// 
    /// Nguồn dữ liệu:
    /// - Doanh thu: acc.hoa_don_ban (tong_tien, ngay_hoa_don, da_xoa = 0)
    /// - (Option) Giá vốn: có thể tính từ acc.phieu_xuat_dong nếu bạn muốn sau này
    /// - 93 loại chi phí: mô phỏng = 5% doanh thu / loại
    /// </summary>
    public sealed record BaoCaoLoiNhuanTheoKyDto(
        string KieuKy,          // "MONTH"
        int Nam,
        int? Thang,
        int? Quy,
        DateTime? TuNgay,
        DateTime? DenNgay,

        decimal DoanhThu,
        decimal GiaVon,

        decimal ChiPhiQuanLy,   // 5% doanh thu
        decimal ChiPhiBanHang,  // 5% doanh thu
        decimal ChiPhiPhatSinh, // 5% doanh thu

        decimal TongChiPhi,     // = 3 * 5% * DoanhThu
        decimal LoiNhuan        // = DoanhThu - TongChiPhi
    );

    /// <summary>
    /// (Tuỳ chọn) Chi tiết từng loại chi phí theo kỳ.
    /// 
    /// Nếu bạn muốn hiển thị breakdown 93 loại chi phí riêng,
    /// có thể dùng DTO này cho một API khác.
    /// 
    /// Mapping gợi ý:
    /// - Từ danh mục acc.loai_chi_phi (nếu bạn tạo)
    /// - Tính số tiền = DoanhThu * ti_le_doanh_thu
    /// </summary>
    public sealed record BaoCaoChiPhiTyLeTheoKyItemDto(
        string KieuKy,          // "MONTH", ...
        int Nam,
        int? Thang,
        int? Quy,
        long LoaiChiPhiId,
        string LoaiChiPhiMa,
        string LoaiChiPhiTen,
        decimal TiLeDoanhThu,   // ví dụ 0.05
        decimal DoanhThuKy,     // doanh thu của kỳ tương ứng
        decimal SoTienChiPhi    // = DoanhThuKy * TiLeDoanhThu
    );
}
