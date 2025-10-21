using Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Infrastructure
{
    /// <summary>
    /// DbContext cho hệ thống Kế toán In ấn Bao bì (schema: acc)
    /// </summary>
    public class AccountingDbContext : DbContext
    {
        public const string Schema = "acc";

        // Cho phép khởi tạo bằng DI/options
        public AccountingDbContext(DbContextOptions<AccountingDbContext> options) : base(options) { }

        // Cho phép khởi tạo mặc định (Form1 tạo thẳng new AccountingDbContext())
        public AccountingDbContext() { }

        // ===== DbSet Danh mục =====
        public DbSet<KhachHang> KhachHang => Set<KhachHang>();
        public DbSet<NhaCungCap> NhaCungCap => Set<NhaCungCap>();
        public DbSet<DonViTinh> DonViTinh => Set<DonViTinh>();
        public DbSet<VatTu> VatTu => Set<VatTu>();
        public DbSet<Kho> Kho => Set<Kho>();
        public DbSet<TaiKhoanNganHang> TaiKhoanNganHang => Set<TaiKhoanNganHang>();
        public DbSet<ThueSuat> ThueSuat => Set<ThueSuat>();

        // ===== Nghiệp vụ Mua hàng =====
        public DbSet<DonMua> DonMua => Set<DonMua>();
        public DbSet<DonMuaDong> DonMuaDong => Set<DonMuaDong>();
        public DbSet<PhieuNhap> PhieuNhap => Set<PhieuNhap>();
        public DbSet<PhieuNhapDong> PhieuNhapDong => Set<PhieuNhapDong>();
        public DbSet<HoaDonMua> HoaDonMua => Set<HoaDonMua>();
        public DbSet<HoaDonMuaDong> HoaDonMuaDong => Set<HoaDonMuaDong>();

        /// <summary>
        /// Kết nối mặc định tới SQL Server nếu chưa được cấu hình từ bên ngoài.
        /// Thay Server=... đúng tên instance SQL của bạn (xem trong SSMS).
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Ví dụ LocalDB:
                // optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AccountingDB;Trusted_Connection=True;TrustServerCertificate=True;");

                // Ví dụ SQL Express / SQL Server cài máy bạn:
                optionsBuilder.UseSqlServer(
                    "Server=DESKTOP-Khoi\\SQLEXPRESS;Database=AccountingDB;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.HasDefaultSchema(Schema);

            // =========================
            // KHÁCH HÀNG
            // =========================
            b.Entity<KhachHang>(e =>
            {
                e.ToTable("khach_hang");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.Ten).HasColumnName("ten").HasMaxLength(200).IsRequired();
                e.Property(x => x.MaSoThue).HasColumnName("ma_so_thue");
                e.Property(x => x.DiaChi).HasColumnName("dia_chi");
                e.Property(x => x.SoDienThoai).HasColumnName("so_dien_thoai");
                e.Property(x => x.Email).HasColumnName("email");

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat");
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });

            // =========================
            // NHÀ CUNG CẤP
            // =========================
            b.Entity<NhaCungCap>(e =>
            {
                e.ToTable("nha_cung_cap");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.Ten).HasColumnName("ten").HasMaxLength(200).IsRequired();
                e.Property(x => x.MaSoThue).HasColumnName("ma_so_thue");
                e.Property(x => x.DiaChi).HasColumnName("dia_chi");
                e.Property(x => x.SoDienThoai).HasColumnName("so_dien_thoai");
                e.Property(x => x.Email).HasColumnName("email");

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat");
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });

            // =========================
            // ĐƠN VỊ TÍNH
            // =========================
            b.Entity<DonViTinh>(e =>
            {
                e.ToTable("don_vi_tinh");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.Ten).HasColumnName("ten").HasMaxLength(50).IsRequired();
            });

            // =========================
            // VẬT TƯ
            // =========================
            b.Entity<VatTu>(e =>
            {
                e.ToTable("vat_tu");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.Ten).HasColumnName("ten").HasMaxLength(200).IsRequired();
                e.Property(x => x.KichThuoc).HasColumnName("kich_thuoc");
                e.Property(x => x.LoaiGiay).HasColumnName("loai_giay");
                e.Property(x => x.DinhLuongGsm).HasColumnName("dinh_luong_gsm");
                e.Property(x => x.MauIn).HasColumnName("mau_in");
                e.Property(x => x.GiaCong).HasColumnName("gia_cong");

                e.Property(x => x.DonViTinhId).HasColumnName("don_vi_tinh_id");
                e.HasOne<DonViTinh>().WithMany().HasForeignKey(x => x.DonViTinhId);

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat");
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });

            // =========================
            // KHO
            // =========================
            b.Entity<Kho>(e =>
            {
                e.ToTable("kho");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.Ten).HasColumnName("ten").HasMaxLength(200).IsRequired();

                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
                e.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
                e.Property(x => x.NguoiCapNhat).HasColumnName("nguoi_cap_nhat");
                e.Property(x => x.DaXoa).HasColumnName("da_xoa").HasDefaultValue(false);
            });

            // =========================
            // TÀI KHOẢN NGÂN HÀNG
            // =========================
            b.Entity<TaiKhoanNganHang>(e =>
            {
                e.ToTable("tai_khoan_ngan_hang");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ma).HasColumnName("ma").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Ma).IsUnique();

                e.Property(x => x.TenNganHang).HasColumnName("ten_ngan_hang").IsRequired();
                e.Property(x => x.SoTaiKhoan).HasColumnName("so_tai_khoan").IsRequired();
                e.Property(x => x.TienTe).HasColumnName("tien_te").HasMaxLength(10).HasDefaultValue("VND");
            });

            // =========================
            // THUẾ SUẤT
            // =========================
            b.Entity<ThueSuat>(e =>
            {
                e.ToTable("thue_suat");
                e.HasKey(x => x.Id);

                e.Property(x => x.Ten).HasColumnName("ten").IsRequired();
                e.Property(x => x.TyLe).HasColumnName("ty_le").HasPrecision(9, 4).IsRequired();
                e.Property(x => x.DangHoatDong).HasColumnName("dang_hoat_dong").HasDefaultValue(true);
            });

            // ===== ĐƠN MUA =====
            b.Entity<DonMua>(e =>
            {
                e.ToTable("don_mua");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoCt).HasColumnName("so_ct").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.SoCt).IsUnique();

                e.Property(x => x.NhaCungCapId).HasColumnName("nha_cung_cap_id");
                e.HasOne<NhaCungCap>().WithMany().HasForeignKey(x => x.NhaCungCapId);

                e.Property(x => x.NgayDon).HasColumnName("ngay_don");
                e.Property(x => x.TienTe).HasColumnName("tien_te");
                e.Property(x => x.TyGia).HasColumnName("ty_gia").HasPrecision(9, 4).HasDefaultValue(1.0000m);
                e.Property(x => x.CoHopDongLon).HasColumnName("co_hop_dong_lon").HasDefaultValue(false);
                e.Property(x => x.GhiChu).HasColumnName("ghi_chu");
                e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(20).HasDefaultValue("nhap");

                e.Property(x => x.TienHang).HasColumnName("tien_hang").HasPrecision(18, 2);
                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasPrecision(18, 2);
                e.Property(x => x.TongTien).HasColumnName("tong_tien").HasPrecision(18, 2);
            });

            // ===== ĐƠN MUA DÒNG =====
            b.Entity<DonMuaDong>(e =>
            {
                e.ToTable("don_mua_dong");
                e.HasKey(x => x.Id);

                e.Property(x => x.DonMuaId).HasColumnName("don_mua_id");
                e.HasOne<DonMua>()
                    .WithMany(d => d.Dong)              // ✅ trỏ đúng collection navigation
                    .HasForeignKey(x => x.DonMuaId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(x => x.VatTuId).HasColumnName("vat_tu_id");
                e.HasOne<VatTu>().WithMany().HasForeignKey(x => x.VatTuId);

                e.Property(x => x.KichThuoc).HasColumnName("kich_thuoc");
                e.Property(x => x.LoaiGiay).HasColumnName("loai_giay");
                e.Property(x => x.DinhLuongGsm).HasColumnName("dinh_luong_gsm");
                e.Property(x => x.MauIn).HasColumnName("mau_in");
                e.Property(x => x.GiaCong).HasColumnName("gia_cong");

                e.Property(x => x.SoLuong).HasColumnName("so_luong").HasPrecision(18, 3);
                e.Property(x => x.DonGia).HasColumnName("don_gia").HasPrecision(18, 2);

                e.Property(x => x.ThueSuatId).HasColumnName("thue_suat_id");
                e.HasOne<ThueSuat>().WithMany().HasForeignKey(x => x.ThueSuatId);

                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasPrecision(18, 2);
                e.Property(x => x.ThanhTien).HasColumnName("thanh_tien").HasPrecision(18, 2);
            });

            // ===== PHIẾU NHẬP =====
            b.Entity<PhieuNhap>(e =>
            {
                e.ToTable("phieu_nhap");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoCt).HasColumnName("so_ct").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.SoCt).IsUnique();

                e.Property(x => x.NhaCungCapId).HasColumnName("nha_cung_cap_id");
                e.HasOne<NhaCungCap>().WithMany().HasForeignKey(x => x.NhaCungCapId);

                e.Property(x => x.KhoId).HasColumnName("kho_id");
                e.HasOne<Kho>().WithMany().HasForeignKey(x => x.KhoId);

                e.Property(x => x.NgayNhap).HasColumnName("ngay_nhap");

                e.Property(x => x.DonMuaId).HasColumnName("don_mua_id");
                e.HasOne<DonMua>().WithMany().HasForeignKey(x => x.DonMuaId);

                e.Property(x => x.GhiChu).HasColumnName("ghi_chu");
                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
            });

            // ===== PHIẾU NHẬP DÒNG =====
            b.Entity<PhieuNhapDong>(e =>
            {
                e.ToTable("phieu_nhap_dong");
                e.HasKey(x => x.Id);

                e.Property(x => x.PhieuNhapId).HasColumnName("phieu_nhap_id");
                e.HasOne<PhieuNhap>()
                    .WithMany(p => p.Dong)              // ✅ trỏ đúng collection navigation
                    .HasForeignKey(x => x.PhieuNhapId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(x => x.VatTuId).HasColumnName("vat_tu_id");
                e.HasOne<VatTu>().WithMany().HasForeignKey(x => x.VatTuId);

                e.Property(x => x.SoLo).HasColumnName("so_lo");
                e.Property(x => x.SoLuong).HasColumnName("so_luong").HasPrecision(18, 3);
                e.Property(x => x.DonGia).HasColumnName("don_gia").HasPrecision(18, 2);
                e.Property(x => x.GiaTri).HasColumnName("gia_tri").HasPrecision(18, 2);
            });

            // ===== HÓA ĐƠN MUA =====
            b.Entity<HoaDonMua>(e =>
            {
                e.ToTable("hoa_don_mua");
                e.HasKey(x => x.Id);

                e.Property(x => x.SoCt).HasColumnName("so_ct").HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.SoCt).IsUnique();

                e.Property(x => x.NhaCungCapId).HasColumnName("nha_cung_cap_id");
                e.HasOne<NhaCungCap>().WithMany().HasForeignKey(x => x.NhaCungCapId);

                e.Property(x => x.NgayHoaDon).HasColumnName("ngay_hoa_don");
                e.Property(x => x.HanThanhToan).HasColumnName("han_thanh_toan");

                e.Property(x => x.DonMuaId).HasColumnName("don_mua_id");
                e.HasOne<DonMua>().WithMany().HasForeignKey(x => x.DonMuaId);

                e.Property(x => x.TienHang).HasColumnName("tien_hang").HasPrecision(18, 2);
                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasPrecision(18, 2);
                e.Property(x => x.TongTien).HasColumnName("tong_tien").HasPrecision(18, 2);
                e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(20).HasDefaultValue("con_no");
                e.Property(x => x.NgayTao).HasColumnName("ngay_tao");
                e.Property(x => x.NguoiTao).HasColumnName("nguoi_tao");
            });

            // ===== HÓA ĐƠN MUA DÒNG =====
            b.Entity<HoaDonMuaDong>(e =>
            {
                e.ToTable("hoa_don_mua_dong");
                e.HasKey(x => x.Id);

                e.Property(x => x.HoaDonMuaId).HasColumnName("hoa_don_mua_id");
                e.HasOne<HoaDonMua>()
                    .WithMany(h => h.Dong)              // ✅ trỏ đúng collection navigation
                    .HasForeignKey(x => x.HoaDonMuaId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(x => x.VatTuId).HasColumnName("vat_tu_id");
                e.HasOne<VatTu>().WithMany().HasForeignKey(x => x.VatTuId);

                e.Property(x => x.SoLuong).HasColumnName("so_luong").HasPrecision(18, 3);
                e.Property(x => x.DonGia).HasColumnName("don_gia").HasPrecision(18, 2);

                e.Property(x => x.ThueSuatId).HasColumnName("thue_suat_id");
                e.HasOne<ThueSuat>().WithMany().HasForeignKey(x => x.ThueSuatId);

                e.Property(x => x.TienThue).HasColumnName("tien_thue").HasPrecision(18, 2);
                e.Property(x => x.ThanhTien).HasColumnName("thanh_tien").HasPrecision(18, 2);
            });
        }
    }
}
