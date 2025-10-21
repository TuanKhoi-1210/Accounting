namespace Accounting.Application.DTOs
{
    // ĐƠN MUA
    public record DonMuaCreateDto(
        string SoCt,
        int NhaCungCapId,
        DateTime NgayDon,
        string? TienTe,
        decimal? TyGia,          // null = 1.0000
        bool CoHopDongLon,
        string? GhiChu
    );

    public record DonMuaDongDto(
        int VatTuId,
        string? KichThuoc,
        string? LoaiGiay,
        string? DinhLuongGsm,
        string? MauIn,
        string? GiaCong,
        decimal SoLuong,
        decimal DonGia,
        int ThueSuatId       // FK tới ThueSuat
    );

    // PHIẾU NHẬP (tạo từ đơn mua)
    public record PhieuNhapCreateDto(
        string SoCt,
        int DonMuaId,
        int NhaCungCapId,
        int KhoId,
        DateTime NgayNhap,
        string? GhiChu
    );

    // HÓA ĐƠN MUA (tạo từ đơn mua)
    public record HoaDonMuaCreateDto(
        string SoCt,
        int DonMuaId,
        int NhaCungCapId,
        DateTime NgayHoaDon,
        DateTime? HanThanhToan
    );
}
